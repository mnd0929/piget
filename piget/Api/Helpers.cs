using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
            // Скачивание
            Stopwatch stopwatch = new Stopwatch();
            WebClient webClient = new WebClient();
            webClient.DownloadProgressChanged += (_s, _e) =>
            {
                if (stopwatch.ElapsedMilliseconds > 100)
                {
                    Console.Write($"\rПолучение данных с {new Uri(url).Host}: {SizeToStringFormat(_e.BytesReceived)} / {SizeToStringFormat(_e.TotalBytesToReceive)} ({_e.ProgressPercentage}%)    ");

                    stopwatch.Restart();
                }
            };
            stopwatch.Start();
            webClient.DownloadFileAsync(new Uri(url), path);

            // Ожидание
            while (webClient.IsBusy)
                Thread.Sleep(200);

            stopwatch.Stop();
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
