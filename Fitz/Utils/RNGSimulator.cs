using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Fitz.Utils
{
    public static class RNGSimulator
    {
        public static int Roll(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max);
        }

        public static int Random(int min, int max)
        {
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                byte[] data = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    rng.GetBytes(data);
                    int value = BitConverter.ToInt32(data, 0);
                    value = Math.Abs(value);
                    value %= max;
                    if (value >= min && value <= max)
                    {
                        return value;
                    }
                    else
                    {
                        return Random(min, max);
                    }
                }
                return 0;
            }
        }
    }
}