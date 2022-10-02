using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace RomSetUnpacker
{
    public partial class ArchiveOrgLinksParser
    {
        private class FileEntry
        {
            public readonly string Name;
            public readonly string Url;
            public readonly string Size;

            public readonly int SizeInMegabytes;
            public readonly IReadOnlyList<string> GroupTags;

            public readonly string GameName;
            
            public FileEntry(string name, string url, string size, ParseSettings parseSettings)
            {
                Name = name;
                Url = url;
                Size = size;

                var gameNameMatch = parseSettings.GameNameRegex.Match(name);
                GameName = gameNameMatch.ToString();

                var groups = new List<string>();
                var groupMatches = parseSettings.GroupRegex.Matches(name);
                foreach (Match match in groupMatches)
                {
                    var group = match.Result(parseSettings.GroupPattern);
                    groups.Add(group);
                }

                GroupTags = groups;

                if (Size.EndsWith("G"))
                {
                    SizeInMegabytes = GetSize("G", 1f / 1024f);
                }
                else if (Size.EndsWith("M"))
                {
                    SizeInMegabytes = GetSize("M", 1f);
                }
                else if (Size.EndsWith("K"))
                {
                    SizeInMegabytes = GetSize("K", 1024f);
                }
                else
                {
                    throw new Exception("Unknown FileSize Format: " + Size + ", Name: " + Name);
                }
            }

            private int GetSize(string replace, float divider)
            {
                var sizeString = Size.Replace(replace, string.Empty);
                if (float.TryParse(sizeString, NumberStyles.Any, CultureInfo.InvariantCulture, out var size))
                {
                    return Convert.ToInt32(Math.Ceiling(size / divider));
                }

                throw new Exception("Fail on parse: " + sizeString + ", Name: " + Name);
            }

            public override string ToString()
            {
                return GameName;
            }
        }
    }
}