using FTask.Repository.Identity;
using FTask.Service.ViewModel.RequestVM;
using FTask.Service.ViewModel.RequestVM.User;
using FTask.Service.ViewModel.ResposneVM;

namespace FTask.Service.IService;

public interface IUserService
{
    Task<LoginUserManagement> LoginMember(LoginUserVM resource);
    Task<IEnumerable<User>> GetUsers(int page, int quantity, string filter);
    Task<User?> GetUserById(Guid id);
    Task<ServiceResponse<User>> CreateNewUser(UserVM newEntity);
    Task<bool> DeleteUser(Guid id);
    Task<ServiceResponse<User>> UpdateUser(UpdateUserVM updateUser, Guid id);
}
