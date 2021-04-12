#!/bin/bash
set -e
pushd `dirname $0`
echo Deploying CRG Madingley Router. Running in `pwd`.

# Install common packages
echo Installing apt packages...
apt update
apt install -y vim molly-guard iftop iptables-persistent mosquitto mosquitto-clients wget i2c-tools openvpn

# Set up some local users using their GitHub public keys
echo Creating CRG users...
function addUser { # $1 = username; $2 = GitHub username for keys
	grep $1 /etc/passwd || useradd -U -m -G sudo -s /bin/bash $1
	[ -d /home/$1/.ssh ] || mkdir /home/$1/.ssh && chmod 700 /home/$1/.ssh
	curl https://github.com/$2.keys > /home/$1/.ssh/authorized_keys
	chown $1:$1 /home/$1/.ssh -R
}
addUser rob rmc47
addUser mark mt104

# Allow sudo without password (since we only use SSH keys)
echo Configuring sudo access...
cp sudoers.d/10-crg-nopasswd /etc/sudoers.d/10-crg-nopasswd

# Disable password auth for SSH connections
echo Configuring SSH...
grep "PasswordAuthentication no" /etc/ssh/sshd_config || echo "PasswordAuthentication no" >> /etc/ssh/sshd_config
systemctl enable sshd

# Set up static IPs and DNS
echo Configuring networking...
systemctl disable dhcpcd # (DHCP *client* daemon)
cp interfaces.d/10-crg-router /etc/network/interfaces.d/10-crg-router
cp etc/resolv.conf /etc/resolv.conf
cp etc/hosts /etc/hosts

# Configure iptables rules
echo Configuring iptables...
cp iptables/rules.v4 /etc/iptables/rules.v4

# Configure dhcpd *then* install the package (or it gets confused about its subnets)
echo Configuring dhcpd...
cp dhcp/dhcpd.conf /etc/dhcp/dhcpd.conf
cp default/isc-dhcp-server /etc/default/isc-dhcp-server
apt install -y isc-dhcp-server

# Download and install .NET 5 SDK
echo Installing .NET 5.0 SDK...
mkdir -p /usr/share/dotnet
wget https://dot.net/v1/dotnet-install.sh -O /usr/share/dotnet/dotnet-install.sh
bash /usr/share/dotnet/dotnet-install.sh --channel 5.0 --install-dir /usr/share/dotnet
cp profile.d/10-dotnet.sh /etc/profile.d/10-dotnet.sh
source /etc/profile.d/10-dotnet.sh

# User for the .NET app
echo Configuring service user...
grep crgpsu /etc/passwd || useradd -U -m crgpsu

# systemd service for the .NET app
echo Installing systemd service...
PSUMANAGER_PATH=$(realpath ../)/pi-software/Crg.PsuManager/bin/Debug/net5.0/Crg.PsuManager.dll
cat system/crgpsu.service | envsubst > /etc/systemd/system/crgpsu.service
systemctl daemon-reload
systemctl enable crgpsu.service

# Build and deploy the .NET app
echo Building .NET app...
source ../pi-software/deploy-pi-software.sh

echo CRG Madingley Router deployment complete. Restart now.
popd
