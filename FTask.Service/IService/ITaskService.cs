using FTask.Service.ViewModel.RequestVM.Task;
using FTask.Service.ViewModel.ResposneVM;
using Task = FTask.Repository.Entity.Task;

namespace FTask.Service.IService
{
    public interface ITaskService
    {
        Task<Task?> GetTaskById(int id);
        Task<IEnumerable<Task>> GetTasks(int page, int quantity, string filter, int? semsesterId, int? departmentId, int? subjectId, int? status, Guid? creatorId);
        Task<ServiceResponse<Task>> CreateNewTask(TaskVM newEntity);
        Task<bool> DeleteTask(int id);
        Task<ServiceResponse<Task>> UpdateTask(UpdateTaskVM updateTask, int id);
        Task<TaskStatusStatisticResposneVM> GetTaskStatusStatistics(DateTime? from, DateTime? to, int? semesterId);
        Task<IEnumerable<TaskCompleteionRateStatisticResponseVM>> GetTaskCompletionRateStatistics(DateTime? from, DateTime? to, int? status, int? taskId, int? semesterId);
        Task<IEnumerable<NumberOfCreatedTaskStatisticsResponseVM>> GetCreatedTaskCountStatistics(DateTime from, DateTime to, int? semesterId);
    }
}
