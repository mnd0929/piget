using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace piget.Api
{
    public class PigetScriptLibrary
    {
        public PigetScriptLibrary() { }
        public PigetScriptLibrary(string url, string libraryListDirectory) =>
            (Url, LibraryDirectory) = (url, libraryListDirectory);

        /// <summary>
        /// Имя файла манифеста скрипта
        /// </summary>
        [NonSerialized] public const string ScriptManifestFileName = "manifest.json";

        /// <summary>
        /// Имя исполняемого файла скрипта
        /// </summary>
        [NonSerialized] public const string ScriptFileName = "run.bat";

        /// <summary>
        /// Имя файла ресурсов скрипта
        /// </summary>
        [NonSerialized] public const string ScriptResourcesName = "resources";

        /// <summary>
        /// Имя временной директории с ресурсами
        /// </summary>
        [NonSerialized] public const string ScriptResourcesTmpDirectoryName = "resourcestmp";

        /// <summary>
        /// Имя директории в которой исполняется скрипт
        /// </summary>
        [NonSerialized] public const string ScriptEnvironementName = "local";

        /// <summary>
        /// Имя файла манифеста библиотеки
        /// </summary>
        [NonSerialized] public const string ManifestFileName = "lib.json";

        /// <summary>
        /// Директория содержащая библиотеку
        /// </summary>
        public string LibraryDirectory { get; set; }

        /// <summary>
        /// Адрес по которому находится манифест библиотеки
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Адрес по которому находится список пакетов библиотеки
        /// </summary>
        public List<string> ScriptListAddresses { get; set; } = new List<string>();

        /// <summary>
        /// Имя библиотеки
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Дополнительная информация о библиотеке
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Обновить библиотеку
        /// </summary>
        public void Update()
        {
            Helpers.Logs.Log("<!> ", $"{Name} -> Обновление манифеста");

            //
            // Обновление манифеста библиотеки
            PigetScriptLibrary pigetScriptLibrary = 
                JsonSerializer.Deserialize<PigetScriptLibrary>(new HttpClient().GetStringAsync(Url).Result);

            (ScriptListAddresses, Name, Description) = 
                (pigetScriptLibrary.ScriptListAddresses, pigetScriptLibrary.Name, pigetScriptLibrary.Description);

            Helpers.FileSystem.CreateFileWithText(Path.Combine(LibraryDirectory, ManifestFileName), JsonSerializer.Serialize(this));

            //
            // Обновление источников библиотеки
            ScriptListAddresses.ForEach(scriptListAddress =>
            {
                ScriptSourceModel pigetScripts =
                    JsonSerializer.Deserialize<ScriptSourceModel>(new HttpClient().GetStringAsync(scriptListAddress).Result, new JsonSerializerOptions { AllowTrailingCommas = true });

                pigetScripts.Scripts.ForEach(script => 
                {
                    string scriptHash = HashManager.GetScriptHash(script);
                    string scriptDirectory = Path.Combine(LibraryDirectory, scriptHash);
                    string scriptEnvironementDirectory = Path.Combine(scriptDirectory, ScriptEnvironementName);
                    string scriptManifestPath = Path.Combine(scriptDirectory, ScriptManifestFileName);
                    string scriptBatExecutablePath = Path.Combine(scriptEnvironementDirectory, ScriptFileName);

                    script.ScriptLibrary = this;

                    if (Directory.Exists(scriptDirectory))
                    {
                        if (HashManager.GetScriptHash(scriptManifestPath) == scriptHash)
                        {
                            Helpers.Logs.Log("<!> ", $"{Name} -> Обновление источников -> Скрипт {script.Name} не нуждается в обновлении");
                            return;
                        }
                        else
                        {
                            Directory.Delete(scriptDirectory, true);
                        }
                    }

                    Directory.CreateDirectory(scriptDirectory);
                    Directory.CreateDirectory(scriptEnvironementDirectory);

                    Helpers.FileSystem.CreateFileWithText(scriptManifestPath, JsonSerializer.Serialize(script));
                    Helpers.FileSystem.CreateFileWithText(scriptBatExecutablePath, script.InitialScript);
                    Helpers.Logs.Log("<!> ", $"{Name} -> Обновление источников -> Скрипт {script.Name} обновлен");
                });
            });
        }
        public List<PigetScript> GetScripts()
        {
            List<PigetScript> pigetScripts = new List<PigetScript>();

            new DirectoryInfo(LibraryDirectory).GetDirectories().ToList().ForEach(scriptDirectory =>
            {
                pigetScripts.Add(JsonSerializer.Deserialize<PigetScript>(File.ReadAllText(Path.Combine(scriptDirectory.FullName, ScriptManifestFileName))));
            });

            return pigetScripts;
        }
        public PigetScript GetScriptByName(string name)
        {
            foreach (DirectoryInfo scriptDirectory in new DirectoryInfo(LibraryDirectory).GetDirectories())
            {
                PigetScript pigetScript =
                    JsonSerializer.Deserialize<PigetScript>(File.ReadAllText(Path.Combine(scriptDirectory.FullName, ScriptManifestFileName)));

                if (name == pigetScript.Name)
                    return pigetScript;
            }

            return null;
        }
    }
}
