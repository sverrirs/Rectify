using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RectifyLib.Analysis
{
    public class DirectoryAnalysis
    {
        private readonly List<FileAnalysis> _files = new List<FileAnalysis>();

        /// <summary>
        /// The full directory path
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// A split version of the full directory path from <see cref="Path"/>
        /// </summary>
        public string[] PathSplit { get; }

        /// <summary>
        /// The full path to the directory containing our directory (one level up from <see cref="Path"/>)
        /// </summary>
        public string PathToDir { get; }

        /// <summary>
        /// Name of the directory (not path)
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Suffix value if any trails the date value
        /// E.g. for the directory name "2015-01-23 The kids came to visit", the suffix would be "The kids came to visit"
        /// </summary>
        public string Suffix { get; }

        /// <summary>
        /// The raw date value extracted from the directory name.
        /// E.g. for the directory name "2015-01-23 The kids came to visit", the DateRaw would be "2015-01-23"
        /// </summary>
        public string DateRaw { get; }

        /// <summary>
        /// The parsed date from the directory name
        /// </summary>
        public DateTime DateCategory { get; }

        /// <summary>
        /// The image and video files identified in the directory
        /// </summary>
        public FileAnalysis[] Files => _files.ToArray();

        public DirectoryAnalysis(string fullPathRaw)
        {
            Path = fullPathRaw;
            PathSplit = fullPathRaw.Split(new[] { System.IO.Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            Name = PathSplit.Last();
            DateRaw = Name;
            Suffix = string.Empty;
            PathToDir = string.Join(System.IO.Path.DirectorySeparatorChar.ToString(), PathSplit.Take(PathSplit.Length - 1).ToArray());

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