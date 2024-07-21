using System;
using System.Collections.Generic;

namespace piget
{
    public class ActionAnswer
    {
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
    }
}
