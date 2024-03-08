using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace piget.Api
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

        public string PigetDirectory { get; private set; }
        public string LibrariesDirectory { get; private set; }

        public void Add(string url)
        {
            ActionAnswer.Log("<!> ", "Инициализация");
            
            string libraryHash = HashManager.GetScriptLibraryHash(url);
            string libraryPath = Path.Combine(LibrariesDirectory, libraryHash);

            Directory.CreateDirectory(libraryPath);

            ActionAnswer.Log("<!> ", "Получение данных");

            PigetScriptLibrary newLibrary = new PigetScriptLibrary(url, libraryPath);
            newLibrary.Update();

            ActionAnswer.Log("<!> ", $"Добавлена библиотека «{newLibrary.Name}»");
        }

        public void Remove(PigetScriptLibrary library) 
        {
            ActionAnswer.Log("<!> ", $"Отключение библиотеки и ее источников: {library.Name}");

            Directory.Delete(library.LibraryDirectory, true);

            ActionAnswer.Log("<!> ", $"Библиотека {library.Name} отключена");
        }

        public void UpdateAll()
        {
            foreach (PigetScriptLibrary lib in GetLibraries())
            {
                ActionAnswer.Log("<!> ", $"Обновление «{lib.Name}»");

                lib.Update();
            }
        }

        public void RemoveAll()
        {
            foreach (PigetScriptLibrary lib in GetLibraries())
            {
                Remove(lib);
            }
        }

        public List<PigetScriptLibrary> GetLibraries() 
        {
            List<PigetScriptLibrary> libraries = new List<PigetScriptLibrary>();

            new DirectoryInfo(LibrariesDirectory).GetDirectories().ToList().ForEach(libraryDirectory => 
            {
                libraries.Add(JsonSerializer.Deserialize<PigetScriptLibrary>(File.ReadAllText(Path.Combine(libraryDirectory.FullName, PigetScriptLibrary.ManifestFileName))));
            });

            return libraries;
        }

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

        public List<PigetScript> GetScripts() 
        {
            List<PigetScript> scripts = new List<PigetScript>();

            GetLibraries().ForEach(library => scripts.AddRange(library.GetScripts()));

            return scripts;
        }

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
        public List<PigetScript> CheckAllResources(bool removeBadScripts = true)
        {
            List<PigetScript> badScripts = new List<PigetScript>();

            GetScripts().ForEach(script =>
            {
                ActionAnswer.WriteColorLine(new Dictionary<string, ConsoleColor> {
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
                    ActionAnswer.Log("<!> ", $"Удаление {badScript.Name} из {badScript.ScriptLibrary.Name}");
                    badScript.Remove();
                });
            }
            else
            {
                ActionAnswer.Log("<!> ", $"Удаление скриптов с недоступными ресурсами отменено");
            }
            
            return badScripts;
        }
    }
}
