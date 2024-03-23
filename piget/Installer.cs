using piget.Api;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace piget
{
    public class Installer
    {
        public static void Run(string directory, LocalLibrariesManager localLibrariesManager)
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            bool isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);

            string InstallWord = "Установка";

            if (!isElevated)
            {
                Console.CursorVisible = false;
                Console.Title = "Установка PIGET";

                Console.Clear();
                ColorConsole.WriteLine($"\r\n   Установка невозможна без прав администратора\r\n", ConsoleColor.Red);
                switch (new ConsoleMenu
                {
                    HideMenuAfterSuccessfulSelection = true,
                    DefaultPrefix = "   ",
                    FinalPrefix = "   ",

                    AnswerOptions = {
                                "Перезапустить с правами администратора",
                                "Отмена                                "
                            }
                }.GetAnswer())
                {
                    case 0:
                        {
                            Console.Clear();

                            ProcessStartInfo processInfo = new ProcessStartInfo();
                            processInfo.Verb = "runas";
                            processInfo.FileName = Assembly.GetExecutingAssembly().Location;
                            processInfo.Arguments = "install";
                            try
                            {
                                Process.Start(processInfo);
                            }
                            catch (Win32Exception)
                            {

                            }

                            Environment.Exit(0);
                            return;
                        }
                    case 1:
                        {
                            Environment.Exit(0);
                        }
                        break;
                }
            }

            if (Regex.IsMatch(Environment.UserName, "[^a-zA-Z0-9]"))
            {
                Console.Clear();
                ColorConsole.WriteLine($"\r\n   Имя пользователя нарушает стандарты именования PIGET. Некоторые сценарии установки могут работать некорректно.\r\n", ConsoleColor.Red);
                switch (new ConsoleMenu
                {
                    HideMenuAfterSuccessfulSelection = true,
                    DefaultPrefix = "   ",
                    FinalPrefix = "   ",

                    AnswerOptions = {
                                "Продолжить установку                  ",
                                "Выход                                 "
                            }
                }.GetAnswer())
                {
                    case 1:
                        {
                            Environment.Exit(0);
                        }
                        break;
                }
            }

            Console.Clear();
            ColorConsole.WriteLine($"\r\n   {InstallWord} PIGET{(InstallWord == "Обновление" ? " до версии" : "")} {Meta.Version}\r\n", ConsoleColor.Gray);
            switch (new ConsoleMenu
            {

                HideMenuAfterSuccessfulSelection = true,
                DefaultPrefix = "   ",
                FinalPrefix = "   ",

                AnswerOptions = {
                                "Продолжить",
                                "Отмена    "
                            }

            }.GetAnswer())
            {
                case 1:
                    {
                        Environment.Exit(0);
                    }
                    break;
            }

            Console.Clear();

            Console.WriteLine(Meta.Logo);

            ActionAnswer.Log("Создание окружения -> ", "Копирование исполняемого файла");
            File.Copy(Assembly.GetExecutingAssembly().Location, Path.Combine(directory, "piget.exe"), true);

            ActionAnswer.Log("Создание окружения -> ", "Создание переменных сред");
            Reg(directory);

            ActionAnswer.Log("Стандартная настройка -> ", "Создание конфигураци");
            // TODO: Создание файла настроек

            ActionAnswer.Log("Стандартная настройка -> ", "Подключение к стандартной библиотеке PIGET");
            localLibrariesManager.Add(Updater.PigetLibraryStd);
            localLibrariesManager.CheckAllResources();

            ActionAnswer.Log("Завершение -> ", "Очистка временных файлов");
            // TODO: Очистка кэша

            Thread.Sleep(1000);

            ColorConsole.WriteLine($"\r\n ---> Установка завершена", ConsoleColor.Green);
        }

        public static void Reg(string directory)
        {
            string EnvPath = System.Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine) ?? string.Empty;
            if (!string.IsNullOrEmpty(EnvPath) && !EnvPath.EndsWith(";"))
                EnvPath += ';';

            if (!EnvPath.Contains(directory))
            {
                EnvPath += directory;
                Environment.SetEnvironmentVariable("PATH", EnvPath, EnvironmentVariableTarget.Machine);
            }
        }
    }
}
