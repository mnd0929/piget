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
                            ActionAnswer.Log("<!> ", "Передача команды API");
                            {
                                localLibrariesManager.Add(args[2]);
                            }
                            break;

                        case "disconnect":
                            ActionAnswer.Log("<!> ", "Передача команды API");
                            {
                                localLibrariesManager.Remove(localLibrariesManager.GetLibraryByName(args[2]));
                            }
                            break;

                        case "reconnect":
                            ActionAnswer.Log("<!> ", "Передача команды API");
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
                            break;

                        case "reconnect":
                            ActionAnswer.Log("<!> ", "Передача команды API с высоким приоритетом");
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

                case "utilities":
                    switch (args[1])
                    {
                        case "download":
                            Helpers.DownloadWithProgress(args[2], args[3]);
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
                            ActionAnswer.Log("<?> ", $"Найдена новая версия: {res.Value.Replace("\n", null)}");
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
                        ActionAnswer.Log("<?> ", $"Версия PIGET: {Meta.Version}");
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
            $"    Библиотека: {pigetScript.ScriptLibrary.Name}\r\n" +
            $"    Хэш:        {HashManager.GetScriptHash(pigetScript)}\r\n" +
            $"    Код:        {pigetScript.InitialScript}";

        public static string GetHelpPage() =>

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
}
