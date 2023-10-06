using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTask.Service.Validation
{
    public interface ICheckSemesterPeriod
    {
        public int MinimumDuration { get; }
        public int MaximumDuration { get; }
        bool CheckValidDuration(DateTime from, DateTime to);
    }

    internal class CheckSemesterPeriod : ICheckSemesterPeriod
    {
        private static TimeSpan MINIMUM_DURATION = TimeSpan.FromDays(30);
        private static TimeSpan MAXIMUM_DURATION = TimeSpan.FromDays(90);

        public bool CheckValidDuration(DateTime from, DateTime to)
        {
            var duration = (to - from).TotalDays;
            if(duration >= MINIMUM_DURATION.Days && duration <= MAXIMUM_DURATION.Days)
            {
                return true;
            }
            return false;
        }

        public int MinimumDuration { get => MINIMUM_DURATION.Days; }

        public int MaximumDuration { get => MAXIMUM_DURATION.Days; }
    }
}
