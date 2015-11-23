using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RectifyLib.Analysis;

namespace RectifyLib.Correct
{
    public class Corrector : AsyncBackgroundProcessor<CorrectionResults, CorrectorProgressArgs, FileAnalysis[]>
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger(typeof(Corrector));
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

            return results;
        }
    }
}
