using FTask.Service.ViewModel.RequestVM.CreateTaskReport;
using FTask.Service.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FTask.Repository.Entity;

namespace FTask.Service.IService
{
    public interface ITaskReportService
    {
        Task<TaskReport?> GetTaskReportById(int id);
        Task<IEnumerable<TaskReport>> GetTaskReports(int page, int quantity, int? taskActivityId);
        Task<ServiceResponse> CreateNewTaskReport(TaskReportVM newEntity);
    }
}
