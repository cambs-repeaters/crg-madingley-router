using System.Device.Gpio;

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
    }
}
