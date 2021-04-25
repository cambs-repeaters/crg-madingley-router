using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Systemd;
using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Threading;
using System.Threading.Tasks;

namespace Crg.PsuManager
{
    class Program : BackgroundService
    {
        public static void Main(string[] args)
        {
            if (SystemdHelpers.IsSystemdService())
                CreateHostBuilder(args).Build().Run();
            else
                new Program().ExecuteAsync(new CancellationToken()).Wait();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) => 
            Host.CreateDefaultBuilder(args)
            .UseSystemd()
            .ConfigureServices((hostContext, services) => 
            {
                services.AddHostedService<Program>();
            });

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            GpioController gpio = new GpioController();
            I2cBus i2c = I2cBus.Create(1);

            PowerManager powerManager = new PowerManager(gpio);
            powerManager.SwitchPower = true;
            powerManager.PoePower = true;

            AdcManager adc = new AdcManager(i2c);
            TemperatureManager temp = new TemperatureManager(i2c);
            
            Console.WriteLine("On-board temperature: {0:0.00}", temp.OnBoardTemperature);

            Console.WriteLine("Supply voltage: {0:0.00}", adc.MainSupplyVoltage);
            Console.WriteLine("PoE voltage: {0:0.00}", adc.PoeVoltage);
            Console.WriteLine("Switch voltage: {0:0.00}", adc.SwitchVoltage);
            Console.WriteLine("Pi voltage: {0:0.00}", adc.PiVoltage);
            Console.WriteLine("Battery voltage: {0:0.00}", adc.BatteryVoltage);

            TelemetryManager telemetry = new TelemetryManager { PowerManager = powerManager, AdcManager = adc, TemperatureManager = temp };

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    telemetry.PublishTelemetry();
                    powerManager.EnsurePower();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Exception in timer: {0}", ex.ToString());
                    throw; // This will cause the service to bail out, but Systemd will restart it cleanly...
                }

                await Task.Delay(10 * 1000, stoppingToken);
            }
        }
    }
}
