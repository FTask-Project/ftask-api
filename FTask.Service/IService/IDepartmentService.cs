using FTask.Repository.Entity;
using FTask.Service.ViewModel.RequestVM.Department;
using FTask.Service.ViewModel.ResposneVM;

namespace FTask.Service.IService
{
    public interface IDepartmentService
    {
        Task<Department?> GetDepartmentById(int id);
        Task<IEnumerable<Department>> GetDepartments(int page, int quantity, string filter, Guid? headerId);
        Task<ServiceResponse<Department>> CreateNewDepartment(Department newEntity);
        Task<bool> DeleteDepartment(int id);
        Task<ServiceResponse<Department>> UpdateDepartment(UpdateDepartmentVM updateDepartment, int id);
    }
}
