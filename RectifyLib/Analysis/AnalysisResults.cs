using System.Collections.Generic;

namespace RectifyLib.Analysis
{
    public class AnalysisResults
    {
        private readonly List<DirectoryAnalysis> _directories = new List<DirectoryAnalysis>();

        public DirectoryAnalysis[] Directories => _directories.ToArray();

        public void AddDirectory(DirectoryAnalysis dirInfo)
        {
            _directories.Add(dirInfo);
        }
    }
}