using QislEngine;
using System.Diagnostics;
using System.IO;

namespace piget.Api
{
    public class PigetScript
    {
        /// <summary>
        /// Локальная библиотека в которой находится скрипт
        /// </summary>
        public PigetScriptLibrary ScriptLibrary { get; set; }

        /// <summary>
        /// Имя скрипта (Допустимые символы: a-z 0-9)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание скрипта (Допустимы любые символы)
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Адрес zip-архива с ресурсами скрипта
        /// </summary>
        public string Resources { get; set; }

        /// <summary>
        /// Batch-скрипт
        /// </summary>
        public string InitialScript { get; set; }

        /// <summary>
        /// Запустить скрипт в ScriptDirectory
        /// </summary>
        public void Run(string[] args)
        {
            string scriptHash = HashManager.GetScriptHash(this);
            string scriptRootDirectory = Path.Combine(ScriptLibrary.LibraryDirectory, scriptHash);
            string scriptEnvironement = Path.Combine(scriptRootDirectory, PigetScriptLibrary.ScriptEnvironementName);
            string scriptBatchPath = Path.Combine(scriptEnvironement, PigetScriptLibrary.ScriptFileName);

            if (!ScriptLibrary.Url.Contains(Updater.PigetLibraryStd))
                Helpers.Notify.UnknownPublisherNotify();

            InitializeResources(scriptEnvironement, scriptRootDirectory);

            Process process = new Process();
            process.StartInfo.Arguments = $" {Helpers.Convert.ConvertStringArrayToString(args)}";
            process.StartInfo.FileName = scriptBatchPath;
            process.StartInfo.WorkingDirectory = scriptEnvironement;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = false;
            process.Start();

            Helpers.Notify.ProcessStartNotify(process.Id);

            process.WaitForExit();
        }

        /// <summary>
        /// Инициализирует ресурсы скрипта, если это необходимо
        /// </summary>
        public void InitializeResources(string scriptEnvironement, string scriptRootDirectory)
        {
            string resourcesTempPath = Path.Combine(scriptRootDirectory, PigetScriptLibrary.ScriptResourcesName);

            Helpers.Notify.ResourceAuthenticationDisabledNotify();

            if (string.IsNullOrEmpty(Resources) || File.Exists(resourcesTempPath))
            {
                Helpers.Notify.ResourcesAlreadyLoadedNotify();
                return;
            }

            Helpers.Network.DownloadWithProgress(Resources, resourcesTempPath);
            Helpers.FileSystem.ExtractToDirectory(resourcesTempPath, scriptEnvironement);
            Helpers.Notify.SavePackageNotify(scriptEnvironement);
        }

        /// <summary>
        /// Проверяет возможность загрузки ресурсов
        /// </summary>
        public bool CheckResources() =>
            Resources == null ||
            Helpers.Network.RemoteFileExists(Resources);

        public void Remove()
        {
            string scriptHash = HashManager.GetScriptHash(this);
            string scriptRootDirectory = Path.Combine(ScriptLibrary.LibraryDirectory, scriptHash);

            Directory.Delete(scriptRootDirectory, true);
        }
    }
}
