using FTask.Repository.Entity;
using FTask.Service.ViewModel.RequestVM.TaskLecturer;
using FTask.Service.ViewModel.ResposneVM;

namespace FTask.Service.IService
{
    public interface ITaskLecturerService
    {
        Task<TaskLecturer?> GetTaskLecturerById(int id);
        Task<ServiceResponse<TaskLecturer>> CreateNewTaskLecturer(CreateTaskLecturerVM newEntity);
        Task<IEnumerable<TaskLecturer>> GetTaskLecturers(int page, int quantity, int? taskId, Guid? lecturerId);
        Task<bool> DeleteTaskLecturer(int id);
    }
}
