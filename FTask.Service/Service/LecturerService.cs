using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FTask.Repository.Common;
using FTask.Repository.Data;
using FTask.Repository.Entity;
using FTask.Repository.Identity;
using FTask.Service.Caching;
using FTask.Service.Validation;
using FTask.Service.ViewModel.RequestVM;
using FTask.Service.ViewModel.RequestVM.CreateLecturer;
using FTask.Service.ViewModel.ResposneVM;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FTask.Service.IService
{
    internal class LecturerService : ILecturerService
    {
        private readonly ICheckQuantityTaken _checkQuantityTaken;
        private readonly ICacheService<Lecturer> _cacheService;
        private readonly UserManager<Lecturer> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly Cloudinary _cloudinary;
        private readonly ICurrentUserService _currentUserService;

        public LecturerService(
            ICheckQuantityTaken checkQuantityTaken,
            ICacheService<Lecturer> cacheService,
            UserManager<Lecturer> userManager,
            IUnitOfWork unitOfWork,
            Cloudinary cloudinary,
            ICurrentUserService currentUserService
            )
        {
            _checkQuantityTaken = checkQuantityTaken;
            _cacheService = cacheService;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _cloudinary = cloudinary;
            _currentUserService = currentUserService;
        }

        public async Task<LoginLecturerManagement> LoginLecturer(LoginUserVM resource)
        {
            var existedUser = await _userManager.FindByNameAsync(resource.UserName);
            if (existedUser == null || existedUser.Deleted)
            {
                return new LoginLecturerManagement
                {
                    Message = "Invalid Username or password",
                    IsSuccess = false,
                };
            }

            var checkPassword = await _userManager.CheckPasswordAsync(existedUser, resource.Password);
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

        public async Task<IEnumerable<Lecturer>> GetLecturers(int page, int quantity, string filter, int? departmentId, int? subjectId)
        {
            if (page == 0)
            {
                page = 1;
            }
            quantity = _checkQuantityTaken.check(quantity);

            var lecturerList = _unitOfWork.LecturerRepository
                    .Get(l => !l.Deleted && (l.Email.Contains(filter) || l.PhoneNumber.Contains(filter) || l.DisplayName!.Contains(filter)),
                        new System.Linq.Expressions.Expression<Func<Lecturer, object>>[1]
                        {
                            l => l.Subjects
                        })
                    .Skip((page - 1) * _checkQuantityTaken.PageQuantity)
                    .Take(quantity);

            if (departmentId is not null)
            {
                lecturerList = lecturerList.Where(l => l.DepartmentId == departmentId);
            }

            if (subjectId is not null)
            {
                lecturerList = lecturerList.Where(l => l.Subjects.Any(s => s.SubjectId == subjectId));
            }

            return await lecturerList.ToArrayAsync();
        }

        public async Task<Lecturer?> GetLectureById(Guid id)
        {
            string key = CacheKeyGenerator.GetKeyById(nameof(Lecturer), id.ToString());
            var cachedData = await _cacheService.GetAsync(key);

            if (cachedData is null)
            {
                Expression<Func<Lecturer, object>>[] includes = new Expression<Func<Lecturer, object>>[3]
                {
                    l => l.DepartmentHead!,
                    l => l.Department!,
                    l => l.Subjects
                };
                var lecturer = await _unitOfWork.LecturerRepository
                    .Get(l => !l.Deleted && l.Id == id)
                    .FirstOrDefaultAsync();
                if (lecturer is not null)
                {
                    await _cacheService.SetAsync(key, lecturer);
                }
                return lecturer;
            }

            return cachedData;
        }

        public async Task<ServiceResponse> CreateNewLecturer(LecturerVM newEntity)
        {
            var existedUser = await _userManager.FindByNameAsync(newEntity.UserName);
            if (existedUser is not null)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Failed to create new lecturer",
                    Errors = new string[1] { "Username is already taken" }
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
                        Message = "Failed to create new lecturer",
                        Errors = new string[1] { "Email is already exist" }
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
                        Message = "Failed to create new lecturer",
                        Errors = new string[1] { "Phone is already taken" }
                    };
                }
            }

            if (newEntity.DepartmentId is not null)
            {
                var existedDepartment = await _unitOfWork.DepartmentRepository.FindAsync(newEntity.DepartmentId ?? 0);
                if (existedDepartment is null)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Failed to create new lecturer",
                        Errors = new string[1] { "Department not found" }
                    };
                }
            }

            Lecturer newLecturer = new Lecturer
            {
                UserName = newEntity.UserName,
                PhoneNumber = newEntity.PhoneNumber,
                Email = newEntity.Email,
                LockoutEnabled = newEntity.LockoutEnabled ?? true,
                LockoutEnd = newEntity.LockoutEnd,
                DepartmentId = newEntity.DepartmentId,
                EmailConfirmed = false,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                CreatedAt = DateTime.Now,
                CreatedBy = _currentUserService.UserId
            };

            if (newEntity.SubjectIds.Count() > 0)
            {
                List<Subject> subjects = new List<Subject>();
                foreach (int id in newEntity.SubjectIds)
                {
                    var existedSubject = await _unitOfWork.SubjectRepository.FindAsync(id);
                    if (existedSubject is null)
                    {
                        return new ServiceResponse
                        {
                            IsSuccess = false,
                            Message = "Failed to create new lecturer",
                            Errors = new string[1] { "Subject not found with the given id :" + id }
                        };
                    }
                    else
                    {
                        subjects.Add(existedSubject);
                    }
                }
                if (subjects.Count() > 0)
                {
                    newLecturer.Subjects = subjects;
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
                        Message = "Failed to create new lecturer",
                        Errors = new string[1] { "Failed to upload image" }
                    };
                }
                newLecturer.FilePath = uploadResult.SecureUrl.ToString();
            };

            try
            {
                var identityResult = await _userManager.CreateAsync(newLecturer, newEntity.Password);
                if (!identityResult.Succeeded)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Failed to create new lecturer",
                        Errors = identityResult.Errors.Select(e => e.Description)
                    };
                }
                else
                {
                    return new ServiceResponse
                    {
                        Id = newLecturer.Id.ToString(),
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
                    Message = "Failed to create new lecturer",
                    Errors = new List<string>() { ex.Message }
                };
            }
            catch (OperationCanceledException)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Failed to create new lecturer",
                    Errors = new string[1] { "The operation has been cancelled" }
                };
            }
        }

        public async Task<bool> DeleteLecturer(Guid id)
        {
            var existedLecturer = await _unitOfWork.LecturerRepository.Get(l => !l.Deleted && l.Id == id).FirstOrDefaultAsync();
            if(existedLecturer is null)
            {
                return false;
            }
            _unitOfWork.LecturerRepository.Remove(existedLecturer);
            var result = await _unitOfWork.SaveChangesAsync();

            if (result)
            {
                string key = CacheKeyGenerator.GetKeyById(nameof(Lecturer), id.ToString());
                await _cacheService.RemoveAsync(key);
            }

            return result;
        }
    }
}
