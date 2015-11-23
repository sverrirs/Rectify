using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TagLib;
using TagLib.Mpeg4;
using File = System.IO.File;

namespace RectifyUI
{
    public class Analyser
    {
        // Current UI TaskScheduler
        private TaskScheduler _uiScheduler;
       
        private static log4net.ILog Log = log4net.LogManager.GetLogger(typeof(Program));

        private CancellationTokenSource _cancelSource = null;

        #region Events

        public event EventHandler<AnalyserProgressArgs> AnalysisProgress;

        protected virtual void OnAnalysisProgress(AnalyserProgressArgs e)
        {
            AnalysisProgress?.Raise(this, e);
            // Raise the event handler on the UI thread
           /* Task.Factory.StartNew(
                action: () =>
                {
                    handler?.Invoke(this, localArgs);
                }, 
                cancellationToken: new CancellationToken(),
                creationOptions: TaskCreationOptions.None,
                scheduler: _uiScheduler);*/
        }

        #endregion
        
        public Analyser(TaskScheduler uiScheduler = null )
        {
            _uiScheduler = uiScheduler ?? TaskScheduler.Current;
        }

        public void CancelAnalysis()
        {
            if(_cancelSource != null && !_cancelSource.IsCancellationRequested )
                _cancelSource.Cancel();
        }

        public Task<AnalysisResults> RunAnalysisAsync(string libraryPath)
        {
            // Create a new cancellation source for the task
            _cancelSource?.Dispose();
            _cancelSource = new CancellationTokenSource();
            var cancellationToken = _cancelSource.Token;

            return new TaskFactory<AnalysisResults>().StartNew(() =>
            {
                    var path = libraryPath;

                    // First we run a pre-analysis to figure out the number of files etc
                    var preAnalysis = RunAnalysis(path, cancellationToken);

                    // Run a full analysis now
                    return RunAnalysis(path, cancellationToken, preAnalysis);
                }, 
                cancellationToken);
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
                        if (cancellationToken.IsCancellationRequested)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                        }

                        if (!this.ShouldAnalyseFile(filePath))
                            continue;

                        // Only create a full file analysis if enabled, otherwise just produce a simple file information
                        if (isFullAnalysis)
                        {
                            // Increment the files processed count
                            ++currentFileCount;

                            // Indicate processing only when doing full analysis after a pre-analysis has been performed
                            OnAnalysisProgress(new AnalyserProgressArgs(dirInfo.Name, filePath, currentFileCount, totalNumberOfFiles));

                            dirInfo.AddFile(this.AnalyseFile(filePath, dirInfo));
                        }
                        else
                        {
                            dirInfo.AddFile(new FileAnalysis(dirInfo, filePath));
                        }
                    }

                    // Save the full directory info for the results
                    results.AddDirectory(dirInfo);

                    break;
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

            //Console.WriteLine("File: " + filePath);
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
                        Console.WriteLine("Unknown error in Taglib: "+e);*/
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
                        //Console.WriteLine("Parsed date '{0}' from VIDEO filename '{1}'", dateTaken.ToString("yyyy/MM/dd HH:mm:ss"), fileName);
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

    public class AnalysisCompletedArgs : EventArgs
    {
        /// <summary>
        /// Indicates the termination state of the analysis. Only valid values are
        /// <see cref="TaskStatus.RanToCompletion" />, <see cref="TaskStatus.Canceled" /> and <see cref="TaskStatus.Faulted" />
        /// </summary>
        public TaskStatus Status { get; }

        public AnalysisCompletedArgs(TaskStatus status)
        {
            Status = status;
        }
    }

    public class AnalysisConfiguration
    {
        public bool PreAnalysis { get; }

        public AnalysisConfiguration(bool preAnalysis)
        {
            PreAnalysis = preAnalysis;
        }
    }

    public class AnalysisResults
    {
        private readonly List<DirectoryAnalysis> _directories = new List<DirectoryAnalysis>();

        public DirectoryAnalysis[] Directories => _directories.ToArray();

        public void AddDirectory(DirectoryAnalysis dirInfo)
        {
            _directories.Add(dirInfo);
        }
    }

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
