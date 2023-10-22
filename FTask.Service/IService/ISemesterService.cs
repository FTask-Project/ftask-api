using FTask.Repository.Entity;
using FTask.Service.ViewModel.RequestVM.Semester;
using FTask.Service.ViewModel.ResposneVM;

namespace FTask.Service.IService
{
    public interface ISemesterService
    {
        Task<Semester?> GetSemesterById(int id);
        Task<IEnumerable<Semester>> GetSemesters(int page, int quantity, string filter);
        Task<ServiceResponse<Semester>> CreateNewSemester(SemesterVM newEntity);
        Task<bool> DeleteSemester(int id);
        Task<ServiceResponse<Semester>> UpdateSemester(UpdateSemesterVM updateSemester, int semesterId);
    }
}
