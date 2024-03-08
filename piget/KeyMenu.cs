using System;
using System.Collections.Generic;

namespace piget
{
    public class ConsoleMenu
    {
        public string DefaultPrefix = null;
        public string FinalPrefix = null;
        public string ActivePrefix = null;
        public string NotActivePrefix = null;
        public bool HideMenuAfterSuccessfulSelection = false;
        public List<string> AnswerOptions = new List<string>();
        public ConsoleColor ActiveMenuItemForeColor = ConsoleColor.Black;
        public ConsoleColor ActiveMenuItemBackColor = ConsoleColor.Gray;
        public ConsoleColor NotActiveMenuItemForeColor = ConsoleColor.Gray;
        public ConsoleColor NotActiveMenuItemBackColor = ConsoleColor.Black;

        private int SelectedItemIndex = 0;

        public int GetAnswer()
        {
            int reservePosTop = Console.CursorTop;
            int reservePosLeft = Console.CursorLeft;

            while (true)
            {
                Console.CursorVisible = false;

                Console.SetCursorPosition(reservePosLeft, reservePosTop);

                for (int i = 0; i <= AnswerOptions.Count - 1; i++)
                {
                    if (i == SelectedItemIndex)
                    {
                        ColorConsole.WriteLine(
                            DefaultPrefix + ActivePrefix + AnswerOptions[i] + FinalPrefix, ActiveMenuItemForeColor, ActiveMenuItemBackColor
                        );
                    }
                    else
                    {
                        ColorConsole.WriteLine(
                            DefaultPrefix + NotActivePrefix + AnswerOptions[i] + FinalPrefix, NotActiveMenuItemForeColor, NotActiveMenuItemBackColor
                        );
                    }
                }

                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.UpArrow:
                        {
                            if (SelectedItemIndex - 1 >= 0) SelectedItemIndex--;
                        }
                        break;

                    case ConsoleKey.DownArrow:
                        {
                            if (SelectedItemIndex + 1 < AnswerOptions.Count) SelectedItemIndex++;
                        }
                        break;

                    case ConsoleKey.Enter:
                        {
                            Console.SetCursorPosition(reservePosLeft, reservePosTop);

                            for (int i = 0; i <= AnswerOptions.Count - 1; i++)
                            {
                                ColorConsole.WriteLine(AnswerOptions[i], Console.BackgroundColor, Console.BackgroundColor);
                            }

                            Console.SetCursorPosition(reservePosLeft, reservePosTop);

                            Console.CursorVisible = true;

                            return SelectedItemIndex;
                        }
                }
            }
        }
    }
}