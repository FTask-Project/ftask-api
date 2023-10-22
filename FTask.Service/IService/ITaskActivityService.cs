using FTask.Repository.Entity;
using FTask.Service.ViewModel.RequestVM.TaskActivity;
using FTask.Service.ViewModel.ResposneVM;

namespace FTask.Service.IService
{
    public interface ITaskActivityService
    {
        Task<TaskActivity?> GetTaskActivityById(int id);
        Task<IEnumerable<TaskActivity>> GetTaskActivities(int page, int quantity, string filter, int? taskLecturerId);
        Task<ServiceResponse<TaskActivity>> CreateNewActivity(CreateTaskActivityVM newEntity);
        Task<bool> DeleteTaskActivity(int id);
        Task<ServiceResponse<TaskActivity>> UpdateTaskActivity(UpdateTaskActivityVM updateTaskActivity, int id);
    }
}
