using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using RectifyLib.Analysis;

namespace RectifyLib.Correct
{
    public class Corrector : AsyncBackgroundProcessor<CorrectionResults, CorrectorProgressArgs, FileAnalysis[]>
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger(typeof(Corrector));
        private static string ReplayFolderName = "replays";

        protected override Func<CorrectionResults> CreateAsyncProcess(FileAnalysis[] selectedFiles, CancellationToken cancellationToken)
        {
            return () => CorrectFileLocations(selectedFiles);
        }

        private CorrectionResults CorrectFileLocations(FileAnalysis[] selectedFiles)
        {
            var results = new CorrectionResults();

            foreach (var file in selectedFiles)
            {
                if( string.IsNullOrWhiteSpace(file.CorrectedFilePath))
                    continue;

                try
                {
                    var dir = Path.GetDirectoryName(file.CorrectedFilePath);
                    if (dir != null && !Directory.Exists(dir))
                    {
                        // Need to create the directory, but have to be careful to copy the ACL from the parent directory
                        // otherwise we will have all kinds of access issues
                        DirectoryInfo dirInfo = new DirectoryInfo(Path.GetDirectoryName(dir));
                        var parentDirAcl = dirInfo.GetAccessControl();

                        Log.Debug($"Creating destination directory {dir}");
                        Directory.CreateDirectory(dir, parentDirAcl);
                    }

                    Log.Info($"   Moving {file} to {file.CorrectedFilePath}");
                    Directory.Move(file.FilePath, file.CorrectedFilePath);

                    results.AddCorrection(new FileCorrectionResult(file.FilePath, file.CorrectedFilePath, success: true));
                }
                catch (Exception e)
                {
                    results.AddCorrection(new FileCorrectionResult(file.FilePath, file.FilePath, success:false, error: e.Message));
                }
            }

            // After everything has been processed generate the trace file with only the successful correction results
            // this will allow us to roll back the changes after the fact
            CreateRollbackFile(results);

            return results;
        }

        private void CreateRollbackFile(CorrectionResults results)
        {
            // Ensure that the directory for the replays exists
            Directory.CreateDirectory(ReplayFolderName);

            var fileName = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss")+".txt";
            var filePath = Path.Combine(ReplayFolderName, fileName);

            using (StreamWriter sw = new StreamWriter(filePath))
            {
                using (var csv = new CsvWriter(sw))
                {
                    csv.Configuration.AllowComments = true;
                    
                    // Write header first
                    csv.WriteField("from");
                    csv.WriteField("to");
                    csv.NextRecord();

                    foreach (var item in results.Corrections.Where(x=>x.Success) )
                    {
                        csv.WriteField(item.OldFilePath);
                        csv.WriteField(item.NewFilePath);
                        csv.NextRecord();
                    }
                }
            }

            Log.InfoFormat("Replay file saved to {0}", filePath);
        }
    }
}
