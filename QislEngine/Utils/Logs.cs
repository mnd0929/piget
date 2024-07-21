using QislEngine.Helpers;
using System;
using System.Collections.Generic;

namespace QislEngine.Utils
{
    public static class Logs
    {
        public static void Log(string action, string text)
        {
            LogCustom(new Dictionary<string, ConsoleColor> {
                    { $"{action}", ConsoleColor.Yellow },
                    { $"{text}\r\n", Console.ForegroundColor }
                });
        }
        public static void LogError(string title, Exception ex)
        {
            ColorConsole.WriteLine($"{title}\r\n", ConsoleColor.Red);
            ColorConsole.WriteLine($"{ex}\r\n", ConsoleColor.DarkGray);
        }
        public static void LogCustom(Dictionary<string, ConsoleColor> dict)
        {
            foreach (var segment in dict)
            {
                ColorConsole.Write(segment.Key, segment.Value);
            }
        }
    }
}
