using System.Text.RegularExpressions;

namespace RomSetUnpacker
{
    public partial class ArchiveOrgLinksParser
    {
        private class ParseSettings
        {
            public readonly Regex EntriesRegex;
            
            public readonly string FileNamePattern;
            public readonly string FileUrlPattern;
            public readonly string FileSizePattern;

            public readonly Regex GameNameRegex;
            public readonly string GameNamePattern;
            
            public readonly Regex GroupRegex;
            public readonly string GroupPattern;
            
            public readonly Regex BasePathRegex;
            public readonly string BasePathPattern;

            public ParseSettings(Regex entriesRegex, Regex basePathRegex, string basePathPattern,
                string fileNamePattern, string fileUrlPattern, string fileSizePattern,
                Regex groupRegex, string groupPattern, Regex gameNameRegex, string gameNamePattern)
            {
                EntriesRegex = entriesRegex;

                FileNamePattern = fileNamePattern;
                FileUrlPattern = fileUrlPattern;
                FileSizePattern = fileSizePattern;
                
                GroupRegex = groupRegex;
                GroupPattern = groupPattern;
                
                BasePathRegex = basePathRegex;
                BasePathPattern = basePathPattern;

                GameNameRegex = gameNameRegex;
                GameNamePattern = gameNamePattern;
            }
        }
    }
}