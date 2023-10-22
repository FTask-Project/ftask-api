using FTask.Repository.Entity;
using FTask.Service.ViewModel.RequestVM.Subject;
using FTask.Service.ViewModel.ResposneVM;

namespace FTask.Service.IService
{
    public interface ISubjectService
    {
        Task<Subject?> GetSubjectById(int id);
        Task<IEnumerable<Subject>> GetSubjectAllSubject(int page, int quantity, string filter, int? departmentId, bool? status);
        Task<ServiceResponse<Subject>> CreateNewSubject(Subject subjectEntity);
        Task<bool> DeleteSubject(int id);
        Task<ServiceResponse<Subject>> UpdateSubject(UpdateSubjectVM updateSubject, int id);
    }
}