using System.Security.AccessControl;

namespace RectifyLib.Correct
{
    public class FileCorrectionResult
    {
        /// <summary>
        /// The new path of the file
        /// </summary>
        public string NewFilePath { get; }

        /// <summary>
        /// The old path of the file (no longer valid)
        /// </summary>
        public string OldFilePath { get; }

        /// <summary>
        /// True if the operation was a success, false otherwise
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// A short description of any possible error that came up during the file corrective operation
        /// </summary>
        public string Error { get; }

        public FileCorrectionResult(string filePath, string correctedFilePath, bool success)
        {
            this.OldFilePath = filePath;
            this.NewFilePath = correctedFilePath;
            this.Success = success;
        }

        public FileCorrectionResult(string filePath, string correctedFilePath, bool success, string error) 
            : this(filePath, correctedFilePath, success)
        {
            this.Error = error;
        }
    }
}