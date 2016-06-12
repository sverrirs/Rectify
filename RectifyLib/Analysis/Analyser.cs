using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TagLib;

namespace RectifyLib.Analysis
{
    public class Analyser : AsyncBackgroundProcessor<AnalysisResults, AnalyserProgressArgs, AnalyserStartupArgs>
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger(typeof(Analyser));

        /// <summary>
        /// Runs an analysis asynchronously for the given startup arguments
        /// The analysis validates that the image and video files found belong to folders which names
        /// correspond to the actual picture shooting date (based on EXIF, file name and other values stored in the files).
        /// </summary>
        protected override Func<AnalysisResults> CreateAsyncProcess(AnalyserStartupArgs startupArgs, CancellationToken cancellationToken)
        {
            return () =>
            {
                var args = startupArgs;

                // First we run a pre-analysis to figure out the number of files etc
                var preAnalysis = RunAnalysis(args, cancellationToken);

                // Run a full analysis now
                return RunAnalysis(args, cancellationToken, preAnalysis);
            };
        }

        private AnalysisResults RunAnalysis(AnalyserStartupArgs startupArgs, CancellationToken cancellationToken, AnalysisResults preAnalysisResults = null)
        {
            var startTime = DateTime.Now;

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
                foreach (var directoryPath in Directory.EnumerateDirectories(startupArgs.Path))
                {
                    var dirInfo = new DirectoryAnalysis(directoryPath);

                    // If we cannot construct a directory info then skip the directory
                    if (!this.ShouldAnalyseDirectory(dirInfo))
                        continue;

                    foreach (var filePath in Directory.EnumerateFiles(dirInfo.Path))
                    {
                        // Check if cancelled and exit if so
                        cancellationToken.ThrowIfCancellationRequested();

                        if (!this.ShouldAnalyseFile(filePath, startupArgs))
                            continue;

                        // Only create a full file analysis if enabled, otherwise just produce a simple file information
                        if (isFullAnalysis)
                        {
                            // Increment the files processed count
                            ++currentFileCount;

                            // Indicate processing only when doing full analysis after a pre-analysis has been performed
                            OnBackgroundProgress(new AnalyserProgressArgs(dirInfo.Name, filePath, currentFileCount, totalNumberOfFiles, startTime));

                            var fileAnalysis = this.AnalyseFile(filePath, dirInfo, startupArgs);
                            if( fileAnalysis != null )
                                dirInfo.AddFile(fileAnalysis);
                        }
                        else
                        {
                            dirInfo.AddFile(new FileAnalysis(dirInfo, filePath));
                            // Also store a hash of the directory date and the suffix if there is one
                        }
                    }

                    // Save the full directory info for the results
                    results.AddDirectory(dirInfo);
                }

                // If we're dealing with a full analysis then after everything has been analysed
                // a final pass through all the data is needed to see if any suffixes need propogating 
                // to new destination folders.
                // if (isFullAnalysis)
                // TODO: I'm not convinced of this yet!

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
            
            /*if (!dirInfo.Name.StartsWith("2015-"))
                return false;*/

            // All is OK, parse the directory
            return true;
        }

        private bool ShouldAnalyseFile(string filePath, AnalyserStartupArgs startupArgs)
        {
            var extension = Path.GetExtension(filePath);

            if (extension == null || extension.EndsWith("ini"))
            {
                return false;
            }

            return true;
        }

        private FileAnalysis AnalyseFile(string filePath, DirectoryAnalysis dirInfo, AnalyserStartupArgs startupArgs)
        {
            Log.Info($"Folder: {dirInfo.Path}");
            Log.Info($"Date: {dirInfo.DateCategory}");

            // First figure out the creation date of the file
            DateTime dateTaken;
            DateTime correctDateCategory = DateTime.MinValue;
            if (TryGetFileCreationDate(filePath, out dateTaken))
            {
                // Is the date within our limits
                if (startupArgs.Limit == DateLimits.NoLimit ||
                    !startupArgs.IsDateExcludedByLimit(dateTaken))
                {
                    // Determine the correct date category for this date
                    if (!TryGetCorrectFileDateCategory(dirInfo.DateCategory, dateTaken, out correctDateCategory))
                    {
                        // If no correct date could be found then just use the current date (nothing will be modified)
                        correctDateCategory = dirInfo.DateCategory;
                    }
                }
                else
                {
                    // This file should not be offered for rectification as it is outside the bounds
                    // of the date limit
                    //Log.Debug("File '"+filePath+"' excluded due to date range limits");
                }
            }
            else
            {
                // If no date could be parsed, then the safest bet is to deduce that the file
                // is correctly placed and leave it alone. We can't do anything intelligent with it anyways.
                correctDateCategory = DateTime.MinValue;
            }
            return new FileAnalysis(dirInfo, filePath, dirInfo.DateCategory, correctDateCategory);
        }

