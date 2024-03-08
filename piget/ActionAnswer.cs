using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace piget
{
    public class ActionAnswer
    {
        public static void WriteColorLine(Dictionary<string, ConsoleColor> dict)
        {
            foreach (var segment in dict)
            {
                ColorConsole.Write(segment.Key, segment.Value);
            }
        }
        public static void Log(string action, string text)
        {
            WriteColorLine(new Dictionary<string, ConsoleColor> {
                { $"{action}", ConsoleColor.Yellow },
                { $"{text}\r\n", Console.ForegroundColor }
            });
        }
        public static void Done()
        {
            ColorConsole.Write(" +", ConsoleColor.Green);
        }
        public static void Error()
        {
            ColorConsole.Write(" -", ConsoleColor.Red);
        }
        public static void Information()
        {
            ColorConsole.Write(" I", ConsoleColor.Blue);
        }
        private static string ReadAtBegin(string staticText)
        {
            string input = "";
            ConsoleKeyInfo key;

            int top = Console.CursorTop;
            int left = Console.CursorLeft;

            Console.Write(staticText);
            do
            {
                key = Console.ReadKey();

                if (key.Key != ConsoleKey.Enter)
                {
                    input += key.KeyChar;

                    Console.SetCursorPosition(left, top);
                    Console.Write(input + staticText);
                }
                else
                {
                    Console.SetCursorPosition(left, top);
                    Console.WriteLine(input + staticText);
                }
            } while (key.Key != ConsoleKey.Enter);

            return input;
        }
    }
}
