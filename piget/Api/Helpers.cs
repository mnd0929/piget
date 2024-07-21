using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;

namespace piget.Api
{
    public class Helpers
    {
        public static class FileSystem 
        {
            public static void CreateFileWithText(string path, string text)
            {
                File.Create(path).Dispose();
                File.WriteAllText(path, text);
            }

            public static void ExtractToDirectory(string archivePath, string outDir)
            {
                Console.CursorVisible = false;

                Logs.LogCustom(new Dictionary<string, ConsoleColor> {
                { "Распаковка ", System.Console.ForegroundColor }, { archivePath + Environment.NewLine, ConsoleColor.Blue } });

                using (var archive = new ZipArchive(File.OpenRead(archivePath)))
                {
                    int totalProgress = archive.Entries.Count;

                    for (int i = 0; i < totalProgress; i++)
                    {
                        ZipArchiveEntry entry = archive.Entries[i];

                        try
                        {
                            string destPath = Path.Combine(outDir, entry.FullName.Replace('/', '\\'));
                            string destDir = Path.GetDirectoryName(destPath);

                            Directory.CreateDirectory(destDir);

                            entry.ExtractToFile(destPath, true);
                        }
                        catch { }

                        ProgressIndicator.WriteDecompressingProgressLine(20, i + 1, totalProgress);
                    }
                }

                Console.WriteLine(Environment.NewLine);
                Console.CursorVisible = true;
            }

        }
        
        public static class Network
        {
            [DllImport("wininet.dll")]
            private extern static bool InternetGetConnectedState(out int description, int reservedValue);
            public static bool IsInternetAvailable()
            {
                int description;
                return InternetGetConnectedState(out description, 0);
            }

            public static void DownloadWithProgress(string url, string path)
            {
                Console.CursorVisible = false;
                Logs.LogCustom(new Dictionary<string, ConsoleColor> {
                { "Скачивание ", System.Console.ForegroundColor }, { url + Environment.NewLine, ConsoleColor.Blue } });

                // Скачивание
                Stopwatch stopwatch = new Stopwatch();
                WebClient webClient = new WebClient();
                webClient.DownloadProgressChanged += (_s, _e) =>
                {
                    if (stopwatch.ElapsedMilliseconds > 1000 || _e.ProgressPercentage == 100)
                    {
                        ProgressIndicator.WriteDownloadingProgressLine(_e.ProgressPercentage, 20, _e);

                        stopwatch.Restart();
                    }
                };
                stopwatch.Start();
                webClient.DownloadFileAsync(new Uri(url), path);

                // Ожидание
                while (webClient.IsBusy)
                    Thread.Sleep(200);

                stopwatch.Stop();

                Console.WriteLine(Environment.NewLine);
                Console.CursorVisible = true;
            }

            ///
            /// Checks the file exists or not.
            ///
            /// The URL of the remote file.
            /// True : If the file exits, False if file not exists
            public static bool RemoteFileExists(string url)
            {
                try
                {
                    //Creating the HttpWebRequest
                    HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                    //Setting the Request method HEAD, you can also use GET too.
                    request.Timeout = 3000;
                    request.Method = "HEAD";
                    //Getting the Web Response.
                    HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                    //Returns TRUE if the Status code == 200
                    response.Close();
                    return (response.StatusCode == HttpStatusCode.OK);
                }
                catch
                {
                    //Any exception will returns false.
                    return false;
                }
            }
        }
        
        public static class ProgressIndicator
        {
            public static void WriteDecompressingProgressLine(int segmentsCount, int currentCount, int targetCount)
            {
                int progress = GetPercent(targetCount, currentCount);

                Console.Write($"{BuildProgressLine(progress, segmentsCount)} {progress}% ({currentCount} / {targetCount})\t");
            }

            public static void WriteDownloadingProgressLine(int percent, int segmentsCount, DownloadProgressChangedEventArgs e)
            {
                string size = e == null ? null :
                    $"{Convert.SizeToStringFormat(e.BytesReceived)} / {Convert.SizeToStringFormat(e.TotalBytesToReceive)}";

                Console.Write($"{BuildProgressLine(percent, segmentsCount)} {size}        ");
            }

            public static void WriteProgressLine(int percent, int segmentsCount) =>
                Console.Write($"{BuildProgressLine(percent, segmentsCount)} {percent}%        ");

            private static string BuildProgressLine(int percent, int segmentsCount)
            {
                int currentSegmentsCount = percent / (100 / segmentsCount);
                int currentHideSegmentsCount = segmentsCount - currentSegmentsCount;

                return $"\r  {string.Concat(Enumerable.Repeat("█", currentSegmentsCount))}{string.Concat(Enumerable.Repeat("░", currentHideSegmentsCount))}";
            }

            public static Int32 GetPercent(Int32 b, Int32 a)
            {
                if (b == 0) return 0;

                return (Int32)(a / (b / 100M));
            }
        }
        
        public static class Notify
        {
            public static void SavePackageNotify(string dir) =>
                Logs.LogCustom(new Dictionary<string, ConsoleColor> {
                    { "Ресурсы сохранены в ", System.Console.ForegroundColor }, { dir + Environment.NewLine, ConsoleColor.Blue } });

            public static void ResourceAuthenticationDisabledNotify() =>
                Logs.Log("<?> ", "Аутентификация хэшей ресурсов отключена.");

            public static void ResourcesAlreadyLoadedNotify() =>
                Logs.Log("<?> ", "Скрипт будет инициализирован с сохраненными ресурсами.");

            public static void UnknownPublisherNotify() =>
                Logs.Log("<?> ", "Издатель PIGET (QISL) не несет ответственности за сторонние сценарии и не предоставляет для них никакие лицензии.");

            public static void ProcessStartNotify(int id) =>
                Logs.Log("<?> ", $"Процесс инициализируется со следующим идентификатором: {id}.");
        }

        public static class Logs
        {
            public static void Log(string action, string text)
            {
                LogCustom(new Dictionary<string, ConsoleColor> {
                    { $"{action}", ConsoleColor.Yellow },
                    { $"{text}\r\n", Console.ForegroundColor }
                });
            }
            public static void LogError(string title, Exception ex)
            {
                ColorConsole.WriteLine($"{title}\r\n", ConsoleColor.Red);
                ColorConsole.WriteLine($"{ex}\r\n", ConsoleColor.DarkGray);
            }
            public static void LogCustom(Dictionary<string, ConsoleColor> dict)
            {
                foreach (var segment in dict)
                {
                    ColorConsole.Write(segment.Key, segment.Value);
                }
            }
        }

        public static class Convert
        {
            public static string SizeToStringFormat(double size)
            {
                string[] sizes = { "B", "KB", "MB", "GB", "TB" };
                int order = 0;
                while (size >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    size = size / 1024;
                }

                // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
                // show a single decimal place, and no space.
                return String.Format("{0:0.##} {1}", size, sizes[order]);
            }

            public static string ConvertStringArrayToString(string[] array)
            {
                string result = null;

                array.ToList().ForEach(x => result += $"{x} ");

                return result;
            }
        }

        public static class Array
        {
            public static string[] TrimArrayStart(string[] array)
            {
                List<string> res = array.ToList();
                res.Remove(array.FirstOrDefault());

                return ListToArray(res);
            }
            public static string[] ListToArray(List<string> list)
            {
                string[] result = new string[list.Count];
                for (int i = 0; i < list.Count; i++)
                {
                    result[i] = list[i];
                }

                return result;
            }
        }
    }
}
