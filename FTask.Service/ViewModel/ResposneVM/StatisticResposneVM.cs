using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTask.Service.ViewModel.ResposneVM
{
    public class TaskStatusStatisticResposneVM
    {
        public TaskStatusStatistic ToDo { get; set; }
        public TaskStatusStatistic InProgress { get; set; }
        public TaskStatusStatistic End { get; set; }
        public int TotalParticipant { get; set; }
    }

    public struct TaskStatusStatistic
    {
        public int Quantity { get; set; }
        public float Percent { get; set; }
        public TaskStatusStatistic(int quantity, float percent)
        {
            Quantity = quantity;
            Percent = percent;
        }
    }

    public class TaskCompleteionRateStatisticResponseVM
    {
        public TaskStatisticResponseVM? Task { get; set; }
        public float CompletionRate { get; set; }
    }

    public class TaskStatisticResponseVM : Auditable
    {
        public int TaskId { get; set; }
        public string? TaskTitle { get; set; }
        public string? TaskContent { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TaskLevel { get; set; }
        public int TaskStatus { get; set; }
        public string? Location { get; set; }
    }

    public class NumberOfCreatedTaskStatisticsResponseVM
    {
        public DateTime DateTime { get; set; }
        public int Quantity { get; set; } = 0;
    }
}
