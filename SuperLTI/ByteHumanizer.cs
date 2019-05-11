using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperLTI
{
    static class ByteHumanizer
    {
        public static string ByteHumanize(this double Bytes)
        {
            return Converter(Bytes);
        }
        public static string ByteHumanize(this long Bytes)
        {
            return Converter(Bytes);
        }
        private static double Round(this double Bytes)
        {
            return Math.Round(Bytes, 2);
        }
        private static string Converter(double Bytes)
        {
            if (Bytes >= (1024 * 1024 * 1024)) //Greater than Gibibyte
            {
                return (Bytes / 1024 / 1024 / 1024).Round() + " Gigabytes";
            }
            else if (Bytes >= (1024 * 1024)) //Greater than Mebibyte
            {
                return (Bytes / 1024 / 1024).Round() + " Megabytes";
            }
            else if (Bytes < (1024 * 1024)) //Less than Mebibyte
            {
                return (Bytes / 1024).Round() + " Kilobytes";
            }
            else
            {
                return Bytes + " Bytes";
            }
        }
    }
}
