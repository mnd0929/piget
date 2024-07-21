using System;
using System.Linq;

namespace QislEngine.Utils
{
    public static class Convert
    {
        public static string SizeToStringFormat(double size)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size = size / 1024;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            return String.Format("{0:0.##} {1}", size, sizes[order]);
        }

        public static string ConvertStringArrayToString(string[] array)
        {
            string result = null;

            array.ToList().ForEach(x => result += $"{x} ");

            return result;
        }
    }
}
