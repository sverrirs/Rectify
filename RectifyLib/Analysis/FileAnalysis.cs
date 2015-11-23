using System;
using System.IO;

namespace RectifyLib.Analysis
{
    public class FileAnalysis
    {
        public DirectoryAnalysis Parent { get; }

        public string FilePath { get; }

        public DateTime DateCategoryCurrently { get; }

        public DateTime DateCategoryDetected { get; }

        /// <summary>
        /// Gets if the file is correctly placed in the library based on detected date
        /// </summary>
        public bool IsCorrect => this.DateCategoryCurrently.Date == this.DateCategoryDetected.Date;

        public string CorrectedFilePath { get; }

        public bool HasCorrectedFilePath => !string.IsNullOrEmpty(this.CorrectedFilePath);

        public FileAnalysis(DirectoryAnalysis parent, string filePath)
        {
            Parent = parent;
            FilePath = filePath;
            DateCategoryCurrently = DateCategoryDetected = DateTime.MinValue;
        }

        public FileAnalysis(DirectoryAnalysis parent, string filePath, DateTime dateCategoryCurrently, DateTime dateCategoryDetected)
            : this(parent, filePath)
        {
            DateCategoryCurrently = dateCategoryCurrently;
            DateCategoryDetected = dateCategoryDetected;

            if (DateCategoryDetected.Date != DateCategoryCurrently.Date)
            {
                var correctDir = $"{parent.PathToDir}\\{DateCategoryDetected.ToString("yyyy-MM-dd")}\\";

                // If destination already has file then preserve with a unique file name
                CorrectedFilePath = correctDir + Path.GetFileName(filePath);
                var hasConflict = File.Exists(CorrectedFilePath);

                // Rename the file with a GUID if there is a naming conflict in the new category folder
                if (hasConflict)
                {
                    CorrectedFilePath = $"{correctDir}{Path.GetFileNameWithoutExtension(filePath)}_{Guid.NewGuid()}{Path.GetExtension(filePath)}";
                    if (File.Exists(CorrectedFilePath))
                    {
                        CorrectedFilePath = null; // Don't move it if no new file path can be assigned
                        // ERROR!!!
                        //throw new InvalidOperationException($"Unable to resolve file conflict as a file with the same resolution name already exists. This is a development issue, please consider raising an issue. File path: '{newFilePath}'");
                    }
                }
            }
        }
    }
}