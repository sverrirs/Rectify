using System;

namespace RectifyLib.Analysis
{
    public class AnalyserProgressArgs : BackgroundProgressArgs
    {
        /// <summary>
        /// The name of the directory that is currently being analysed
        /// </summary>
        public string DirectoryName { get; }

        /// <summary>
        /// The full file path of the file currently being analysed
        /// </summary>
        public string FilePath { get; }

        public AnalyserProgressArgs(string directoryName, string filePath, int completedFiles, int totalFiles)
            :base(completedFiles, totalFiles)
        {
            DirectoryName = directoryName;
            FilePath = filePath;
        }
    }
}