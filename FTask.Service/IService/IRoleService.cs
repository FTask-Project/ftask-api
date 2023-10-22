using FTask.Repository.Identity;
using FTask.Service.ViewModel.RequestVM.Role;
using FTask.Service.ViewModel.ResposneVM;

namespace FTask.Service.IService
{
    public interface IRoleService
    {
        Task<IEnumerable<Role>> GetRolesByName(IEnumerable<string> roleNames);
        Task<Role?> GetRoleById(Guid id);
        Task<IEnumerable<Role>> GetRoles(int page, int quantity, string filter);
        Task<ServiceResponse<Role>> CreateNewRole(RoleVM newEntity);
        Task<bool> DeleteRole(Guid id);
        Task<ServiceResponse<Role>> UpdateRole(UpdateRoleVM updateRole, Guid id);
    }
}
