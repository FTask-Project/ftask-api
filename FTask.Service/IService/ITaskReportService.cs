using FTask.Repository.Entity;
using FTask.Service.ViewModel.RequestVM.TaskReport;
using FTask.Service.ViewModel.ResposneVM;

namespace FTask.Service.IService
{
    public interface ITaskReportService
    {
        Task<TaskReport?> GetTaskReportById(int id);
        Task<IEnumerable<TaskReport>> GetTaskReports(int page, int quantity, int? taskActivityId);
        Task<ServiceResponse<TaskReport>> CreateNewTaskReport(TaskReportVM newEntity);
        Task<bool> DeleteTaskReport(int id);
        Task<ServiceResponse<TaskReport>> UpdateTaskReport(UpdateTaskReportVM updateTaskReport, int id);
    }
}
