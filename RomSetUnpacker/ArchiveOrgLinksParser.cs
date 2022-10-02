using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RomSetUnpacker
{
    public partial class ArchiveOrgLinksParser
    {
        private const string BaseUrl = "https://archive.org";

        public static void ParseLinks(string filePath)
        {
            var files = new[]
            {
                filePath
            };
            
            var parseSettings = new ParseSettings(
                new Regex("<td><a href=\"(.*).zip\">(.*).zip</a>(.*)\n(.*)\n(.*)<td>(.*)</td>"),
                new Regex("<base href=\"(.*)\"/> "), "$1",
                "$2", "$1.zip", "$6",
                new Regex(@"\((.*?)\)"), "$1", 
                new Regex(@"^.*?(?= \()"), "$1");
            
            ParseLinks(files, parseSettings, "Links/links_{0}.txt");
        }
        
        public static void ParseLinksOld()
        {
            var files = new[]
            {
                "redump.psx.p1.htm",
                "redump.psx.p2.htm",
                "redump.psx.p3.htm",
                "redump.psx.p4.htm",
            };

            var parseSettings = new ParseSettings(
                new Regex(@"\s*(.*)<(.*).zip>(.*)\n(.*)\n\s*(.*)\n"),
                new Regex(""), "",
                "$1", "$2.zip", "$5",
                new Regex(@"\((.*?)\)"), "$1", 
                new Regex(@"(.*) \((.*)\)"), "$1");
            
            ParseLinks(files, parseSettings, "Links/links_{0}.txt");
        }
        
        private static void ParseLinks(string[] files, ParseSettings parseSettings, string outputPathFormat)
        {
            var fileEntries = new List<FileEntry>();
            foreach (var file in files)
            {
                ParseHtmlFile(file, parseSettings, fileEntries);
            }

            var fileEntriesByGroups = new Dictionary<string, List<FileEntry>>();
            SplitFileEntriesByGroup(fileEntries, fileEntriesByGroups);
            
            /*
            AddUniqueGroup(fileEntries, fileEntriesByGroups, new List<string>()
            {
                "USA",
                "Japan"
            });
            */

            fileEntriesByGroups.Add("All", fileEntries);

            foreach (var (group, entries) in fileEntriesByGroups)
            {
                if (entries.Count == 1)
                {
                    continue;
                }

                var totalSize = entries.Sum(entry => entry.SizeInMegabytes);
                var header = $"Group: {group}\nApprox. Total Size: {totalSize} MB\nLinks:\n";
                SaveFileLinks(header, entries, string.Format(outputPathFormat, group));
            }
        }

        private static void ParseHtmlFile(string filePath, ParseSettings parseSettings, ICollection<FileEntry> fileEntries)
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            var fileData = File.ReadAllText(filePath);

            var basePathMatch = parseSettings.BasePathRegex.Match(fileData);
            var basePath = basePathMatch.Result(parseSettings.BasePathPattern);
            var baseFileUrlPattern = BaseUrl + basePath + parseSettings.FileUrlPattern;
            
            var allMatches = parseSettings.EntriesRegex.Matches(fileData);
            foreach (Match match in allMatches)
            {
                var fileName = match.Result(parseSettings.FileNamePattern);
                var fileUrl = match.Result(baseFileUrlPattern);
                var fileSize = match.Result(parseSettings.FileSizePattern);

                fileEntries.Add(new FileEntry(fileName, fileUrl, fileSize, parseSettings));
            }
        }

        private static void SplitFileEntriesByGroup(List<FileEntry> fileEntries,
            IDictionary<string, List<FileEntry>> fileEntriesByGroups)
        {
            foreach (var fileEntry in fileEntries)
            {
                foreach (var groupTag in fileEntry.GroupTags)
                {
                    if (!fileEntriesByGroups.TryGetValue(groupTag, out var entries))
                    {
                        entries = new List<FileEntry>();
                        fileEntriesByGroups.Add(groupTag, entries);
                    }

                    entries.Add(fileEntry);
                }
            }
        }

        private static void SplitFileEntriesByGameName(List<FileEntry> fileEntries,
            IDictionary<string, List<FileEntry>> fileEntriesByGameName)
        {
            foreach (var fileEntry in fileEntries)
            {
                var gameName = fileEntry.GameName;
                if (!fileEntriesByGameName.TryGetValue(gameName, out var entries))
                {
                    entries = new List<FileEntry>();
                    fileEntriesByGameName.Add(gameName, entries); 
                }

                entries.Add(fileEntry);
            }
        }

        private static void AddUniqueGroup(List<FileEntry> fileEntries,
            IDictionary<string, List<FileEntry>> fileEntriesByGroups, List<string> includeGroups)
        {
            var fileEntriesByGameName = new Dictionary<string, List<FileEntry>>();
            SplitFileEntriesByGameName(fileEntries, fileEntriesByGameName);

            var uniqueFileEntries = new List<FileEntry>();

            foreach (var pair in fileEntriesByGameName)
            {
                var entries = pair.Value;
                if (entries.Count == 1)
                {
                    uniqueFileEntries.Add(entries[0]);
                }
                else
                {
                    var foundEntries = false;
                    foreach (var includeGroup in includeGroups)
                    {
                        foreach (var entry in entries)
                        {
                            if (entry.GroupTags.Contains(includeGroup))
                            {
                                uniqueFileEntries.Add(entry);
                                foundEntries = true;
                                break;
                            }
                        }

                        if (foundEntries)
                        {
                            break;
                        }
                    }
                    

                    if (!foundEntries)
                    {
                        uniqueFileEntries.Add(entries[0]);
                    }
                }
            }
            
            fileEntriesByGroups.Add("Unique", uniqueFileEntries);
        }
        
        private static void SaveFileLinks(string header, List<FileEntry> fileEntries, string outputPath)
        {
            var linksBuilder = new StringBuilder();
            linksBuilder.AppendLine(header);

            foreach (var fileEntry in fileEntries)
            {
                linksBuilder.AppendLine(fileEntry.Url);
            }

            File.WriteAllText(outputPath, linksBuilder.ToString());
        }
    }
}