        private bool TryGetFileCreationDate(string filePath, out DateTime outCreationDate)
        {
            //Log.Debug("File: " + filePath);
            outCreationDate = DateTime.MinValue;

            // 
            // Start with attempted filname parsing, this is the fastest and easiest way 
            // as we already have the metadata in the path. This is however potentially incorrect
            // as the original image date as many phone apps give photos names based on the date they
            // were sent to the recipient. For now this is not considered a problem
            if (Core_TryGetFileCreationDateFromFilename(filePath, out outCreationDate))
            {
                Log.Debug("Creation date extracted from filename");
            }

            // 
            // If filename parsing did not work then attempt EXIF data retrieval, metadata parsing
            // is not supported by apple mov and panasonic mts formats though
            else if (Core_TryGetFileCreationDateFromEXIFInfo(filePath, out outCreationDate))
            {
                Log.Debug("Creation date extracted from EXIF information");
            }
            
            //
            // As a final fallback read the MODIFIED date of the file (this field is usually the most
            // correct field as it will represent the last date that the contents of the file were modified)
            else
            {
                var fInfo = new FileInfo(filePath);
                if (   fInfo.CreationTime != fInfo.LastWriteTime 
                    && fInfo.LastWriteTime < fInfo.CreationTime)
                {
                    outCreationDate = fInfo.LastWriteTime;
                    Log.Debug("Creation date extracted from file LastWrite time");
                }
            }

            // Now if the creation date field has not been changed from the min value then
            // we have been unsuccessful at determining the file creation date
            return outCreationDate != DateTime.MinValue;
        }

