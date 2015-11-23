using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TagLib;

namespace RectifyLib.Analysis
{
    public class Analyser : AsyncBackgroundProcessor<AnalysisResults, AnalyserProgressArgs, string>
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger(typeof(Analyser));

        /// <summary>
        /// Runs an analysis asynchronously for the given <see cref="libraryPath"/> directory path.
        /// The analysis validates that the image and video files found belong to folders which names
        /// correspond to the actual picture shooting date (based on EXIF, file name and other values stored in the files).
        /// </summary>
        /// <param name="libraryPath">The directory to analyse for incorrect file placements</param>
        protected override Func<AnalysisResults> CreateAsyncProcess(string libraryPath, CancellationToken cancellationToken)
        {
            return () =>
            {
                var path = libraryPath;

                // First we run a pre-analysis to figure out the number of files etc
                var preAnalysis = RunAnalysis(path, cancellationToken);

                // Run a full analysis now
                return RunAnalysis(path, cancellationToken, preAnalysis);
            };
        }

        private AnalysisResults RunAnalysis(string libraryPath, CancellationToken cancellationToken, AnalysisResults preAnalysisResults = null)
        {
            var isFullAnalysis = preAnalysisResults != null;
            int totalNumberOfFiles = 0;
            if (isFullAnalysis)
            {
                totalNumberOfFiles = preAnalysisResults?.Directories.Sum(x => x.Files?.Length) ?? 0;
            }

            var results = new AnalysisResults();
            try
            {
                int currentFileCount = 0;
                foreach (var directoryPath in Directory.EnumerateDirectories(libraryPath))
                {
                    var dirInfo = new DirectoryAnalysis(directoryPath);

                    // If we cannot construct a directory info then skip the directory
                    if (!this.ShouldAnalyseDirectory(dirInfo))
                        continue;

                    foreach (var filePath in Directory.EnumerateFiles(dirInfo.Path))
                    {
                        // Check if cancelled and exit if so
                        cancellationToken.ThrowIfCancellationRequested();

                        if (!this.ShouldAnalyseFile(filePath))
                            continue;

                        // Only create a full file analysis if enabled, otherwise just produce a simple file information
                        if (isFullAnalysis)
                        {
                            // Increment the files processed count
                            ++currentFileCount;

                            // Indicate processing only when doing full analysis after a pre-analysis has been performed
                            OnBackgroundProgress(new AnalyserProgressArgs(dirInfo.Name, filePath, currentFileCount, totalNumberOfFiles));

                            dirInfo.AddFile(this.AnalyseFile(filePath, dirInfo));
                        }
                        else
                        {
                            dirInfo.AddFile(new FileAnalysis(dirInfo, filePath));
                        }
                    }

                    // Save the full directory info for the results
                    results.AddDirectory(dirInfo);
                }

                return results;
            }
            catch (OperationCanceledException cex)
            {
                Log.Warn("Operation cancelled.", cex);
                return results;
            }
            catch (Exception e)
            {
                Log.Error("Error during analysis, aborted.", e);
                throw;
            }
        }

        private bool ShouldAnalyseDirectory(DirectoryAnalysis dirInfo)
        {
            // Skip hidden and system directories that might be embedded
            if (dirInfo.Name.StartsWith("."))
                return false;

            // If we can't parse a date from the directory name then cancel
            if (dirInfo.DateCategory == DateTime.MinValue)
                return false;
            
            if (!dirInfo.Name.StartsWith("2015-"))
                return false;

            // All is OK, parse the directory
            return true;
        }

        private bool ShouldAnalyseFile(string filePath)
        {
            var extension = Path.GetExtension(filePath);

            if (extension == null || extension.EndsWith("ini"))
            {
                return false;
            }

            return true;
        }

        private FileAnalysis AnalyseFile(string filePath, DirectoryAnalysis dirInfo)
        {
            Log.Info($"Folder: {dirInfo.Path}");
            Log.Info($"Date: {dirInfo.DateCategory}");

            DateTime correctDateCategory;
            if (this.IsFileDateCategoryCorrect(dirInfo.DateCategory, filePath, out correctDateCategory))
                correctDateCategory = dirInfo.DateCategory;

            return new FileAnalysis(dirInfo, filePath, dirInfo.DateCategory, correctDateCategory);
        }

        private bool IsFileDateCategoryCorrect(DateTime currentDateCategory, string filePath, out DateTime correctDateCategory)
        {
            correctDateCategory = currentDateCategory;

            //Log.Debug("File: " + filePath);
            DateTime dateTaken = DateTime.MinValue;

            // Apple MOV and Panasonic MTS formats are not supported, use the fallback for those files!
            if (
                !filePath.EndsWith(".mov", StringComparison.OrdinalIgnoreCase) &&
                !filePath.EndsWith(".mts", StringComparison.OrdinalIgnoreCase)
                )
            {
                try
                {
                    var tagFile = TagLib.File.Create(filePath);
                    if (tagFile == null)
                        return false;

                    var imageFile = tagFile as TagLib.Image.File;
                    if (imageFile != null)
                    {
                        if (imageFile.ImageTag.Exif != null && imageFile.ImageTag.Exif.DateTimeOriginal != null)
                            dateTaken = imageFile.ImageTag.Exif.DateTimeOriginal.Value;
                    }
                }
                catch (Exception e)
                {
                    if (e is UnsupportedFormatException)
                        Log.Error("Error in IsFileDateCategoryCorrect", e);
                    /*else 
                        Log.Warn("Unknown error in Taglib",e);*/
                    // Continue on...
                }
            }

            // If the date is still wrong then attempt to parse it from the file name
            // NOTE: Currently only do this for video files
            if (dateTaken == DateTime.MinValue &&
                filePath.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase)
                )
            {
                // Videos from phones are on the form: VID_20150428_112604.mp4
                var fileName = Path.GetFileName(filePath);
                if (fileName.StartsWith("VID_", StringComparison.OrdinalIgnoreCase))
                {
                    if (DateTime.TryParseExact(Path.GetFileNameWithoutExtension(fileName.Substring(4)),
                                               "yyyyMMdd_HHmmss",
                                               CultureInfo.InvariantCulture,
                                               DateTimeStyles.None,
                                               out dateTaken))
                    {
                        //Log.Debug("Parsed date '{0}' from VIDEO filename '{1}'", dateTaken.ToString("yyyy/MM/dd HH:mm:ss"), fileName);
                    }
                }
            }

            // If no EXIF info could be read then use the last modified time for the file
            if (dateTaken == DateTime.MinValue)
            {
                var fInfo = new FileInfo(filePath);
                if (fInfo.CreationTime != fInfo.LastWriteTime && fInfo.LastWriteTime < fInfo.CreationTime)
                    dateTaken = fInfo.LastWriteTime;
            }

            // If no date could be determined for the file then just exit and assume that 
            // whatever process imported it did it correctly as we know no better!
            if (dateTaken == DateTime.MinValue)
                return true;

            bool isCorrect = dateTaken.Date == currentDateCategory.Date;
            if (!isCorrect)
                correctDateCategory = dateTaken.Date;

            return isCorrect;
        }
    }
}
