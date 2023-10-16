using FTask.Repository.Entity;
using FTask.Service.ViewModel.RequestVM.CreateSemester;
using FTask.Service.ViewModel.ResposneVM;

namespace FTask.Service.IService
{
    public interface ISemesterService
    {
        Task<Semester?> GetSemesterById(int id);
        Task<IEnumerable<Semester>> GetSemesters(int page, int quantity, string filter);
        Task<ServiceResponse> CreateNewSemester(SemesterVM newEntity);
        Task<bool> DeleteSemester(int id);
    }
}
