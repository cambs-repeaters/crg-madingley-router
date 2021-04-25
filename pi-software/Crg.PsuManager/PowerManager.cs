using System;
using System.Device.Gpio;
using System.Threading;

namespace Crg.PsuManager
{
    public class PowerManager
    {
        private const int EN_SWITCH_BAR = 7;
        private const int FAULT_SWITCH_BAR = 8;
        private const int PGOOD_SWITCH = 12;

        private const int EN_POE_BAR = 20;
        private const int FAULT_POE_BAR = 16;
        private const int PGOOD_POE = 21;

        private const int PGOOD_5V = 25;

        private const int POWER_RESET_ATTEMPTS = 3;
        private int switchPowerResetAttempts = 0;
        private int poePowerResetAttempts = 0;

        private readonly GpioController gpio;

        public PowerManager(GpioController gpio)
        {
            this.gpio = gpio;

            gpio.OpenPin(EN_SWITCH_BAR, PinMode.Output);
            gpio.OpenPin(FAULT_SWITCH_BAR, PinMode.Input);
            gpio.OpenPin(PGOOD_SWITCH, PinMode.Input);

            gpio.OpenPin(EN_POE_BAR, PinMode.Output);
            gpio.OpenPin(FAULT_POE_BAR, PinMode.Input);
            gpio.OpenPin(PGOOD_POE, PinMode.Input);

            gpio.OpenPin(PGOOD_5V, PinMode.Input);
        }

        public bool SwitchPower
        {
            get { return gpio.Read(PGOOD_SWITCH) == PinValue.High; }
            set { gpio.Write(EN_SWITCH_BAR, value ? PinValue.Low : PinValue.High); }
        }

        public bool PoePower
        {
            get { return gpio.Read(PGOOD_POE) == PinValue.High; }
            set { gpio.Write(EN_POE_BAR, value ? PinValue.Low : PinValue.High); }
        }

        public bool PowerGood
        {
            get
            {
                return (gpio.Read(PGOOD_5V) == PinValue.High) 
                    && (gpio.Read(PGOOD_SWITCH) == PinValue.High) 
                    && (gpio.Read(PGOOD_POE) == PinValue.High);
            }
        }

        public void EnsurePower()
        {
            if (!SwitchPower)
            {
                Console.Error.WriteLine("Switch power not good");
                SwitchPower = false;
                if (switchPowerResetAttempts < POWER_RESET_ATTEMPTS)
                {
                    switchPowerResetAttempts++;
                    Console.WriteLine("Resetting switch power in 5s - attempt {0}", switchPowerResetAttempts);
                    Thread.Sleep(5000);
                    SwitchPower = true;
                }
                else
                {
                    Console.WriteLine("Switch power reset attempts exceeded threshold - not resetting");
                }
            }
            if (!PoePower)
            {
                Console.Error.WriteLine("PoE power not good");
                PoePower = false;
                if (poePowerResetAttempts < POWER_RESET_ATTEMPTS)
                {
                    poePowerResetAttempts++;
                    Console.WriteLine("Resetting PoE power in 5s - attempt {0}", poePowerResetAttempts);
                    Thread.Sleep(5000);
                    PoePower = true;
                }
                else
                {
                    Console.WriteLine("PoE power reset attempts exceeded threshold - not resetting");
                }
            }
        }
    }
}
