using FTask.Repository.Entity;
using FTask.Service.ViewModel;
using FTask.Service.ViewModel.RequestVM.CreateTaskReport;

namespace FTask.Service.IService
{
    public interface ITaskReportService
    {
        Task<TaskReport?> GetTaskReportById(int id);
        Task<IEnumerable<TaskReport>> GetTaskReports(int page, int quantity, int? taskActivityId);
        Task<ServiceResponse> CreateNewTaskReport(TaskReportVM newEntity);
    }
}
