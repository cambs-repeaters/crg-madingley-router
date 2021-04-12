using System.Device.I2c;

namespace Crg.PsuManager
{
    public class TemperatureManager
    {
        private const int ON_BOARD_TEMPERATURE_ADDRESS = 0x71;

        I2cBus i2c;
        Pct2057d onBoardTemperature;

        public TemperatureManager(I2cBus i2c)
        {
            this.i2c = i2c;
            onBoardTemperature = new Pct2057d(i2c.CreateDevice(ON_BOARD_TEMPERATURE_ADDRESS));
        }

        public double OnBoardTemperature
        {
            get { return onBoardTemperature.Temperature; }
        }
    }
}
