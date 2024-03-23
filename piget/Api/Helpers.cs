using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace piget.Api
{
    public class Helpers
    {
        public static string StringToMD5(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString().Substring(0, 16); // Получаем только первые 16 символов хэша
            }
        }

        public static void CreateFileWithText(string path, string text)
        {
            File.Create(path).Dispose();
            File.WriteAllText(path, text);
        }
        public static void DownloadWithProgress(string url, string path)
        {
            Console.CursorVisible = false;

            ActionAnswer.WriteColorLine(new Dictionary<string, ConsoleColor> { 
                { "Скачивание ", System.Console.ForegroundColor }, { url + Environment.NewLine, ConsoleColor.Blue } });

            // Скачивание
            Stopwatch stopwatch = new Stopwatch();
            WebClient webClient = new WebClient();
            webClient.DownloadProgressChanged += (_s, _e) =>
            {
                if (stopwatch.ElapsedMilliseconds > 200)
                {
                    WriteProgressLine(_e.ProgressPercentage, 20, _e);

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

        public static void WriteProgressLine(int percent, int segmentsCount, DownloadProgressChangedEventArgs e)
        {
            int currentSegmentsCount = percent / (100 / segmentsCount);
            int currentHideSegmentsCount = segmentsCount - currentSegmentsCount;

            string progressBar = 
                $"\r  {string.Concat(Enumerable.Repeat("█", currentSegmentsCount))}{string.Concat(Enumerable.Repeat("░", currentHideSegmentsCount))}";

            string size = e == null ? null : 
                $"{Helpers.SizeToStringFormat(e.BytesReceived)} / {Helpers.SizeToStringFormat(e.TotalBytesToReceive)}";

            Console.Write($"{progressBar} {size}        ");
        }

        public static Int32 GetPercent(Int32 b, Int32 a)
        {
            if (b == 0) return 0;

            return (Int32)(a / (b / 100M));
        }

        public static void ExtractToDirectory(string archivePath, string outDir)
        {
            ActionAnswer.WriteColorLine(new Dictionary<string, ConsoleColor> {
                { "Распаковка ", System.Console.ForegroundColor }, { archivePath + Environment.NewLine, ConsoleColor.Blue } });

            ZipFile.ExtractToDirectory(archivePath, outDir);
        }

        public static void SavePackageNotify(string dir)
        {
            ActionAnswer.WriteColorLine(new Dictionary<string, ConsoleColor> {
                { "Ресурсы сохранены в ", System.Console.ForegroundColor }, { dir + Environment.NewLine, ConsoleColor.Blue } });
        }

        public static void ResourceAuthenticationDisabledNotify() =>
            ActionAnswer.Log("<?> ", "Аутентификация хэшей ресурсов отключена.");

        public static void ResourcesAlreadyLoadedNotify() =>
            ActionAnswer.Log("<?> ", "Скрипт будет инициализирован с сохраненными ресурсами.");

        public static void UnknownPublisherNotify() =>
            ActionAnswer.Log("<?> ", "Издатель PIGET (QISL) не несет ответственности за сторонние сценарии и не предоставляет для них никакие лицензии.");

        public static void ProcessStartNotify(int id) =>
            ActionAnswer.Log("<?> ", $"Процесс инициализируется со следующим идентификатором: {id}.");

        public static void LogError(string title, Exception ex)
        {
            ColorConsole.WriteLine($"{title}\r\n", ConsoleColor.Red);
            ColorConsole.WriteLine($"{ex}\r\n", ConsoleColor.DarkGray);
        }

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

        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int description, int reservedValue);

        public static bool IsInternetAvailable()
        {
            int description;
            return InternetGetConnectedState(out description, 0);
        }

        public static string ConvertStringArrayToString(string[] array)
        {
            string result = null;

            array.ToList().ForEach(x => result += $"{x} ");

            return result;
        }
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
