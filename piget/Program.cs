using piget.Api;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace piget
{
    internal class Program
    {
        public const string Logo = "\r\n ______     _____ _____ _____ ______ _______ \r\n \\ \\ \\ \\   |  __ \\_   _/ ____|  ____|__   __|\r\n  \\ \\ \\ \\  | |__) || || |  __| |__     | |   \r\n   > > > > |  ___/ | || | |_ |  __|    | |   \r\n  / / / /  | |    _| || |__| | |____   | |   \r\n /_/_/_/   |_|   |_____\\_____|______|  |_|  \r\n";

        public const string Version = "1.2";

        public static LocalLibrariesManager localLibrariesManager = new LocalLibrariesManager();

        public static string[] publicArgs = { };

        static void Main(string[] args)
        {
            publicArgs = args;

            if (args.Count() == 0)
            {
                if (!File.Exists(Path.Combine(localLibrariesManager.PigetDirectory, "piget.exe")))
                    Installer.Run(localLibrariesManager.PigetDirectory, localLibrariesManager);

                return;
            }

            try
            {
                StartArg(args);
            }
            catch (Exception ex)
            {
                ColorConsole.WriteLine("Не удалось выполнить операцию\r\n", ConsoleColor.Red);
                ColorConsole.WriteLine($"{ex}\r\n", ConsoleColor.DarkGray);
            }
        }

        public static void StartArg(string[] args)
        {
            switch (args[0])
            {
                default:
                    Console.WriteLine(GetHelpPage());
                    break;

                case "library":
                    switch (args[1])
                    {
                        case "connect":
                            ActionAnswer.Log("<!> ", "Передача команды API");
                            {
                                localLibrariesManager.Add(args[2]);
                            }
                            ActionAnswer.Log("<!> ", "Операция завершена без ошибок");
                            break;

                        case "disconnect":
                            ActionAnswer.Log("<!> ", "Передача команды API");
                            {
                                localLibrariesManager.Remove(localLibrariesManager.GetLibraryByName(args[2]));
                            }
                            ActionAnswer.Log("<!> ", "Операция завершена без ошибок");
                            break;

                        case "list":
                            {
                                List<PigetScript> Scripts = localLibrariesManager.GetLibraryByName(args[2]).GetScripts();
                                for (int i = 0; i < Scripts.Count; i++)
                                {
                                    PigetScript pigetScript = Scripts[i];

                                    Console.WriteLine($"{i + 1}: {pigetScript.Name}");
                                }
                            }
                            break;

                        case "update":
                            ActionAnswer.Log("<!> ", "Получение компонентов");
                            {
                                localLibrariesManager.GetLibraryByName(args[2]).Update();
                            }
                            ActionAnswer.Log("<!> ", "Библиотеки обновлены");
                            break;
                    }
                    break;

                case "libraries":
                    switch (args[1])
                    {
                        case "disconnect":
                            ActionAnswer.Log("<!> ", "Передача команды API с высоким приоритетом");
                            {
                                localLibrariesManager.RemoveAll();
                            }
                            ActionAnswer.Log("<!> ", "Операция завершена без ошибок");
                            break;

                        case "list":
                            {
                                List<PigetScriptLibrary> pigetScriptLibraries = localLibrariesManager.GetLibraries();
                                for (int i = 0; i < pigetScriptLibraries.Count; i++)
                                {
                                    PigetScriptLibrary pigetScriptLibrary = pigetScriptLibraries[i];

                                    Console.WriteLine(
                                        $"{i + 1} ---\r\n" +
                                        $"    Имя: {pigetScriptLibrary.Name}\r\n" +
                                        $"    Описание: {pigetScriptLibrary.Description}\r\n" +
                                        $"    Хэш: {HashManager.GetScriptLibraryHash(pigetScriptLibrary)}\r\n" +
                                        $"    Колличество пакетов: {pigetScriptLibrary.GetScripts().Count}");
                                }
                            }
                            break;

                        case "update":
                            ActionAnswer.Log("<!> ", "Запуск операции обновления библиотек и их источников");
                            {
                                localLibrariesManager.UpdateAll();
                            }
                            ActionAnswer.Log("<!> ", "Библиотеки обновлены");
                            break;
                    }
                    break;

                case "check":
                    switch (args[1])
                    {
                        case "resources":
                            {
                                ActionAnswer.Log("<!> ", "Запуск операции проверки ресурсов");
                                {
                                    localLibrariesManager.CheckAllResources();
                                }
                                ActionAnswer.Log("<!> ", "Операция завершена");
                            }
                            break;
                    }
                    break;

                case "update":
                    ActionAnswer.Log("<!> ", "Поиск обновлений");
                    {
                        KeyValuePair<bool, string> res = Updater.CheckUpdates();
                        if (res.Key)
                        {
                            ActionAnswer.Log("<!> ", "Установлена последняя версия");
                        }
                        else
                        {
                            ActionAnswer.Log("<?> ", $"Найдена новая версия: {res.Value}");
                            ActionAnswer.Log("<!> ", $"Запуск Updater с сохранением текущих параметров");
                            Updater.Update();
                        }
                    }
                    ActionAnswer.Log("<!> ", "Операция завершена");
                    break;

                case "search":
                    List<PigetScript> pigetScripts = localLibrariesManager.SearchScripts(args[1]);
                    if (pigetScripts != null)
                    {
                        ActionAnswer.Log("<?> ", $"Результатов: {pigetScripts.Count}");
                        pigetScripts.ForEach(scr => 
                        {
                            ActionAnswer.Log($"<{scr.ScriptLibrary.Name}> ", $"{scr.Name}");
                        });
                    }
                    else
                    {
                        ActionAnswer.Log("<?> ", "Пакеты не найдены");
                    }
                    break;

                case "run":
                    PigetScript sc = localLibrariesManager.GetScriptByName(args[1]);
                    if (sc != null)
                    {
                        ActionAnswer.Log("<?> ", $"Библиотека пакета: {sc.ScriptLibrary.Name}, Хэш: {HashManager.GetScriptHash(sc)}");
                        sc.Run(Helpers.TrimArrayStart(Helpers.TrimArrayStart(args)));
                    }
                    else
                    {
                        ActionAnswer.Log("<?> ", "Пакеты не найдены");
                    }
                    break;

                case "install":
                    Installer.Run(localLibrariesManager.PigetDirectory, localLibrariesManager);
                    break;

                case "info":
                    {
                        Console.WriteLine(GetScrPage(localLibrariesManager.GetScriptByName(args[1])));
                    }
                    break;

                case "ver":
                    {
                        ActionAnswer.Log("<?> ", $"Версия PIGET: {Version}");
                    }
                    break;

                case "help":
                    {
                        Console.WriteLine(GetHelpPage());
                    }
                    break;
            }
        }

        public static string GetScrPage(PigetScript pigetScript) =>

            $"    Имя:        {pigetScript.Name}\r\n" +
            $"    Описание:   {pigetScript.Description}\r\n" +
            $"    Хэш:        {HashManager.GetScriptHash(pigetScript)}\r\n" +
            $"    Библиотека: {pigetScript.ScriptLibrary.Name}\r\n" +
            $"    Код:        {pigetScript.InitialScript}";

        public static string GetHelpPage() =>

            "piget \r\n" +
            "    run <ScriptName>\r\n" +
            "    search \"<Keywords>\"\r\n" +
            "    library [connect <ManifestLink> | disconnect <LibraryName> | list <LibraryName> | update <LibraryName>]\r\n" +
            "    libraries [disconnect | list | update]\r\n" +
            "    check [resources | ...]\r\n" +
            "    info <ScriptName>\r\n" +
            "    update\r\n" +
            "    ver\r\n" +
            "    help";
    }
}
