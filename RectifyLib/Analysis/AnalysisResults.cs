using System;
using System.Collections.Generic;

namespace RectifyLib.Analysis
{
    public class AnalysisResults
    {
        private readonly List<DirectoryAnalysis> _directories = new List<DirectoryAnalysis>();

        public DirectoryAnalysis[] Directories => _directories.ToArray();

        public Dictionary<DateTime, List<DirectoryAnalysis>> Suffixes { get; } = new Dictionary<DateTime, List<DirectoryAnalysis>>();

        public void AddDirectory(DirectoryAnalysis dirInfo)
        {
            _directories.Add(dirInfo);

            // if the directory has a suffix then add it to the list of possible suffixes to use!
            // this can be used in the UI to allow users to choose from different folders for the same
            // date in the destination
            if (!string.IsNullOrWhiteSpace(dirInfo.Suffix))
            {
                if( !Suffixes.ContainsKey(dirInfo.DateCategory.Date))
                    Suffixes[dirInfo.DateCategory.Date] = new List<DirectoryAnalysis>();

                Suffixes[dirInfo.DateCategory.Date].Add(dirInfo);
            }
        }
    }
}