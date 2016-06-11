using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RectifyLib.Analysis
{
    public sealed class AnalyserStartupArgs
    {
        private DateTime _upperLimit; 
        private DateTime _lowerLimit;
                
        /// <summary>
        /// The directory to analyse for incorrect file placements. All subdirectories of this
        /// directory will also be analysed.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Is there a limit on the search 
        /// </summary>
        public DateLimits Limit { get; private set; }

        /// <summary>
        /// If the limit is set then this value is the exact date limit that 
        /// is imposed
        /// </summary>
        public DateTime DateLimit { get; private set; }
        

        public AnalyserStartupArgs(string path, DateLimits limit, DateTime dateLimit)
        {
            Path = path;
            DateLimit = dateLimit;
            Limit = limit;

            // Create the upper and lower limits
            switch (limit)
            {
                case DateLimits.ExactDate:
                    _lowerLimit = new DateTime(DateLimit.Year, DateLimit.Month, DateLimit.Day, 0,0,0,1);
                    _upperLimit = _lowerLimit.AddDays(1).AddSeconds(-1);
                    break;
                case DateLimits.MonthAndYear:
                    _lowerLimit = new DateTime(DateLimit.Year, DateLimit.Month, 1, 0, 0, 0, 1);
                    _upperLimit = _lowerLimit.AddMonths(1).AddSeconds(-1);
                    break;
                case DateLimits.Year:
                    _lowerLimit = new DateTime(DateLimit.Year, 1, 1, 0, 0, 0, 1);
                    _upperLimit = _lowerLimit.AddYears(1).AddSeconds(-1);
                    break;
            }
        }

        public AnalyserStartupArgs(string path) : this(path, DateLimits.NoLimit, DateTime.MinValue)
        {
        }

        public bool IsDateExcludedByLimit(DateTime dateTaken)
        {
            switch (Limit)
            {
                case DateLimits.NoLimit:
                    return false;
                case DateLimits.ExactDate:
                case DateLimits.MonthAndYear:
                case DateLimits.Year:
                    return dateTaken < _lowerLimit || dateTaken > _upperLimit;
                default:
                    throw new ArgumentOutOfRangeException("Unsupported date range for limit filter");
            }
        }
    }
}
