using piget.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace piget
{
    internal class Program
    {
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
                            Helpers.Logs.Log("<!> ", "Передача команды API");
                            {
                                localLibrariesManager.Add(args[2]);
                            }
                            break;

                        case "disconnect":
                            Helpers.Logs.Log("<!> ", "Передача команды API");
                            {
                                localLibrariesManager.Remove(localLibrariesManager.GetLibraryByName(args[2]));
                            }
                            break;

                        case "reconnect":
                            Helpers.Logs.Log("<!> ", "Передача команды API");
                            {
                                localLibrariesManager.Reconnect(localLibrariesManager.GetLibraryByName(args[2]));
                            }
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
                            Helpers.Logs.Log("<!> ", "Получение компонентов");
                            {
                                localLibrariesManager.GetLibraryByName(args[2]).Update();
                            }
                            Helpers.Logs.Log("<!> ", "Библиотеки обновлены");
                            break;
                    }
                    break;

                case "libraries":
                    switch (args[1])
                    {
                        case "disconnect":
                            Helpers.Logs.Log("<!> ", "Передача команды API с высоким приоритетом");
                            {
                                localLibrariesManager.RemoveAll();
                            }
                            break;

                        case "reconnect":
                            Helpers.Logs.Log("<!> ", "Передача команды API с высоким приоритетом");
                            {
                                localLibrariesManager.ReconnectAll();
                            }
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
                                        $"    Адрес: {pigetScriptLibrary.Url}\r\n" +
                                        $"    Источники: {ListToString(pigetScriptLibrary.ScriptListAddresses)}\r\n" +
                                        $"    Хэш: {HashManager.GetScriptLibraryHash(pigetScriptLibrary)}\r\n" +
                                        $"    Кол-во пакетов: {pigetScriptLibrary.GetScripts().Count}");
                                }
                            }
                            break;

                        case "update":
                            Helpers.Logs.Log("<!> ", "Запуск операции обновления библиотек и их источников");
                            {
                                localLibrariesManager.UpdateAll();
                            }
                            Helpers.Logs.Log("<!> ", "Библиотеки обновлены");
                            break;
                    }
                    break;

                case "utilities":
                    switch (args[1])
                    {
                        case "download":
                            Helpers.Network.DownloadWithProgress(args[2], args[3]);
                            break;
                    }
                    break;

                case "check":
                    switch (args[1])
                    {
                        case "resources":
                            {
                                Helpers.Logs.Log("<!> ", "Запуск операции проверки ресурсов");
                                {
                                    localLibrariesManager.CheckAllResources();
                                }
                                Helpers.Logs.Log("<!> ", "Операция завершена");
                            }
                            break;
                    }
                    break;

                case "update":
                    Helpers.Logs.Log("<!> ", "Поиск обновлений");
                    {
                        KeyValuePair<bool, string> res = Updater.CheckUpdates();
                        if (res.Key)
                        {
                            Helpers.Logs.Log("<!> ", "Установлена последняя версия");
                        }
                        else
                        {
                            Helpers.Logs.Log("<?> ", $"Найдена новая версия: {res.Value.Replace("\n", null)}");
                            Helpers.Logs.Log("<!> ", $"Запуск Updater с сохранением текущих параметров");
                            Updater.Update();
                        }
                    }
                    Helpers.Logs.Log("<!> ", "Операция завершена");
                    break;

                case "search":
                    List<PigetScript> pigetScripts = localLibrariesManager.SearchScripts(args[1]);
                    if (pigetScripts != null)
                    {
                        Helpers.Logs.Log("<?> ", $"Результатов: {pigetScripts.Count}");
                        pigetScripts.ForEach(scr => 
                        {
                            Helpers.Logs.Log($"<{scr.ScriptLibrary.Name}> ", $"{scr.Name}");
                        });
                    }
                    else
                    {
                        Helpers.Logs.Log("<?> ", "Пакеты не найдены");
                    }
                    break;

                case "run":

                    if (args[1].Contains(":"))
                    {
                        string libName = args[1].Split(':')[0];
                        string scrName = args[1].Split(':')[1];

                        PigetScriptLibrary pigetScriptLibrary = localLibrariesManager.GetLibraryByName(libName);
                        if (pigetScriptLibrary != null)
                        {
                            PigetScript sc = pigetScriptLibrary.GetScriptByName(scrName);
                            if (sc == null)
                            {
                                Helpers.Logs.Log("<?> ", $"Скрипт {args[1]} не найден");
                            }
                            else
                            {
                                ScRun(args, sc);
                            }
                        }
                        else
                        {
                            Helpers.Logs.Log("<?> ", $"Библиотека {libName} не найдена");
                        }
                    }
                    else
                    {
                        List<PigetScript> sc = localLibrariesManager.GetScriptsByName(args[1]);
                        if (sc.Count > 1)
                        {
                            Helpers.Logs.Log("<?> ", $"Найдено несколько сценариев. Для уточнения расположения используйте <libName>:<scName>.");
                            Helpers.Logs.Log("<?> ", $"После подтверждения будет запущен первый найденный сценарий.");

                            Helpers.Logs.LogCustom(new Dictionary<string, ConsoleColor> { 
                                { $"Запустить {sc[0].ScriptLibrary.Name}:{sc[0].Name}?", ConsoleColor.Yellow },
                                { " [Y/n] ", Console.ForegroundColor }
                            });

                            string answer = Console.ReadLine();

                            if (answer == "n" ||
                                answer == "N" ||
                                !string.IsNullOrWhiteSpace(answer) && answer != "Y" && answer != "y")

                                return;
                        }

                        if (sc.Count > 0)
                        {
                            ScRun(args, sc[0]);
                        } 
                        else
                        {
                            Helpers.Logs.Log("<?> ", $"Ничего не найдено");
                        }
                    }
                    break;

                case "install":
                    {
                        Installer.Run(localLibrariesManager.PigetDirectory, localLibrariesManager);
                    }
                    break;

                case "info":
                    {
                        Console.WriteLine(GetScrPage(localLibrariesManager.GetScriptByName(args[1])));
                    }
                    break;

                case "ver":
                    {
                        Helpers.Logs.Log("<?> ", $"Версия PIGET: {Meta.Version}");
                    }
                    break;

                case "editor":
                    {
                        new ReposEditor.Editor().Initialize();
                    }
                    break;

                case "help":
                    {
                        Console.WriteLine(GetHelpPage());
                    }
                    break;
            }
        }

        private static void ScRun(string[] args, PigetScript sc)
        {
            if (sc != null)
            {
                Helpers.Logs.Log("<?> ", $"Библиотека пакета: {sc.ScriptLibrary.Name}, Хэш: {HashManager.GetScriptHash(sc)}");
                sc.Run(Helpers.Array.TrimArrayStart(Helpers.Array.TrimArrayStart(args)));
            }
            else
            {
                Helpers.Logs.Log("<?> ", "Пакеты не найдены");
            }
        }

        public static string GetScrPage(PigetScript pigetScript)
        {
            return

            $"    Имя:        {pigetScript.Name}\r\n" +
            $"    Описание:   {pigetScript.Description}\r\n" +
            $"    Библиотека: {pigetScript.ScriptLibrary.Name}\r\n" +
            $"    Хэш:        {HashManager.GetScriptHash(pigetScript)}\r\n" +
            $"    Код:        {pigetScript.InitialScript}";
        }

        public static string GetHelpPage()
        {
            return

            "piget \r\n" +
            "    run <ScriptName>\r\n" +
            "    search \"<Keywords>\"\r\n" +
            "    library [connect <ManifestLink> | disconnect <LibraryName> | reconnect <LibraryName> | list <LibraryName> | update <LibraryName>]\r\n" +
            "    libraries [disconnect | reconnect | list | update]\r\n" +
            "    utilites [download <RemoteLink> <LocalPath>]\r\n" +
            "    check [resources | ...]\r\n" +
            "    info <ScriptName>\r\n" +
            "    update\r\n" +
            "    ver\r\n" +
            "    help";
        }

        public static string ListToString(List<string> list)
        {
            string result = null;

            foreach (string item in list)
                result += $" -> {item}\r\n";

            result.TrimEnd('\n');

            return result;
        }
    }
}
