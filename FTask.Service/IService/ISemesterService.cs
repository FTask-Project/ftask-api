using FTask.Repository.Entity;
using FTask.Service.ViewModel;
using FTask.Service.ViewModel.RequestVM.CreateSemester;

namespace FTask.Service.IService
{
    public interface ISemesterService
    {
        Task<Semester?> GetSemesterById(int id);
        Task<IEnumerable<Semester>> GetSemesters(int page, int quantity, string filter);
        Task<ServiceResponse> CreateNewSemester(SemesterVM newEntity);
    }
}
