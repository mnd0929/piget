using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace piget.Api
{
    public class HashManager
    {
        public static string GetScriptLibraryHash(PigetScriptLibrary pigetScriptLibrary) =>
            StringToMD5(pigetScriptLibrary.Url);

        public static string GetScriptLibraryHash(string url) =>
            StringToMD5(url);

        public static string GetScriptHash(PigetScript pigetScript) =>
            StringToMD5(string.Concat(pigetScript.Name, pigetScript.InitialScript, pigetScript.Resources));

        public static string GetScriptHash(string manifestPath)
        {
            PigetScript pigetScript = JsonSerializer.Deserialize<PigetScript>(File.ReadAllText(manifestPath));
            return StringToMD5(string.Concat(pigetScript.Name, pigetScript.InitialScript, pigetScript.Resources));
        }

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
    }
}
