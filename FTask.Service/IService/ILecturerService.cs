using FTask.Repository.Identity;
using FTask.Service.ViewModel.RequestVM;
using FTask.Service.ViewModel.RequestVM.Lecturer;
using FTask.Service.ViewModel.ResposneVM;

namespace FTask.Service.IService
{
    public interface ILecturerService
    {
        Task<LoginLecturerManagement> LoginLecturer(LoginUserVM resource);
        Task<IEnumerable<Lecturer>> GetLecturers(int page, int quantity, string filter, int? departmentId, int? subjectId);
        Task<Lecturer?> GetLectureById(Guid id);
        Task<ServiceResponse<Lecturer>> CreateNewLecturer(LecturerVM newEntity);
        Task<bool> DeleteLecturer(Guid id);
        Task<ServiceResponse<Lecturer>> UpdateLecturer(UpdateLecturerVM updateLecturer, Guid id);
    }
}
