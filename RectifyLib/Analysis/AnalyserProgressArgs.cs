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

        public DateTime StartTime { get; }

        public TimeSpan RemainingTime { get; }

        public AnalyserProgressArgs(string directoryName, string filePath, int completedFiles, int totalFiles, DateTime startTime)
            :base(completedFiles, totalFiles)
        {
            DirectoryName = directoryName;
            FilePath = filePath;
            StartTime = startTime;

            // Calc estimated remaining, estimate how many millisec per file and multiply
            if (completedFiles > 0)
            {
                var elapsed = DateTime.Now - startTime;
                var msecPerFile = elapsed.TotalMilliseconds / completedFiles;
                RemainingTime = TimeSpan.FromMilliseconds(msecPerFile*(totalFiles - completedFiles));
            }
            else
            {
                RemainingTime = TimeSpan.Zero;
            }
        }
    }
}