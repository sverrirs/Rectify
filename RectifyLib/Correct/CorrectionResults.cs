using System.Collections.Generic;

namespace RectifyLib.Correct
{
    public class CorrectionResults
    {
        private readonly List<FileCorrectionResult> _corrections = new List<FileCorrectionResult>();

        public FileCorrectionResult[] Corrections => _corrections.ToArray();

        public void AddCorrection(FileCorrectionResult file)
        {
            _corrections.Add(file);
        }
    }
}