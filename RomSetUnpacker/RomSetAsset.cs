using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace RomSetUnpacker
{
    public class RomSetAsset : Asset
    {
        public delegate void ProgressUpdate(int currentProgress, int progressSize);

        private readonly List<string> _regionsCache = new List<string>();

        public void Unpack(string inputPath, string baseOutputPath, ProgressUpdate progressUpdate = null)
        {
            ReadZipFile(inputPath, zipArchive =>
            {
                var entries = zipArchive.Entries;
                for (int i = 0, size = entries.Count; i < size; i++)
                {
                    var romEntry = entries[i];

                    var region = GetRegion(romEntry.Name);
                    var outputPath = CreateFolder(baseOutputPath, region);
                    
                    var (baseGroupKey, groupKey) = GetGroupKey(romEntry.Name);
                    outputPath = CreateFolder(outputPath, baseGroupKey);
                    
                    using (var zipRomStream = romEntry.Open())
                    {
                        var outputDirectory = Path.Combine(outputPath, groupKey);
                        WriteUnzippedRomFile(zipRomStream, outputDirectory);
                    }
                    
                    progressUpdate?.Invoke(i + 1, size);
                }
            });
        }

        private static string CreateFolder(string baseOutputPath, string folderName)
        {
            if (string.IsNullOrEmpty(folderName))
            {
                return baseOutputPath;
            }
            
            var outputPath = Path.Combine(baseOutputPath, folderName);
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            return outputPath;
        }

        private static (string baseGroupKey, string groupkey) GetGroupKey(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return (string.Empty, "Unknown");
            }

            var firstChar = name[0];
            if (char.IsDigit(firstChar) || char.IsPunctuation(firstChar))
            {
                return (string.Empty, "0-9");
            }

            string key;
            if (name.Length > 1)
            {
                var secondChar = name[1];
                if (char.IsLetter(secondChar))
                {
                    key = string.Concat(firstChar, secondChar);
                }
                else
                {
                    key = string.Concat(firstChar, "A");
                }
            }
            else
            {
                key = string.Concat(firstChar, "A");
            }

            return (firstChar.ToString().ToUpper(), key.ToUpper());
        }

        private string GetRegion(string archiveName)
        {
            ParseGroups(archiveName, _regionsCache);
            return _regionsCache.Count > 0 ? _regionsCache[0] : "Unknown";
        }
        
        private static void ParseGroups(string archiveName, List<string> groups)
        {
            groups.Clear();
            for (var i = 0; i < archiveName.Length; i++)
            {
                var c = archiveName[i];
                if (c != '(')
                {
                    continue;
                }
                
                i++;
                var startIdx = i;
                while (c != ')' && i < archiveName.Length)
                {
                    c = archiveName[i];
                    i++;
                }
                var endIdx = i - 1;

                var group = archiveName.Substring(startIdx, endIdx - startIdx);
                groups.Add(group);
            }
        }
        
        protected static void WriteUnzippedRomFile(Stream zipRomStream, string outputDirectory)
        {
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            using (var romZipArchive = new ZipArchive(zipRomStream, ZipArchiveMode.Read))
            {
                var romEntries = romZipArchive.Entries;
                for (int i = 0, size = romEntries.Count; i < size; i++)
                {
                    var entry = romEntries[i];
                    using (var romStream = entry.Open())
                    {
                        var path = Path.Combine(outputDirectory, entry.FullName);
                        using (var romWriter = new FileStream(path, FileMode.Create, FileAccess.Write))
                        {
                            romStream.CopyTo(romWriter);
                        }
                    }
                }
            }
        }
    }
}