using FTask.Service.ViewModel.RequestVM.CreateTask;
using FTask.Service.ViewModel.ResposneVM;
using Task = FTask.Repository.Entity.Task;

namespace FTask.Service.IService
{
    public interface ITaskService
    {
        Task<Task?> GetTaskById(int id);
        Task<IEnumerable<Task>> GetTasks(int page, int quantity, string filter, int? semsesterId, int? departmentId, int? subjectId, int? status);
        Task<ServiceResponse> CreateNewTask(TaskVM newEntity);
    }
}
