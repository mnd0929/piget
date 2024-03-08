using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace piget.Api
{
    public class HashManager
    {
        public static string GetScriptLibraryHash(PigetScriptLibrary pigetScriptLibrary) =>
            Helpers.StringToMD5(pigetScriptLibrary.Url);

        public static string GetScriptLibraryHash(string url) =>
            Helpers.StringToMD5(url);

        public static string GetScriptHash(PigetScript pigetScript) =>
            Helpers.StringToMD5(string.Concat(pigetScript.Name, pigetScript.InitialScript, pigetScript.Resources));

        public static string GetScriptHash(string manifestPath)
        {
            PigetScript pigetScript = JsonSerializer.Deserialize<PigetScript>(File.ReadAllText(manifestPath));
            return Helpers.StringToMD5(string.Concat(pigetScript.Name, pigetScript.InitialScript, pigetScript.Resources));
        }  
    }
}
