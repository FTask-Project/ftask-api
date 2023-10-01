using FTask.Service.ViewModel;
using FTask.Repository.Data;
using FTask.Repository.Identity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTask.Service.IService;

internal class UserService : IUserService
{
    private readonly UserManager<Lecturer> _lecturerManager;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IUnitOfWork _unitOfWork;

    public UserService(UserManager<User> userManager, RoleManager<Role> roleManager, UserManager<Lecturer> lecturerManager, IUnitOfWork unitOfWork)
    {
        _lecturerManager = lecturerManager;
        _userManager = userManager;
        _roleManager = roleManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<LoginUserManagement> LoginMember(LoginUserVM resource)
    {
        var existedUser = await _userManager.FindByNameAsync(resource.UserName);
        if (existedUser == null)
        {
            return new LoginUserManagement
            {
                Message = "Invalid Username or password",
                IsSuccess = false,
            };
        }

        var checkPassword = await _userManager.CheckPasswordAsync(existedUser, resource.Password);
        if (!checkPassword)
        {
            return new LoginUserManagement
            {
                Message = "Invalid Username or password",
                IsSuccess = false,
            };
        }

        if (existedUser.LockoutEnabled)
        {
            return new LoginUserManagement
            {
                Message = "Account is locked",
                IsSuccess = false,
            };
        }
        else
        {
            var roles = await _userManager.GetRolesAsync(existedUser);
            return new LoginUserManagement
            {
                Message = "Login Successfully",
                IsSuccess = true,
                LoginUser = existedUser,
                RoleNames = roles
            };
        }
    }

    public async Task<LoginLecturerManagement> LoginLecturer(LoginUserVM resource)
    {
        var existedUser = await _lecturerManager.FindByNameAsync(resource.UserName);
        if (existedUser == null)
        {
            return new LoginLecturerManagement
            {
                Message = "Invalid Username or password",
                IsSuccess = false,
            };
        }

        var checkPassword = await _lecturerManager.CheckPasswordAsync(existedUser, resource.Password);
        if (!checkPassword)
        {
            return new LoginLecturerManagement
            {
                Message = "Invalid Username or password",
                IsSuccess = false,
            };
        }

        if (existedUser.LockoutEnabled)
        {
            return new LoginLecturerManagement
            {
                Message = "Account is locked",
                IsSuccess = false,
            };
        }
        else
        {
            return new LoginLecturerManagement
            {
                Message = "Login Successfully",
                IsSuccess = true,
                LoginUser = existedUser
            };
        }
    }

    /*public async Task Start()
    {

        Role r1 = new Role();
        r1.Name = "Manager";

        Role r2 = new Role();
        r2.Name = "Admin";

        await _roleManager.CreateAsync(r1);
        await _roleManager.CreateAsync(r2);

        User user = new User();
        user.UserName = "User1";
        await _userManager.CreateAsync(user, "string1");
        await _userManager.AddToRolesAsync(user, new string[2] { "Manager", "Admin" });

        Lecturer lecturer = new Lecturer();
        lecturer.UserName = "Lecturer1";
        await _lecturerManager.CreateAsync(lecturer, "string1");
    }*/
}
