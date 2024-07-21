using QislEngine.Helpers;
using QislEngine.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace QislEngine
{
    public class LocalLibrariesManager
    {
        public LocalLibrariesManager() 
        {
            PigetDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "piget");
            LibrariesDirectory = Path.Combine(PigetDirectory, "libraries");

            Directory.CreateDirectory(PigetDirectory);
            Directory.CreateDirectory(LibrariesDirectory);
        }

        /// <summary>
        /// Корневая директория PIGET
        /// </summary>
        public string PigetDirectory { get; private set; }

        /// <summary>
        /// Директория LocalLibrariesManager
        /// </summary>
        public string LibrariesDirectory { get; private set; }

        /// <summary>
        /// Подключает библиотеку
        /// </summary>
        /// <param name="url">Ссылка на манифест библиотеки</param>
        public void Add(string url)
        {
            string libraryHash = HashManager.GetScriptLibraryHash(url);
            string libraryPath = Path.Combine(LibrariesDirectory, libraryHash);

            Directory.CreateDirectory(libraryPath);

            PigetScriptLibrary newLibrary = new PigetScriptLibrary(url, libraryPath);
            newLibrary.Update();
        }

        /// <summary>
        /// Отключает библиотеку
        /// </summary>
        public void Remove(PigetScriptLibrary library) 
        {
            Directory.Delete(library.LibraryDirectory, true);
        }

        /// <summary>
        /// Переподключает библиотеку
        /// </summary>
        public void Reconnect(PigetScriptLibrary library)
        {
            Remove(library);
            Add(library.Url);
        }

        /// <summary>
        /// Обновляет ВСЕ библиотеки
        /// </summary>
        public void UpdateAll()
        {
            foreach (PigetScriptLibrary lib in GetLibraries())
            {
                try
                {
                    lib.Update();
                }
                catch (Exception ex)
                {
                    Logs.LogError($"Не удалось обновить библиотеку «{lib.Name}»", ex);
                }
            }
        }

        /// <summary>
        /// Переподключает ВСЕ библиотеки
        /// </summary>
        public void ReconnectAll()
        {
            foreach (PigetScriptLibrary lib in GetLibraries())
            {
                try
                {
                    Reconnect(lib);
                }
                catch (Exception ex)
                {
                    Logs.LogError($"Не удалось переподключить библиотеку «{lib.Name}»", ex);
                }
            }
        }

        /// <summary>
        /// Отключает ВСЕ библиотеки
        /// </summary>
        public void RemoveAll()
        {
            foreach (PigetScriptLibrary lib in GetLibraries())
            {
                try
                {
                    Remove(lib);
                }
                catch (Exception ex)
                {
                    Logs.LogError($"Не удалось отключить библиотеку «{lib.Name}»", ex);
                }
            }
        }

        /// <summary>
        /// Возвращает все подключенные библиотеки
        /// </summary>
        /// <returns></returns>
        public List<PigetScriptLibrary> GetLibraries() 
        {
            List<PigetScriptLibrary> libraries = new List<PigetScriptLibrary>();

            new DirectoryInfo(LibrariesDirectory).GetDirectories().ToList().ForEach(libraryDirectory => 
            {
                try
                {
                    libraries.Add(JsonSerializer.Deserialize<PigetScriptLibrary>(File.ReadAllText(Path.Combine(libraryDirectory.FullName, PigetScriptLibrary.ManifestFileName))));
                }
                catch (Exception ex)
                {
                    Logs.LogError($"Не удалось считать библиотеку {libraryDirectory}", ex);
                }
            });

            return libraries;
        }

        /// <summary>
        /// Возвращает библиотеку по ее имени
        /// </summary>
        public PigetScriptLibrary GetLibraryByName(string name)
        {
            foreach (var libraryDirectory in new DirectoryInfo(LibrariesDirectory).GetDirectories())
            {
                PigetScriptLibrary pigetScriptLibrary =
                    JsonSerializer.Deserialize<PigetScriptLibrary>(File.ReadAllText(Path.Combine(libraryDirectory.FullName, PigetScriptLibrary.ManifestFileName)));

                if (pigetScriptLibrary.Name == name)
                    return pigetScriptLibrary;
            };

            return null;
        }

        /// <summary>
        /// Возвращает ВСЕ скрипты из ВСЕХ библиотек
        /// </summary>
        /// <returns></returns>
        public List<PigetScript> GetScripts() 
        {
            List<PigetScript> scripts = new List<PigetScript>();

            GetLibraries().ForEach(library => scripts.AddRange(library.GetScripts()));

            return scripts;
        }

        /// <summary>
        /// Возвращает скрипт по его имени
        /// </summary>
        public PigetScript GetScriptByName(string name)
        {
            foreach (PigetScriptLibrary lib in GetLibraries())
            {
                PigetScript pigetScript = lib.GetScriptByName(name);

                if (pigetScript != null)
                    return pigetScript;
            }

            return null;
        }

        /// <summary>
        /// Возвращает скрипты c указанным именем
        /// </summary>
        public List<PigetScript> GetScriptsByName(string name)
        {
            List<PigetScript> result = new List<PigetScript>();

            foreach (PigetScriptLibrary lib in GetLibraries())
            {
                PigetScript pigetScript = lib.GetScriptByName(name);

                if (pigetScript != null)
                    result.Add(pigetScript);
            }

            return result;
        }

        /// <summary>
        /// Ищет скрипты по указанным ключевым словам
        /// </summary>
        public List<PigetScript> SearchScripts(string keywords) =>
            GetScripts().FindAll(x => 
                keywords.ToLower().Contains(x.Name.ToLower()) || 
                x.Name.ToLower().Contains(keywords.ToLower()) || 
                x.Description.ToLower().Contains(keywords.ToLower())
            );

        /// <summary>
        /// Проверяет на доступность ресурсы скриптов
        /// </summary>
        /// <param name="removeBadScripts">Нужно ли удалять скрипты с недоступными ресурсами</param>
        /// <returns></returns>
        public List<PigetScript> CheckAllResources(bool removeBadScripts = true)
        {
            List<PigetScript> badScripts = new List<PigetScript>();

            GetScripts().ForEach(script =>
            {
                Logs.LogCustom(new Dictionary<string, ConsoleColor> {
                    { $"<!> ", ConsoleColor.Yellow } ,
                    { $"Проверка доступности ресурса для {script.Name} из {script.ScriptLibrary.Name}: ", Console.ForegroundColor }
                });

                bool result = script.CheckResources();

                ColorConsole.Write($"{result}\r\n", result ? ConsoleColor.Green : ConsoleColor.Red);

                if (!result)
                    badScripts.Add(script);
            });

            if (removeBadScripts)
            {
                badScripts.ForEach(badScript => 
                {
                    Logs.Log("<!> ", $"Удаление {badScript.Name} из {badScript.ScriptLibrary.Name}");
                    badScript.Remove();
                });
            }
            else
            {
                Logs.Log("<!> ", $"Удаление скриптов с недоступными ресурсами отменено");
            }
            
            return badScripts;
        }
    }
}
