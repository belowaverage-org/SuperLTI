using System;

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
            if (Bytes >= (1024 * 1024 * 1024))
            {
                return (Bytes / 1024 / 1024 / 1024).Round() + " Gigabytes";
            }
            else if (Bytes >= (1024 * 1024))
            {
                return (Bytes / 1024 / 1024).Round() + " Megabytes";
            }
            else if (Bytes < (1024 * 1024))
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
