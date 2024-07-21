using System;
using System.Linq;
using System.Net;

namespace QislEngine.Utils
{
    public static class ProgressIndicator
    {
        public static void WriteDecompressingProgressLine(int segmentsCount, int currentCount, int targetCount)
        {
            int progress = GetPercent(targetCount, currentCount);

            Console.Write($"{BuildProgressLine(progress, segmentsCount)} {progress}% ({currentCount} / {targetCount})\t");
        }

        public static void WriteDownloadingProgressLine(int percent, int segmentsCount, DownloadProgressChangedEventArgs e)
        {
            string size = e == null ? null :
                $"{Convert.SizeToStringFormat(e.BytesReceived)} / {Convert.SizeToStringFormat(e.TotalBytesToReceive)}";

            Console.Write($"{BuildProgressLine(percent, segmentsCount)} {size}        ");
        }

        public static void WriteProgressLine(int percent, int segmentsCount) =>
            Console.Write($"{BuildProgressLine(percent, segmentsCount)} {percent}%        ");

        private static string BuildProgressLine(int percent, int segmentsCount)
        {
            int currentSegmentsCount = percent / (100 / segmentsCount);
            int currentHideSegmentsCount = segmentsCount - currentSegmentsCount;

            return $"\r  {string.Concat(Enumerable.Repeat("█", currentSegmentsCount))}{string.Concat(Enumerable.Repeat("░", currentHideSegmentsCount))}";
        }

        public static Int32 GetPercent(Int32 b, Int32 a)
        {
            if (b == 0) return 0;

            return (Int32)(a / (b / 100M));
        }
    }
}
