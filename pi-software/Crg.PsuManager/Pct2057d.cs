using System;
using System.Device.I2c;

namespace Crg.PsuManager
{
    public class Pct2057d
    {
        private readonly I2cDevice i2c;

        public Pct2057d(I2cDevice i2c)
        {
            this.i2c = i2c;
        }

        public double Temperature
        {
            get
            {
                Span<byte> addressSpan = stackalloc byte[1] { 0x00 };
                Span<byte> dataSpan = stackalloc byte[2];
                i2c.WriteRead(addressSpan, dataSpan);

                bool positive = (dataSpan[0] & 0x80) == 0;
                int tempBits = (dataSpan[0] & 0x7F) << 3 | dataSpan[1] >> 5;

                if (positive)
                    return tempBits * 0.125;
                else
                    return -(((tempBits ^ 0x3FF) + 1) * 0.125); // 2's complement of 10-bit data: flip everything, add one, multiply by -1
            }
        }
    }
}
