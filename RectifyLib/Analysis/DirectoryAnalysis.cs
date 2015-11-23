using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RectifyLib.Analysis
{
    public class DirectoryAnalysis
    {
        private readonly List<FileAnalysis> _files = new List<FileAnalysis>();
        public string Path { get; }

        public string[] PathSplit { get; }

        public string PathToDir { get; }

        public string Name { get; }
        public string Suffix { get; }
        public string DateRaw { get; }
        public DateTime DateCategory { get; }

        public FileAnalysis[] Files => _files.ToArray();

        public DirectoryAnalysis(string fullPathRaw)
        {
            Path = fullPathRaw;
            PathSplit = fullPathRaw.Split(new[] { System.IO.Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            Name = PathSplit.Last();
            DateRaw = Name;
            Suffix = string.Empty;
            PathToDir = System.IO.Path.Combine(PathSplit.Take(PathSplit.Length - 1).ToArray());

            var dirSpaceIdx = Name.IndexOf(' ');
            if (dirSpaceIdx != -1)
            {
                DateRaw = Name.Substring(0, dirSpaceIdx);
                Suffix = Name.Substring(dirSpaceIdx);
            }

            DateTime parsedDate;
            if (DateTime.TryParseExact(DateRaw, "yyyy-MM-dd", CultureInfo.CurrentCulture, DateTimeStyles.None, out parsedDate))
            {
                DateCategory = parsedDate;
            }
            else
            {
                DateCategory = DateTime.MinValue;
            }
                
        }

        public void AddFile(FileAnalysis fileInfo)
        {
            _files.Add(fileInfo);
        }
    }
}