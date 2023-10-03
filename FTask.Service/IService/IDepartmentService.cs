using FTask.Repository.Entity;
using FTask.Service.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTask.Service.IService
{
    public interface IDepartmentService
    {
        Task<Department?> GetDepartmentById(int id);
        Task<IEnumerable<Department>?> GetDepartments(int page, int quantity);
        Task<ServiceResponse> CreateNewDepartment(Department newEntity);
    }
}
