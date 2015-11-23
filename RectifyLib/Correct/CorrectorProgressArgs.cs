namespace RectifyLib.Correct
{
    public class CorrectorProgressArgs : BackgroundProgressArgs
    {
        public string FilePathFrom { get; }
        public string FilePathTo { get; }

        public CorrectorProgressArgs(string filePathFrom, string filePathTo, int currentStep, int totalSteps) : base(currentStep, totalSteps)
        {
            FilePathFrom = filePathFrom;
            FilePathTo = filePathTo;
        }
    }
}