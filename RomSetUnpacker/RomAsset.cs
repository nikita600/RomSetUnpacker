using System.Collections.Generic;
using System.IO.Compression;

namespace RomSetUnpacker
{
    public class RomAsset : Asset
    {
        private readonly IReadOnlyList<string> _groups;
        private readonly ZipArchiveEntry _romArchiveEntry;

        public string Region => _groups.Count > 0 ? _groups[0] : "Unknown";
        
        public IReadOnlyList<string> Groups => _groups;

        public RomAsset(ZipArchiveEntry zipArchiveEntry)
        {
            _romArchiveEntry = zipArchiveEntry;
            _groups = ParseGroups(zipArchiveEntry.Name);
        }

        private static IReadOnlyList<string> ParseGroups(string archiveName)
        {
            var groups = new List<string>();
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

            return groups;
        }
    }
}