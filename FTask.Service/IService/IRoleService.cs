using FTask.Repository.Identity;
using FTask.Service.ViewModel.RequestVM.CreateRole;
using FTask.Service.ViewModel.ResposneVM;

namespace FTask.Service.IService
{
    public interface IRoleService
    {
        Task<IEnumerable<Role>> GetRolesByName(IEnumerable<string> roleNames);
        Task<Role?> GetRoleById(Guid id);
        Task<IEnumerable<Role>> GetRoles(int page, int quantity, string filter);
        Task<ServiceResponse> CreateNewRole(RoleVM newEntity);
    }
}
