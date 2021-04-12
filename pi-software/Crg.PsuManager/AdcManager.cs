using System.Device.I2c;

namespace Crg.PsuManager
{
    public class AdcManager
    {
        private const int MAIN_ADC_ADDRESS = 0x49;
        private const int FLOATING_ADC_ADDRESS = 0x48;

        private const double ADC_REFERENCE_VOLTAGE = 3.0;

        private const double VIN_VOLTAGE_MULTIPLIER = 11.0; // (10+100)/10
        private const double SWITCH_VOLTAGE_MULTIPLIER = 5.7; // (10+47)/10
        private const double POE_VOLTAGE_MULTIPLIER = 11.0; // (10+100)/10
        private const double PI_VOLTAGE_MULTIPLIER = 2.0; // (10+10)/10

        private const double FLOATING_ADC_VOLTAGE_MULTIPLIER = 11.0; // (10+100)/10

        public I2cBus i2c;
        Ads7924 mainAdc;
        Ads7924 floatingAdc;

        public AdcManager(I2cBus i2c)
        {
            this.i2c = i2c;
            mainAdc = new Ads7924(i2c.CreateDevice(MAIN_ADC_ADDRESS));
            floatingAdc = new Ads7924(i2c.CreateDevice(FLOATING_ADC_ADDRESS));
        }

        public double MainSupplyVoltage
        {
            get
            {
                double rawValue = mainAdc.ReadChannel(2);
                return rawValue * ADC_REFERENCE_VOLTAGE * VIN_VOLTAGE_MULTIPLIER;
            }
        }

        public double PoeVoltage
        {
            get
            {
                double rawValue = mainAdc.ReadChannel(1);
                return rawValue * ADC_REFERENCE_VOLTAGE * POE_VOLTAGE_MULTIPLIER;
            }
        }

        public double SwitchVoltage
        {
            get
            {
                double rawValue = mainAdc.ReadChannel(0);
                return rawValue * ADC_REFERENCE_VOLTAGE * SWITCH_VOLTAGE_MULTIPLIER;
            }
        }

        public double PiVoltage
        {
            get
            {
                double rawValue = mainAdc.ReadChannel(0);
                return rawValue * ADC_REFERENCE_VOLTAGE * PI_VOLTAGE_MULTIPLIER;
            }
        }

        public double BatteryVoltage
        {
            get
            {
                double rawValue = floatingAdc.ReadChannel(0);
                return rawValue * ADC_REFERENCE_VOLTAGE * FLOATING_ADC_VOLTAGE_MULTIPLIER;
            }
        }
    }
}
