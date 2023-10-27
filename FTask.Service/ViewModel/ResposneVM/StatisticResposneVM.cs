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
}
