using FTask.Service.ViewModel;
using FTask.Repository.Data;
using FTask.Repository.Identity;
using Microsoft.AspNetCore.Identity;
using FTask.Service.Validation;
using Microsoft.EntityFrameworkCore;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Role = FTask.Repository.Identity.Role;
using FTask.Service.Caching;
using FTask.Service.ViewModel.RequestVM.CreateUser;
using FTask.Service.ViewModel.RequestVM;
using Microsoft.IdentityModel.Tokens;

namespace FTask.Service.IService;

internal class UserService : IUserService
{
    private readonly ICheckQuantityTaken _checkQuantityTaken;
    private readonly UserManager<Lecturer> _lecturerManager;
    private readonly ICacheService<User> _cacheService;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly Cloudinary _cloudinary;

    public UserService(
        ICheckQuantityTaken checkQuantityTaken,
        UserManager<Lecturer> lecturerManager, 
        ICacheService<User> cacheService,
        UserManager<User> userManager, 
        RoleManager<Role> roleManager, 
        IUnitOfWork unitOfWork, 
        Cloudinary cloudinary
        )
    {
        _checkQuantityTaken = checkQuantityTaken;
        _lecturerManager = lecturerManager;
        _cacheService = cacheService;
        _userManager = userManager;
        _roleManager = roleManager;
        _unitOfWork = unitOfWork;
        _cloudinary = cloudinary;
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

    public async Task<IEnumerable<User>> GetUsers(int page, int quantity)
    {
        if (page == 0)
        {
            page = 1;
        }
        quantity = _checkQuantityTaken.check(quantity);

        string key = CacheKeyGenerator.GetKeyByPageAndQuantity(nameof(User), page, quantity);
        var cacheData = await _cacheService.GetAsyncArray(key);
        if (cacheData.IsNullOrEmpty())
        {
            var userList = await _unitOfWork.UserRepository
                .FindAll()
                .Skip((page - 1) * _checkQuantityTaken.PageQuantity)
                .Take(quantity)
                .ToArrayAsync();

            if (userList.Count() > 0)
            {
                await _cacheService.SetAsyncArray(key, userList);
            }
            return userList;
        }

        return cacheData;
    }

    public async Task<User?> GetUserById(Guid id)
    {
        string key = CacheKeyGenerator.GetKeyById(nameof(User), id.ToString());
        var cacheData = await _cacheService.GetAsync(key);
        if (cacheData is null)
        {
            var user = await _unitOfWork.UserRepository.FindAsync(id);
            if (user is not null)
            {
                await _cacheService.SetAsync(key, user);
            }
            return user;
        }

        return cacheData;
    }

    public async Task<ServiceResponse> CreateNewUser(UserVM newEntity)
    {
        try
        {
            var existedUser = await _userManager.FindByNameAsync(newEntity.UserName);
            if (existedUser is not null)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = $"Username is already taken"
                };
            }

            if (newEntity.Email is not null)
            {
                var existedLecturer = await _unitOfWork.LecturerRepository.Get(l => newEntity.Email.Equals(l.Email)).FirstOrDefaultAsync();
                if (existedLecturer is not null)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Email is already taken"
                    };
                }
            }

            if (newEntity.PhoneNumber is not null)
            {
                var existedLecturer = await _unitOfWork.LecturerRepository.Get(l => newEntity.PhoneNumber.Equals(l.PhoneNumber)).FirstOrDefaultAsync();
                if (existedLecturer is not null)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Phone is already taken"
                    };
                }
            }

            User newUser = new User
            {
                UserName = newEntity.UserName,
                PhoneNumber = newEntity.PhoneNumber,
                Email = newEntity.Email,
                LockoutEnabled = newEntity.LockoutEnabled ?? true,
                LockoutEnd = newEntity.LockoutEnd,
                EmailConfirmed = false,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false
            };

            if (newEntity.RoleIds.Count() > 0)
            {
                List<Role> roles = new List<Role>();
                foreach (Guid id in newEntity.RoleIds)
                {
                    var existedRole = await _unitOfWork.RoleRepository.FindAsync(id);
                    if (existedRole is null)
                    {
                        return new ServiceResponse
                        {
                            IsSuccess = false,
                            Message = "Create new user failed",
                            Errors = new List<string>() { "Role not found with the given id :" + id }
                        };
                    }
                    else
                    {
                        roles.Add(existedRole);
                    }
                }
                if (roles.Count() > 0)
                {
                    newUser.Roles = roles;
                }
            }

            //Upload file
            var file = newEntity.Avatar;
            if (file is not null && file.Length > 0)
            {
                var uploadFile = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, file.OpenReadStream())
                };
                var uploadResult = await _cloudinary.UploadAsync(uploadFile);

                if (uploadResult.Error is not null)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Create new lecturer failed",
                        Errors = new string[1] { "Error when upload image" }
                    };
                }
                newUser.FilePath = uploadResult.SecureUrl.ToString();
            };

            var identityResult = await _userManager.CreateAsync(newUser, newEntity.Password);
            if (!identityResult.Succeeded)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Create new lecturer failed",
                    Errors = identityResult.Errors.Select(e => e.Description)
                };
            }
            else
            {
                return new ServiceResponse
                {
                    Id = newUser.Id.ToString(),
                    IsSuccess = true,
                    Message = "Create new lecturer successfully"
                };
            }
        }
        catch (DbUpdateException ex)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Some error happened",
                Errors = new List<string>() { ex.Message }
            };
        }
    }
}
