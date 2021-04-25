using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Threading;
using System.Threading.Tasks;

namespace Crg.PsuManager
{
    class Program
    {
        static void Main(string[] args)
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

            Timer telemtryTimer = new Timer(_ =>
            {
                telemetry.PublishTelemetry();
                powerManager.EnsurePower();
            }, null, 0, 10 * 1000);
        }
    }
}
