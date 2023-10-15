using FTask.Repository.Entity;
using FTask.Service.ViewModel.RequestVM.CreateTaskActivity;
using FTask.Service.ViewModel.ResposneVM;

namespace FTask.Service.IService
{
    public interface ITaskActivityService
    {
        Task<TaskActivity?> GetTaskActivityById(int id);
        Task<IEnumerable<TaskActivity>> GetTaskActivities(int page, int quantity, string filter, int? taskLecturerId);
        Task<ServiceResponse> CreateNewActivity(CreateTaskActivityVM newEntity);
    }
}
