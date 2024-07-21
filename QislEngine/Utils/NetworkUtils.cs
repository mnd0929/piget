using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;

namespace QislEngine.Utils
{
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
}
