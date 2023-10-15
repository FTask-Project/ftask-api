using FTask.Repository.Entity;
using FTask.Service.ViewModel.RequestVM.CreateTaskLecturer;
using FTask.Service.ViewModel.ResposneVM;

namespace FTask.Service.IService
{
    public interface ITaskLecturerService
    {
        Task<TaskLecturer?> GetTaskLecturerById(int id);
        Task<ServiceResponse> CreateNewTaskLecturer(CreateTaskLecturerVM newEntity);
        Task<IEnumerable<TaskLecturer>> GetTaskLecturers(int page, int quantity, int? taskId, Guid? lecturerId);
    }
}
