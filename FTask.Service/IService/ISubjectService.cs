using FTask.Repository.Entity;
using FTask.Service.ViewModel.ResposneVM;

namespace FTask.Service.IService
{
    public interface ISubjectService
    {
        Task<Subject?> GetSubjectById(int id);
        Task<IEnumerable<Subject>> GetSubjectAllSubject(int page, int quantity, string filter, int? departmentId);
        Task<IEnumerable<Subject>> GetSubjectFromDepartment(int departmentId);
        Task<ServiceResponse> CreateNewSubject(Subject subjectEntity);
        Task<ServiceResponse> UpdateSubject(Subject subjectEntity);
        Task<ServiceResponse> DeleteSubject(Subject subjectEntity);
    }
}