        private bool Core_TryGetFileCreationDateFromFilename(string filePath, out DateTime outCreationDate)
        {
            // Assign default date
            outCreationDate = DateTime.MinValue;
            var fileName = Path.GetFileName(filePath) ?? "";

            // First check video file patterns
            if (fileName.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
            {
                // Android Camera Video 
                if (fileName.StartsWith("VID_", StringComparison.OrdinalIgnoreCase))
                {
                    // VID_20150428_112604.mp4
                    if (DateTime.TryParseExact(Path.GetFileNameWithoutExtension(fileName.Substring(4)),
                                               "yyyyMMdd_HHmmss",
                                               CultureInfo.InvariantCulture,
                                               DateTimeStyles.None,
                                               out outCreationDate))
                    {
                        //Log.Debug("Parsed date '{0}' from VIDEO filename '{1}'", outCreationDate.ToString("yyyy/MM/dd HH:mm:ss"), fileName);
                        return true;
                    }
                }

                // Unrecognized video file pattern
                Log.Warn("Unrecognized image file pattern '' ");
                return false;
            }
            

            //
            // If not video file then check known IMAGE name patterns

            // Android Camera Images
            if (fileName.StartsWith("IMG_", StringComparison.OrdinalIgnoreCase))
            {
                // IMG_20160611_114938.jpg  //android naming
                if (DateTime.TryParseExact(Path.GetFileNameWithoutExtension(fileName.Substring(4)),
                                               "yyyyMMdd_HHmmss",
                                               CultureInfo.InvariantCulture,
                                               DateTimeStyles.None,
                                               out outCreationDate))
                {
                    //Log.Debug("Parsed date '{0}' from IMG filename '{1}'", outCreationDate.ToString("yyyy/MM/dd HH:mm:ss"), fileName);
                    return true;
                }
            }
            // WhatsApp
            else if (fileName.StartsWith("IMG-", StringComparison.OrdinalIgnoreCase) &&
                     fileName.IndexOf("-WA0", StringComparison.OrdinalIgnoreCase) != -1)
            {
                // IMG-20160526-WA0000.jpg  //whatsapp naming, sequential numbering of WAxxxx    
                var split = fileName.Split('-'); // Extract the middle portion
                if (split.Length > 1 &&
                    DateTime.TryParseExact(split[1],
                                           "yyyyMMdd",
                                           CultureInfo.InvariantCulture,
                                           DateTimeStyles.None,
                                           out outCreationDate))
                {
                    //Log.Debug("Parsed date '{0}' from WA IMG filename '{1}'", outCreationDate.ToString("yyyy/MM/dd HH:mm:ss"), fileName);
                    return true;
                }
            }

            // Android Screenshots
            else if (fileName.StartsWith("Screenshot_", StringComparison.OrdinalIgnoreCase) )
            {
                // Screenshot_20160502-094608.png 
                if (DateTime.TryParseExact(Path.GetFileNameWithoutExtension(fileName.Substring(11)),
                                               "yyyyMMdd-HHmmss",
                                               CultureInfo.InvariantCulture,
                                               DateTimeStyles.None,
                                               out outCreationDate))
                {
                    //Log.Debug("Parsed date '{0}' from Screenshot IMG filename '{1}'", outCreationDate.ToString("yyyy/MM/dd HH:mm:ss"), fileName);
                    return true;
                }
            }

            // Facebook and edited image (both have 7 chars in the beginning that are stripped)
            else if (fileName.StartsWith("FB_IMG_", StringComparison.OrdinalIgnoreCase)
                     || fileName.StartsWith("edited_", StringComparison.OrdinalIgnoreCase) )
            {
                // FB_IMG_1464893689895.jpg Facebook uses epoc
                // edited_1459185577127.jpg Other apps that use epoch
                if (TryParseDateFromEpoch(Path.GetFileNameWithoutExtension(fileName.Substring(7)), out outCreationDate))
                {
                    //Log.Debug("Parsed date '{0}' from Facebook IMG filename '{1}'", outCreationDate.ToString("yyyy/MM/dd HH:mm:ss"), fileName);
                    return true;
                }
            }

            // Nothing was parsed
            return false;
        }

        private static bool TryParseDateFromEpoch(string value, out DateTime outDate)
        {
            long epochValue;
            if (long.TryParse(value, NumberStyles.None, NumberFormatInfo.InvariantInfo, out epochValue) &&
                TryParseDateFromEpoch(epochValue, out outDate))
            {
                return true;
            }

            outDate = DateTime.MinValue;
            return false;
        }

        private static bool TryParseDateFromEpoch(long value, out DateTime outDate)
        {
            try
            {
                outDate = DateTimeOffset.FromUnixTimeSeconds(value).DateTime;
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                outDate = DateTime.MinValue;
                return false;
            }
        }

        private static bool Core_TryGetFileCreationDateFromEXIFInfo(string filePath, out DateTime outCreationDate)
        {
            // Assign default
            outCreationDate = DateTime.MinValue;

            // Apple MOV and Panasonic MTS formats are not supported, use other methods for these files
            if (filePath.EndsWith(".mov", StringComparison.OrdinalIgnoreCase)
                || filePath.EndsWith(".mts", StringComparison.OrdinalIgnoreCase)
                )
                return false;

            try
            {
                var imageFile = TagLib.File.Create(filePath) as TagLib.Image.File;
                if (imageFile?.ImageTag.Exif?.DateTimeOriginal != null)
                {
                    outCreationDate = imageFile.ImageTag.Exif.DateTimeOriginal.Value;
                    return true;
                }

                // Fallback, we couldn't extract anything from EXIF
                return false;
            }
            catch (Exception e)
            {
                if (e is UnsupportedFormatException)
                    Log.Error("Error in Core_TryGetFileCreationDateFromEXIFInfo", e);
                /*else 
                    Log.Warn("Unknown error in Taglib",e);*/
                // Continue on...
                return false;
            }
        }

        private bool TryGetCorrectFileDateCategory(DateTime currentDateCategory, DateTime dateTaken, out DateTime correctDateCategory)
        {
            // If no date could be determined for the file then just exit and assume that 
            // whatever process imported it did it correctly as we know no better!
            if (dateTaken == DateTime.MinValue)
            {
                correctDateCategory = DateTime.MinValue;
                return false;
            }

            bool areDateTheSame = dateTaken.Date == currentDateCategory.Date;
            if (!areDateTheSame)
            {
                correctDateCategory = dateTaken.Date;
                return true;
            }

            correctDateCategory = DateTime.MinValue;
            return false;
        }
    }
}
