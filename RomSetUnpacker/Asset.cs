using System;
using System.IO;
using System.IO.Compression;

namespace RomSetUnpacker
{
    public abstract class Asset
    {
        public static bool IsZipArchive(string path)
        {
            return !string.IsNullOrEmpty(path) && path.EndsWith(".zip");
        }

        protected static void ReadZipFile(string zipPath, Action<ZipArchive> onRead)
        {
            try
            {
                using (var fileStream = new FileStream(zipPath, FileMode.Open, FileAccess.Read))
                {
                    using (var zipArchive = new ZipArchive(fileStream))
                    {
                        onRead.Invoke(zipArchive);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error reading ZIP file at: " + zipPath + "\n" +
                                  "Reason: " + e.Message);
            }
        }
    }
}