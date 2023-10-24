using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using FTask.Repository.Common;
using FTask.Repository.Data;
using FTask.Repository.Entity;
using FTask.Repository.Identity;
using FTask.Service.Caching;
using FTask.Service.Validation;
using FTask.Service.ViewModel.RequestVM;
using FTask.Service.ViewModel.RequestVM.Lecturer;
using FTask.Service.ViewModel.ResposneVM;
using Google.Apis.Auth.OAuth2;
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

        public async Task<LoginLecturerManagement> LoginWithGoogle(string idToken, bool fromMobile)
        {
            string? email;

            try
            {
                FirebaseToken decodedToken;
                if (fromMobile)
                {
                    if (FirebaseApp.GetInstance("mobileInstance") is null)
                    {
                        FirebaseApp.Create(new AppOptions()
                        {
                            Credential = GoogleCredential.FromFile("mobileServiceAccountKey.json"),
                        }, "mobileInstance");
                    }
                    var mobileInstance = FirebaseApp.GetInstance("mobileInstance");
                    decodedToken = await FirebaseAuth.GetAuth(mobileInstance).VerifyIdTokenAsync(idToken);
                }
                else
                {
                    if (FirebaseApp.GetInstance("webInstance") is null)
                    {
                        FirebaseApp.Create(new AppOptions()
                        {
                            Credential = GoogleCredential.FromFile("webServiceAccountKey.json"),
                        }, "webInstance");
                    }
                    var webInstance = FirebaseApp.GetInstance("webInstance");
                    decodedToken = await FirebaseAuth.GetAuth(webInstance).VerifyIdTokenAsync(idToken);
                }

                email = decodedToken.Claims["email"].ToString();
            }
            catch (FirebaseAuthException ex)
            {
                return new LoginLecturerManagement
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }


            var existedLecturer = await _userManager.FindByEmailAsync(email);
            if(existedLecturer is null || existedLecturer.Deleted)
            {
                return new LoginLecturerManagement
                {
                    Message = "Email not found",
                    IsSuccess = false,
                };
            }

            if (existedLecturer.LockoutEnabled)
            {
                return new LoginLecturerManagement
                {
                    Message = "Account is locked",
                    IsSuccess = false,
                };
            }

            return new LoginLecturerManagement
            {
                Message = "Login Successfully",
                IsSuccess = true,
                LoginUser = existedLecturer
            };
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
                    .Get(l => !l.Deleted && l.Id == id, includes)
                    .FirstOrDefaultAsync();
                if (lecturer is not null)
                {
                    await _cacheService.SetAsync(key, lecturer);
                }
                return lecturer;
            }

            return cachedData;
        }

        public async Task<ServiceResponse<Lecturer>> CreateNewLecturer(LecturerVM newEntity)
        {
            var existedUser = await _userManager.FindByNameAsync(newEntity.UserName);
            if (existedUser is not null)
            {
                return new ServiceResponse<Lecturer>
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
                    return new ServiceResponse<Lecturer>
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
                    return new ServiceResponse<Lecturer>
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
                    return new ServiceResponse<Lecturer>
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
                DisplayName = newEntity.DisplayName,
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
                        return new ServiceResponse<Lecturer>
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
                    return new ServiceResponse<Lecturer>
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
                    return new ServiceResponse<Lecturer>
                    {
                        IsSuccess = false,
                        Message = "Failed to create new lecturer",
                        Errors = identityResult.Errors.Select(e => e.Description)
                    };
                }
                else
                {
                    return new ServiceResponse<Lecturer>
                    {
                        Entity = newLecturer,
                        IsSuccess = true,
                        Message = "Create new lecturer successfully"
                    };
                }
            }
            catch (DbUpdateException ex)
            {
                return new ServiceResponse<Lecturer>
                {
                    IsSuccess = false,
                    Message = "Failed to create new lecturer",
                    Errors = new List<string>() { ex.Message }
                };
            }
            catch (OperationCanceledException)
            {
                return new ServiceResponse<Lecturer>
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
            if (existedLecturer is null)
            {
                return false;
            }
            existedLecturer.Deleted = true;
            var result = await _unitOfWork.SaveChangesAsync();

            if (result)
            {
                string key = CacheKeyGenerator.GetKeyById(nameof(Lecturer), id.ToString());
                await _cacheService.RemoveAsync(key);
            }

            return result;
        }

        public async Task<ServiceResponse<Lecturer>> UpdateLecturer(UpdateLecturerVM updateLecturer, Guid id)
        {
            Expression<Func<Lecturer, object>>[] includes = new Expression<Func<Lecturer, object>>[3]
                {
                    l => l.DepartmentHead!,
                    l => l.Department!,
                    l => l.Subjects
                };

            var existedLecturer = await _unitOfWork.LecturerRepository
                .Get(l => !l.Deleted && l.Id == id, includes)
                .FirstOrDefaultAsync();

            if (existedLecturer is null)
            {
                return new ServiceResponse<Lecturer>
                {
                    IsSuccess = false,
                    Message = "Failed to update lecturer",
                    Errors = new string[] { "Lecturer not found" }
                };
            }

            var checkLecturer = _unitOfWork.LecturerRepository
                .Get(l => l.PhoneNumber.Equals(updateLecturer.PhoneNumber) || l.Email.Equals(updateLecturer.Email))
                .AsNoTracking();
            if (updateLecturer.Email is not null)
            {
                if (checkLecturer.Any(l => l.Email.Equals(updateLecturer.Email)))
                {
                    return new ServiceResponse<Lecturer>
                    {
                        IsSuccess = false,
                        Message = "Failed to update lecturer",
                        Errors = new string[] { "Email is already taken" }
                    };
                }
                existedLecturer.Email = updateLecturer.Email;
            }

            if (updateLecturer.PhoneNumber is not null)
            {
                if (checkLecturer.Any(l => l.PhoneNumber.Equals(updateLecturer.PhoneNumber)))
                {
                    return new ServiceResponse<Lecturer>
                    {
                        IsSuccess = false,
                        Message = "Failed to update lecturer",
                        Errors = new string[] { "Phone number is already taken" }
                    };
                }
                existedLecturer.PhoneNumber = updateLecturer.PhoneNumber;
            }

            if (updateLecturer.DepartmentId is not null)
            {
                if (updateLecturer.DepartmentId == 0)
                {
                    existedLecturer.DepartmentId = null;
                }
                else
                {
                    var existedDepartment = await _unitOfWork.DepartmentRepository.Get(d => !d.Deleted && d.DepartmentId == updateLecturer.DepartmentId).FirstOrDefaultAsync();
                    if (existedDepartment is null)
                    {
                        return new ServiceResponse<Lecturer>
                        {
                            IsSuccess = false,
                            Message = "Failed to update lecturer",
                            Errors = new string[] { "Department not found" }
                        };
                    }
                    existedLecturer.DepartmentId = updateLecturer.DepartmentId;
                }
            }

            if (updateLecturer.SubjectIds is not null)
            {
                var subjectList = await _unitOfWork.SubjectRepository.Get(s => !s.Deleted && updateLecturer.SubjectIds.Contains(s.SubjectId)).ToListAsync();
                existedLecturer.Subjects = subjectList;
            }

            if (updateLecturer.Avatar is not null && updateLecturer.Avatar.Length > 0)
            {
                var uploadFile = new ImageUploadParams
                {
                    File = new FileDescription(updateLecturer.Avatar.FileName, updateLecturer.Avatar.OpenReadStream())
                };
                var uploadResult = await _cloudinary.UploadAsync(uploadFile);

                if (uploadResult.Error is not null)
                {
                    return new ServiceResponse<Lecturer>
                    {
                        IsSuccess = false,
                        Message = "Failed to update lecturer",
                        Errors = new string[1] { "Failed to upload image" }
                    };
                }

                existedLecturer.FilePath = uploadResult.SecureUrl.ToString();
            }

            existedLecturer.LockoutEnabled = updateLecturer.LockoutEnabled ?? existedLecturer.LockoutEnabled;
            existedLecturer.LockoutEnd = updateLecturer.LockoutEnd ?? existedLecturer.LockoutEnd;
            existedLecturer.DisplayName = updateLecturer.DisplayName ?? existedLecturer.DisplayName;

            try
            {
                var result = await _unitOfWork.SaveChangesAsync();
                if (result)
                {
                    string key = CacheKeyGenerator.GetKeyById(nameof(Lecturer), existedLecturer.Id.ToString());
                    var task = _cacheService.RemoveAsync(key);

                    return new ServiceResponse<Lecturer>
                    {
                        IsSuccess = true,
                        Message = "Update lecturer successfully",
                        Entity = existedLecturer
                    };
                }
                else
                {
                    return new ServiceResponse<Lecturer>
                    {
                        IsSuccess = false,
                        Message = "Failed to update lecturer",
                        Errors = new string[1] { "Can not save changes" }
                    };
                }
            }
            catch (DbUpdateException ex)
            {
                return new ServiceResponse<Lecturer>
                {
                    IsSuccess = false,
                    Message = "Failed to update lecturer",
                    Errors = new List<string>() { ex.Message }
                };
            }
            catch (OperationCanceledException)
            {
                return new ServiceResponse<Lecturer>
                {
                    IsSuccess = false,
                    Message = "Failed to update lecturer",
                    Errors = new string[1] { "The operation has been cancelled" }
                };
            }
        }
    }
}
