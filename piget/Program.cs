using piget.Api;
using ArgumentParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using QislEngine;

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
            // LTSC команды
            switch (args[0])
            {
                case "utilities":
                    switch (args[1])
                    {
                        case "download":
                            Helpers.Network.DownloadWithProgress(args[2], args[3]);
                            Environment.Exit(0);
                            break;
                    }
                    break;
            }

            // Основные команды
            Parser parser = new Parser
            {
                Name = "piget",
                Description = "Программа командной строки piget дает возможность выполнять удаленные сценарии из командной строки.",
                HelpArg = "-?",
                Items =
                {
                    new ParserItem
                    {
                        Name = "libraries",
                        Description = "Выполняет команду над всеми локальными библиотеками",
                        Childrens =
                        {
                            new ParserItem
                            {
                                Name = "update",
                                Description = "Обновляет все локальные библиотеки",

                            }.AddFunction((Dictionary<string, string> argums) => LibrariesUpdate()),

                            new ParserItem
                            {
                                Name = "disconnect",
                                Description = "Отключает все локальные библиотеки",

                            }.AddFunction((Dictionary<string, string> argums) => LibrariesDisconnect()),

                            new ParserItem
                            {
                                Name = "reconnect",
                                Description = "Переподключает все локальные библиотеки",

                            }.AddFunction((Dictionary<string, string> argums) => LibrariesReconnect()),

                            new ParserItem
                            {
                                Name = "list",
                                Description = "Отображает все подключенные библиотеки",

                            }.AddFunction((Dictionary<string, string> argums) => LibrariesList()),
                        }
                    },
                    new ParserItem
                    {
                        Name = "library",
                        Description = "Выполняет команду над определенной локальной библиотекой",
                        Childrens =
                        {
                            new ParserItem
                            {
                                Name = "connect",
                                Description = "Подключает библиотеку",
                                DefaultArgument = new Argument
                                {
                                    Name = "manifestLink",
                                    Description = "Ссылка на манифест библиотеки",
                                    IsRequired = true,
                                }

                            }.AddFunction((Dictionary<string, string> argums) => LibraryAdd(argums["manifestLink"])),

                            new ParserItem
                            {
                                Name = "disconnect",
                                Description = "Отключает библиотеку",
                                DefaultArgument = new Argument
                                {
                                    Name = "name",
                                    ShortName = "n",
                                    Description = "Имя библиотеки",
                                    IsRequired = true,
                                },

                            }.AddFunction((Dictionary<string, string> argums) => LibraryRemove(argums["name"])),

                            new ParserItem
                            {
                                Name = "reconnect",
                                Description = "Переподключает библиотеку",
                                DefaultArgument = new Argument
                                {
                                    Name = "name",
                                    ShortName = "n",
                                    Description = "Имя библиотеки",
                                    IsRequired = true
                                }

                            }.AddFunction((Dictionary<string, string> argums) => LibraryReconnect(argums["name"])),

                            new ParserItem
                            {
                                Name = "update",
                                Description = "Обновляет библиотеку",
                                DefaultArgument = new Argument
                                {
                                    Name = "name",
                                    ShortName = "n",
                                    Description = "Имя библиотеки",
                                    IsRequired = true
                                }

                            }.AddFunction((Dictionary<string, string> argums) => LibraryUpdate(argums["name"])),

                            new ParserItem
                            {
                                Name = "list",
                                Description = "Отображает список скриптов в библиотеке",
                                DefaultArgument = new Argument
                                {
                                    Name = "name",
                                    ShortName = "n",
                                    Description = "Имя библиотеки",
                                    IsRequired = true
                                }

                            }.AddFunction((Dictionary<string, string> argums) => LibraryList(argums["name"])),
                        }
                    },
                    new ParserItem
                    {
                        Name = "tools",
                        Description = "Предоставляет вспомогательные функции для разработки скриптов",
                        Childrens =
                        {
                            new ParserItem
                            {
                                Name = "download",
                                Description = "Скачивает файл в указанное расположение",
                                Arguments =
                                {
                                    new Argument
                                    {
                                        Name = "url",
                                        ShortName = "u",
                                        Description = "Удаленное расположение файла",
                                        IsRequired = true
                                    },
                                    new Argument
                                    {
                                        Name = "path",
                                        ShortName = "p",
                                        Description = "Локальное расположение файла",
                                        IsRequired = true
                                    }
                                }

                            }.AddFunction((Dictionary<string, string> argums) => UtilitiesDownload(argums["url"], argums["path"])),
                        }
                    },
                    new ParserItem
                    {
                        Name = "check",
                        Description = "Выполняет проверку определенного компонента piget",
                        Childrens =
                        {
                            new ParserItem
                            {
                                Name = "resources",
                                Description = "Выполняет проверку доступности ресурсов всех скриптов во всех библиотеках",
                                Arguments =
                                {
                                    new Argument
                                    {
                                        Name = "auto-delete",
                                        ShortName = "d",
                                        Description = "Указывает, нужно ли удалять скрипты с недоступными ресурсами [True / False]",
                                        IsRequired = true
                                    }
                                }

                            }.AddFunction((Dictionary<string, string> argums) => CheckResources(bool.Parse(argums["auto-delete"]))),
                        }
                    },

                    new ParserItem
                    {
                        Name = "update",
                        Description = "Обновляет piget, если это необходимо"

                    }.AddFunction((Dictionary<string, string> argums) => Update()),

                    new ParserItem
                    {
                        Name = "search",
                        Description = "Выполняет поиск скриптов по фильтру",
                        DefaultArgument = new Argument
                        {
                            Name = "filter",
                            ShortName = "f",
                            Description = "Фильтр для поиска скриптов",
                            IsRequired = true
                        }

                    }.AddFunction((Dictionary<string, string> argums) => Search(argums["filter"])),

                    new ParserItem
                    {
                        Name = "run",
                        Description = "Запускает сценарий",
                        DefaultArgument = new Argument
                        {
                            Name = "name",
                            ShortName = "n",
                            Description = "Имя скрипта (Используйте <[libraryName]>:<[scriptName]> для уточнения расположения)",
                            IsRequired = true
                        }

                    }.AddFunction((Dictionary<string, string> argums) => Run(args, argums["name"])),

                    new ParserItem
                    {
                        Name = "install",
                        Description = "Запускает установку piget"

                    }.AddFunction((Dictionary<string, string> argums) => Install()),

                    new ParserItem
                    {
                        Name = "info",
                        Description = "Отображает информацию об указанном скрипте",
                        DefaultArgument = new Argument
                        {
                            Name = "name",
                            ShortName = "n",
                            Description = "Имя скрипта",
                            IsRequired = true
                        }

                    }.AddFunction((Dictionary<string, string> argums) => Info(argums["name"])),

                    new ParserItem
                    {
                        Name = "ver",
                        Description = "Отображает версию piget"

                    }.AddFunction((Dictionary<string, string> argums) => Ver()),

                    new ParserItem
                    {
                        Name = "editor",
                        Description = "Запускает редактор скриптов (В разработке)"

                    }.AddFunction((Dictionary<string, string> argums) => Editor()),
                }
            };

            try
            {
                parser.Parse(args);
            }
            catch 
            {
                Console.WriteLine($"Ошибка в синтаксисе команды. Добавьте '{parser.HelpArg}' в конец команды для просмотра справки.");
            }
        }

        public static void LibraryAdd(string manifestLink) =>
            localLibrariesManager.Add(manifestLink);

        public static void LibraryRemove(string name) =>
            localLibrariesManager.Remove(localLibrariesManager.GetLibraryByName(name));

        public static void LibraryReconnect(string name) =>
            localLibrariesManager.Reconnect(localLibrariesManager.GetLibraryByName(name));

        public static void LibraryList(string name)
        {
            List<PigetScript> Scripts = localLibrariesManager.GetLibraryByName(name).GetScripts();
            for (int i = 0; i < Scripts.Count; i++)
            {
                PigetScript pigetScript = Scripts[i];

                Console.WriteLine($"{i + 1}: {pigetScript.Name}");
            }
        }

        public static void LibraryUpdate(string name)
        {
            Helpers.Logs.Log("<!> ", "Получение компонентов");
            localLibrariesManager.GetLibraryByName(name).Update();
            Helpers.Logs.Log("<!> ", "Библиотеки обновлены");
        }

        public static void LibrariesDisconnect()
        {
            Helpers.Logs.Log("<!> ", "Передача команды API с высоким приоритетом");
            localLibrariesManager.RemoveAll();
        }

        public static void LibrariesReconnect()
        {
            Helpers.Logs.Log("<!> ", "Передача команды API с высоким приоритетом");
            localLibrariesManager.ReconnectAll();
        }


        public static void LibrariesList()
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

        public static void LibrariesUpdate()
        {
            Helpers.Logs.Log("<!> ", "Запуск операции обновления библиотек и их источников");
            localLibrariesManager.UpdateAll();
            Helpers.Logs.Log("<!> ", "Библиотеки обновлены");
        }

        public static void UtilitiesDownload(string url, string outPath) => Helpers.Network.DownloadWithProgress(url, outPath);

        public static void CheckResources(bool removeBadScripts = true)
        {
            Helpers.Logs.Log("<!> ", "Запуск операции проверки ресурсов");
            {
                localLibrariesManager.CheckAllResources(removeBadScripts);
            }
            Helpers.Logs.Log("<!> ", "Операция завершена");
        }

        public static void Update()
        {
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
        }

        public static void Search(string filter)
        {
            List<PigetScript> pigetScripts = localLibrariesManager.SearchScripts(filter);
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
        }

        public static void Run(string[] args, string name)
        {
            if (name.Contains(":"))
            {
                string libName = name.Split(':')[0];
                string scrName = name.Split(':')[1];

                PigetScriptLibrary pigetScriptLibrary = localLibrariesManager.GetLibraryByName(libName);
                if (pigetScriptLibrary != null)
                {
                    PigetScript sc = pigetScriptLibrary.GetScriptByName(scrName);
                    if (sc == null)
                    {
                        Helpers.Logs.Log("<?> ", $"Скрипт {name} не найден");
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
                List<PigetScript> sc = localLibrariesManager.GetScriptsByName(name);
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
        }

        public static void Install()
        {
            Installer.Run(localLibrariesManager.PigetDirectory, localLibrariesManager);
        }

        public static void Info(string name)
        {
            Console.WriteLine(GetScrPage(localLibrariesManager.GetScriptByName(name)));
        }

        public static void Ver()
        {
            Helpers.Logs.Log("<?> ", $"Версия PIGET: {Meta.Version}");
        }

        public static void Editor()
        {
            new ReposEditor.Editor().Initialize();
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

        public static string ListToString(List<string> list)
        {
            string result = null;

            foreach (string item in list)
                result += $" -> {item}\r\n";

            result?.TrimEnd('\r').TrimEnd('\n');

            return result;
        }
    }
}
