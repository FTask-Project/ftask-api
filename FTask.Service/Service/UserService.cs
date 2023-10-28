using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FirebaseAdmin.Auth;
using FirebaseAdmin;
using FTask.Repository.Common;
using FTask.Repository.Data;
using FTask.Repository.Identity;
using FTask.Service.Caching;
using FTask.Service.Validation;
using FTask.Service.ViewModel.RequestVM;
using FTask.Service.ViewModel.RequestVM.User;
using FTask.Service.ViewModel.ResposneVM;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Role = FTask.Repository.Identity.Role;
using Google.Apis.Auth.OAuth2;

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
    private readonly ICurrentUserService _currentUserService;

    public UserService(
        ICheckQuantityTaken checkQuantityTaken,
        UserManager<Lecturer> lecturerManager,
        ICacheService<User> cacheService,
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        IUnitOfWork unitOfWork,
        Cloudinary cloudinary,
        ICurrentUserService currentUserService
        )
    {
        _checkQuantityTaken = checkQuantityTaken;
        _lecturerManager = lecturerManager;
        _cacheService = cacheService;
        _userManager = userManager;
        _roleManager = roleManager;
        _unitOfWork = unitOfWork;
        _cloudinary = cloudinary;
        _currentUserService = currentUserService;
    }

    public async Task<LoginUserManagement> LoginWithGoogle(string idToken)
    {
        // Initialize the Firebase app
        if (FirebaseApp.GetInstance("webInstance") is null)
        {
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile("webServiceAccountKey.json"),
            }, "webInstance");
        }

        // Verify the ID token
        var webInstance = FirebaseApp.GetInstance("webInstance");

        string? email; 

        try
        {
            var decodedToken = await FirebaseAuth.GetAuth(webInstance).VerifyIdTokenAsync(idToken);
            email = decodedToken.Claims["email"].ToString();
        }
        catch (FirebaseAuthException ex)
        {
            return new LoginUserManagement
            {
                IsSuccess = false,
                Message = ex.Message
            };
        }


        // Get user data
        //var uid = decodedToken.Uid;
        //var name = decodedToken.Claims["name"].ToString();
        //pictureUrl = decodedToken.Claims["picture"].ToString();


        var includes = new Expression<Func<User, object>>[]
        {
            u => u.Roles
        };

        var existedUser = await _unitOfWork.UserRepository
            .Get(u => !u.Deleted && u.Email.Equals(email), includes)
            .FirstOrDefaultAsync();
        if (existedUser is null)
        {
            return new LoginUserManagement
            {
                IsSuccess = false,
                Message = "Email not found"
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

        return new LoginUserManagement
        {
            Message = "Login Successfully",
            IsSuccess = true,
            LoginUser = existedUser,
            RoleNames = existedUser.Roles.Select(r => r.Name)
        };
    }

    public async Task<LoginUserManagement> LoginUser(LoginUserVM resource)
    {
        var existedUser = await _userManager.FindByNameAsync(resource.UserName);
        if (existedUser == null || existedUser.Deleted)
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
            var roles = await _unitOfWork.RoleRepository.Get(r => !r.Deleted && r.Users.Contains(existedUser)).ToArrayAsync();
            existedUser.Roles = roles;
            return new LoginUserManagement
            {
                Message = "Login Successfully",
                IsSuccess = true,
                LoginUser = existedUser,
                RoleNames = roles.Select(r => r.Name)
            };
        }
    }

    public async Task<IEnumerable<User>> GetUsers(int page, int quantity, string filter)
    {
        if (page == 0)
        {
            page = 1;
        }
        quantity = _checkQuantityTaken.check(quantity);

        var userList = _unitOfWork.UserRepository
                .Get(u => !u.Deleted && (u.Email.Contains(filter) || u.PhoneNumber.Contains(filter) || u.DisplayName!.Contains(filter)))
                .Skip((page - 1) * _checkQuantityTaken.PageQuantity)
                .Take(quantity);
        return await userList.ToArrayAsync();
    }

    public async Task<User?> GetUserById(Guid id)
    {
        string key = CacheKeyGenerator.GetKeyById(nameof(User), id.ToString());
        var cacheData = await _cacheService.GetAsync(key);
        if (cacheData is null)
        {
            var include = new Expression<Func<User, object>>[]
            {
                u => u.Roles
            };
            var user = await _unitOfWork.UserRepository
                .Get(u => !u.Deleted && u.Id == id, include)
                .FirstOrDefaultAsync();
            if (user is not null)
            {
                await _cacheService.SetAsync(key, user);
            }
            return user;
        }

        return cacheData;
    }

    public async Task<ServiceResponse<User>> CreateNewUser(UserVM newEntity)
    {
        var existedUser = await _userManager.FindByNameAsync(newEntity.UserName);
        if (existedUser is not null)
        {
            return new ServiceResponse<User>
            {
                IsSuccess = false,
                Message = $"Failed to create new user",
                Errors = new string[1] { "Username is already taken" }
            };
        }

        if (newEntity.Email is not null)
        {
            var existedLecturer = await _unitOfWork.LecturerRepository.Get(l => newEntity.Email.Equals(l.Email)).FirstOrDefaultAsync();
            if (existedLecturer is not null)
            {
                return new ServiceResponse<User>
                {
                    IsSuccess = false,
                    Message = "Failed to create new user",
                    Errors = new string[1] { "Email is already taken" }
                };
            }
        }

        if (newEntity.PhoneNumber is not null)
        {
            var existedLecturer = await _unitOfWork.LecturerRepository.Get(l => newEntity.PhoneNumber.Equals(l.PhoneNumber)).FirstOrDefaultAsync();
            if (existedLecturer is not null)
            {
                return new ServiceResponse<User>
                {
                    IsSuccess = false,
                    Message = "Failed to create new user",
                    Errors = new string[1] { "Phone is already taken" }
                };
            }
        }

        User newUser = new User
        {
            DisplayName = newEntity.DisplayName,
            UserName = newEntity.UserName,
            PhoneNumber = newEntity.PhoneNumber,
            Email = newEntity.Email,
            LockoutEnabled = newEntity.LockoutEnabled ?? true,
            LockoutEnd = newEntity.LockoutEnd,
            EmailConfirmed = false,
            PhoneNumberConfirmed = false,
            TwoFactorEnabled = false,
            CreatedAt = DateTime.Now,
            CreatedBy = _currentUserService.UserId
        };

        if (newEntity.RoleIds.Count() > 0)
        {
            List<Role> roles = new List<Role>();
            foreach (Guid id in newEntity.RoleIds)
            {
                var existedRole = await _unitOfWork.RoleRepository.FindAsync(id);
                if (existedRole is null)
                {
                    return new ServiceResponse<User>
                    {
                        IsSuccess = false,
                        Message = "Failed to create new user",
                        Errors = new List<string>() { "Role not found" }
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
                return new ServiceResponse<User>
                {
                    IsSuccess = false,
                    Message = "Failed to create new user",
                    Errors = new string[1] { "Error when upload image" }
                };
            }
            newUser.FilePath = uploadResult.SecureUrl.ToString();
        }
        else if(newEntity.FilePath is not null)
        {
            newUser.FilePath = newEntity.FilePath;
        }

        try
        {
            var identityResult = await _userManager.CreateAsync(newUser, newEntity.Password);
            if (!identityResult.Succeeded)
            {
                return new ServiceResponse<User>
                {
                    IsSuccess = false,
                    Message = "Failed to create new user",
                    Errors = identityResult.Errors.Select(e => e.Description)
                };
            }
            else
            {
                return new ServiceResponse<User>
                {
                    Entity = newUser,
                    IsSuccess = true,
                    Message = "Create new lecturer successfully"
                };
            }
        }
        catch (DbUpdateException ex)
        {
            return new ServiceResponse<User>
            {
                IsSuccess = false,
                Message = "Failed to create new user",
                Errors = new List<string>() { ex.Message }
            };
        }
        catch (OperationCanceledException)
        {
            return new ServiceResponse<User>
            {
                IsSuccess = false,
                Message = "Failed to create new user",
                Errors = new string[1] { "The operation has been cancelled" }
            };
        }
    }

    public async Task<bool> DeleteUser(Guid id)
    {
        var existedUser = await _unitOfWork.UserRepository.Get(u => !u.Deleted && u.Id == id).FirstOrDefaultAsync();
        if (existedUser is null)
        {
            return false;
        }
        existedUser.Deleted = true;
        var result = await _unitOfWork.SaveChangesAsync();

        if (result)
        {
            string key = CacheKeyGenerator.GetKeyById(nameof(User), id.ToString());
            await _cacheService.RemoveAsync(key);
        }

        return result;
    }

    public async Task<ServiceResponse<User>> UpdateUser(UpdateUserVM updateUser, Guid id)
    {
        Expression<Func<User, object>>[] includes = new Expression<Func<User, object>>[]
                {
                    u => u.Roles
                };

        var existedUser = await _unitOfWork.UserRepository
            .Get(u => !u.Deleted && u.Id == id, includes)
            .FirstOrDefaultAsync();

        if (existedUser is null)
        {
            return new ServiceResponse<User>
            {
                IsSuccess = false,
                Message = "Failed to update user",
                Errors = new string[] { "User not found" }
            };
        }

        if (updateUser.Email is not null)
        {
            var checkEmail = _unitOfWork.UserRepository.Get(u => u.Email.Equals(updateUser.Email)).FirstOrDefault() is not null;
            if (checkEmail)
            {
                return new ServiceResponse<User>
                {
                    IsSuccess = false,
                    Message = "Failed to update user",
                    Errors = new string[] { "Email is already taken" }
                };
            }
            existedUser.Email = updateUser.Email;
        }

        if (updateUser.PhoneNumber is not null)
        {
            var checkPhoneNumber = _unitOfWork.UserRepository.Get(u => u.PhoneNumber.Equals(updateUser.PhoneNumber)).FirstOrDefault() is not null;
            if (checkPhoneNumber)
            {
                return new ServiceResponse<User>
                {
                    IsSuccess = false,
                    Message = "Failed to update user",
                    Errors = new string[] { "Phone number is already taken" }
                };
            }
            existedUser.PhoneNumber = updateUser.PhoneNumber;
        }

        if (updateUser.Avatar is not null && updateUser.Avatar.Length > 0)
        {
            var uploadFile = new ImageUploadParams
            {
                File = new FileDescription(updateUser.Avatar.FileName, updateUser.Avatar.OpenReadStream())
            };
            var uploadResult = await _cloudinary.UploadAsync(uploadFile);

            if (uploadResult.Error is not null)
            {
                return new ServiceResponse<User>
                {
                    IsSuccess = false,
                    Message = "Failed to update user",
                    Errors = new string[1] { "Failed to upload image" }
                };
            }
            existedUser.FilePath = uploadResult.SecureUrl.ToString();
        }
        else if(updateUser.FilePath is not null)
        {
            existedUser.FilePath = updateUser.FilePath;
        }

        if (updateUser.RoleIds is not null)
        {
            var roleList = await _unitOfWork.RoleRepository.Get(r => !r.Deleted && updateUser.RoleIds.Contains(r.Id)).ToListAsync();
            existedUser.Roles = roleList;
        }

        existedUser.DisplayName = updateUser.DisplayName ?? existedUser.DisplayName;
        existedUser.LockoutEnabled = updateUser.LockoutEnabled ?? existedUser.LockoutEnabled;
        existedUser.LockoutEnd = updateUser.LockoutEnd ?? existedUser.LockoutEnd;

        try
        {
            var result = await _unitOfWork.SaveChangesAsync();
            if (result)
            {
                string key = CacheKeyGenerator.GetKeyById(nameof(User), existedUser.Id.ToString());
                var task = _cacheService.RemoveAsync(key);

                return new ServiceResponse<User>
                {
                    IsSuccess = true,
                    Message = "Update user successfully",
                    Entity = existedUser
                };
            }
            else
            {
                return new ServiceResponse<User>
                {
                    IsSuccess = false,
                    Message = "Failed to update user",
                    Errors = new string[1] { "Can not save changes" }
                };
            }
        }
        catch (DbUpdateException ex)
        {
            return new ServiceResponse<User>
            {
                IsSuccess = false,
                Message = "Failed to update user",
                Errors = new List<string>() { ex.Message }
            };
        }
        catch (OperationCanceledException)
        {
            return new ServiceResponse<User>
            {
                IsSuccess = false,
                Message = "Failed to update user",
                Errors = new string[1] { "The operation has been cancelled" }
            };
        }
    }
}
