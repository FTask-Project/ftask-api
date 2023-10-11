using FTask.Repository.Identity;
using FTask.Service.ViewModel;
using FTask.Service.ViewModel.RequestVM;
using FTask.Service.ViewModel.RequestVM.CreateUser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTask.Service.IService;

public interface IUserService
{
    Task<LoginUserManagement> LoginMember(LoginUserVM resource);
    Task<IEnumerable<User>> GetUsers(int page, int quantity);
    Task<User?> GetUserById(Guid id);
    Task<ServiceResponse> CreateNewUser(UserVM newEntity);
}
