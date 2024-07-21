using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;

namespace QislEngine.Utils
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
}
