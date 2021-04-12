using System;
using System.Device.I2c;
using System.IO;

namespace Crg.PsuManager
{
    public class Ads7924
    {
        private readonly I2cDevice i2c;

        public Ads7924(I2cDevice i2c)
        {
            this.i2c = i2c;
        }

        private void WriteConfig()
        {
            // 0x00: MODECNTL:
            //  MODE[5:0]: 111111= Auto-Burst Scan with Sleepmode
            //  SEL/ID[1:0]: 00 Channel 0 is selected
            WriteRegister(0x00, 0b11111100);

            // 0x13: SLPCONFIG:
            //  7: Always write 0
            //  6: CONVCTRL: 0 = conversions continue
            //  5: SLPDIV4: 0 = sleep time divider is 1
            //  4: SLPMULT8: 0 = sleep time multiplier is 1
            //  3: Always write 0
            //  2-0: SLPTIME: 111 = 320ms
            WriteRegister(0x13, 0b00000111);

            // 0x14: ACQCONFIG:
            //  7-5: Always write 0
            //  4-0: ACQTIME: 0b11111 = 31 * 2uS + 6uS = 68uS
            WriteRegister(0x14, 0b00011111);
        }

        public double ReadChannel(int channel)
        {
            try
            {
                WriteConfig();
                byte msb = ReadRegister((byte)(0x02 + channel * 2));
                byte lsb = ReadRegister((byte)(0x02 + channel * 2 + 1));
                return (double)(msb << 4 | lsb >> 4) / 0xFFF;
            }
            catch (IOException)
            {
                return 0;
            }
        }

        private void WriteRegister(byte address, byte val)
        {
            Span<byte> bytes = stackalloc byte[2] { address, val };
            i2c.Write(bytes);
        }
        
        private byte ReadRegister(byte address)
        {
            Span<byte> addressSpan = stackalloc byte[1] { address };
            Span<byte> dataSpan = stackalloc byte[1];
            i2c.WriteRead(addressSpan, dataSpan);
            return dataSpan[0];
        }
    }
}
