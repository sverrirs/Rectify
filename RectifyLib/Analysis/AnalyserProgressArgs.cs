using System;

namespace RectifyLib.Analysis
{
    public class AnalyserProgressArgs : EventArgs
    {
        public string DirectoryName { get; }
        public string FilePath { get; }
        public int TotalFiles { get; }
        public int CompletedFiles { get; }

        public AnalyserProgressArgs(string directoryName, string filePath, int completedFiles, int totalFiles)
        {
            DirectoryName = directoryName;
            FilePath = filePath;
            CompletedFiles = completedFiles;
            TotalFiles = totalFiles;
        }
    }
}