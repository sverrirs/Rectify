using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RectifyLib;
using RectifyLib.Analysis;

namespace RectifyUI.DataGridView
{
    public class FileAnalysisBindable
    {
        private readonly FileAnalysis _baseFile;

        public FileAnalysis Readonly => _baseFile;

        public string FileName { get; }

        public string FileType { get; }

        public bool IsSelected { get; set; }
        
        public DateTime DateCategory { get; set; }
        

        public FileAnalysisBindable(FileAnalysis baseFile)
        {
            _baseFile = baseFile;
            
            FileName = Path.GetFileName(_baseFile.FilePath);
            FileType = Path.GetExtension(_baseFile.FilePath)?.Trim('.');
            DateCategory = _baseFile.DateCategoryDetected;
        }
    }
}
