using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

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
                Helpers.UnknownPublisherNotify();

            DownloadResources(scriptEnvironement, scriptRootDirectory);

            Process process = new Process();
            process.StartInfo.Arguments = $" {Helpers.ConvertStringArrayToString(args)}";
            process.StartInfo.FileName = scriptBatchPath;
            process.StartInfo.WorkingDirectory = scriptEnvironement;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = false;
            process.Start();

            Helpers.ProcessStartNotify(process.Id);

            process.WaitForExit();
        }

        /// <summary>
        /// Скачивает ресурсы скрипта, если это необходимо
        /// </summary>
        public void DownloadResources(string scriptEnvironement, string scriptRootDirectory)
        {
            string resourcesArchivePath = Path.Combine(scriptRootDirectory, PigetScriptLibrary.ScriptResourcesName);

            Helpers.ResourceAuthenticationDisabledNotify();

            if (string.IsNullOrEmpty(Resources) || File.Exists(resourcesArchivePath))
            {
                Helpers.ResourcesAlreadyLoadedNotify();
                return;
            }

            Helpers.DownloadWithProgress(Resources, resourcesArchivePath);
            Helpers.ExtractToDirectory(resourcesArchivePath, scriptEnvironement);
            Helpers.SavePackageNotify(scriptEnvironement);
        }

        /// <summary>
        /// Проверяет возможность загрузки ресурсов
        /// </summary>
        public bool CheckResources() =>
            Resources == null ||
            Helpers.RemoteFileExists(Resources);

        public void Remove()
        {
            string scriptHash = HashManager.GetScriptHash(this);
            string scriptRootDirectory = Path.Combine(ScriptLibrary.LibraryDirectory, scriptHash);

            Directory.Delete(scriptRootDirectory, true);
        }
    }
}
