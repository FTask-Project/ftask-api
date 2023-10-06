using FTask.Repository.Identity;
using FTask.Service.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTask.Service.IService
{
    public interface IRoleService
    {
        Task<IEnumerable<Role>> GetRolesByName(IEnumerable<string> roleNames);
        Task<Role?> GetRoleById(Guid id);
        Task<IEnumerable<Role>> GetRoles(int page, int quantity);
        Task<ServiceResponse> CreateNewRole(RoleVM newEntity);
    }
}
