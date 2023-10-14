using FTask.Repository.Identity;
using FTask.Service.ViewModel;
using FTask.Service.ViewModel.RequestVM;
using FTask.Service.ViewModel.RequestVM.CreateLecturer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTask.Service.IService
{
    public interface ILecturerService
    {
        Task<LoginLecturerManagement> LoginLecturer(LoginUserVM resource);
        Task<IEnumerable<Lecturer>> GetLecturers(int page, int quantity, string filter, int? departmentId, int? subjectId);
        Task<Lecturer?> GetLectureById(Guid id);
        Task<ServiceResponse> CreateNewLecturer(LecturerVM newEntity);
    }
}
