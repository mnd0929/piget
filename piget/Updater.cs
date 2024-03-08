using piget.Api;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace piget
{
    public class Updater
    {
        public const string PigetLibraryStd = "https://raw.githubusercontent.com/mnd0929/piget-library/main/library.json";
        public const string PigetLatestVersionCodeUrl = "https://raw.githubusercontent.com/mnd0929/api-apps/main/piget-last.pinfo";
        public const string PigetLatestVersionUpdateCode = "https://raw.githubusercontent.com/mnd0929/api-apps/main/piget-updatecommand.pinfo";
        public const string PigelLatestVersionExecutable = "https://github.com/mnd0929/piget/releases/latest/download/piget.exe";

        public static KeyValuePair<bool, string> CheckUpdates()
        {
            string latestVersion = new HttpClient().GetStringAsync(PigetLatestVersionCodeUrl).Result;
            string currentVersion = Program.Version;

            return new KeyValuePair<bool, string>(latestVersion == currentVersion, latestVersion);
        }
        public static void Update()
        {
            string updateCommand = new HttpClient().GetStringAsync(PigetLatestVersionUpdateCode).Result;

            Helpers.CreateFileWithText("updater.bat", updateCommand);

            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "updater.bat",
                    Arguments = $"/c {updateCommand}"
                }
            };

            process.Start();

            Environment.Exit(0);
        }
    }
}
