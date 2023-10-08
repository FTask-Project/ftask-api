using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FTask.Repository.Data;
using FTask.Repository.Entity;
using FTask.Repository.Identity;
using FTask.Service.Caching;
using FTask.Service.Validation;
using FTask.Service.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FTask.Service.IService
{
    internal class LecturerService : ILecturerService
    {
        private readonly ICheckQuantityTaken _checkQuantityTaken;
        private readonly ICacheService<Lecturer> _cacheService;
        private readonly UserManager<Lecturer> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly Cloudinary _cloudinary;

        public LecturerService(
            ICheckQuantityTaken checkQuantityTaken,
            ICacheService<Lecturer> cacheService,
            UserManager<Lecturer> userManager,
            IUnitOfWork unitOfWork,
            Cloudinary cloudinary
            )
        {
            _checkQuantityTaken = checkQuantityTaken;
            _cacheService = cacheService;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _cloudinary = cloudinary;
        }

        public async Task<IEnumerable<Lecturer>> GetLecturers(int page, int quantity)
        {
            if (page == 0)
            {
                page = 1;
            }
            quantity = _checkQuantityTaken.check(quantity);

            string key = CacheKeyGenerator.GetKeyByPageAndQuantity(nameof(Lecturer), page, quantity);
            var cachedData = await _cacheService.GetAsyncArray(key);
            if (cachedData is null)
            {
                var lecturerList = await _unitOfWork.LecturerRepository
                    .FindAll()
                    .Skip((page - 1) * _checkQuantityTaken.PageQuantity)
                    .Take(quantity)
                    .ToArrayAsync();

                if (lecturerList.Length > 0)
                {
                    await _cacheService.SetAsync(key, lecturerList);
                }
                return lecturerList;
            }
            return cachedData;
        }

        public async Task<Lecturer?> GetLectureById(Guid id)
        {
            string key = CacheKeyGenerator.GetKeyById(nameof(Lecturer), id.ToString());
            var cachedData = await _cacheService.GetAsync(key);

            if (cachedData is null)
            {
                var lecturer = await _unitOfWork.LecturerRepository.FindAsync(id);
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
            try
            {
                var existedUser = await _userManager.FindByNameAsync(newEntity.UserName);
                if (existedUser is not null)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Username is already taken"
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
                            Message = "Email is already exist"
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

                if (newEntity.DepartmentId is not null)
                {
                    var existedDepartment = await _unitOfWork.DepartmentRepository.FindAsync(newEntity.DepartmentId ?? 0);
                    if (existedDepartment is null)
                    {
                        return new ServiceResponse
                        {
                            IsSuccess = false,
                            Message = "Department not found"
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
                    TwoFactorEnabled = false
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
                                Message = "Create new lecturer failed",
                                Errors = new List<string>() { "Subject not found with the given id :" + id }
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
                            Message = "Create new lecturer failed",
                            Errors = new string[1] { "Error when upload image" }
                        };
                    }
                    newLecturer.FilePath = uploadResult.SecureUrl.ToString();
                };

                var identityResult = await _userManager.CreateAsync(newLecturer, newEntity.Password);
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
                        Id = newLecturer.Id.ToString(),
                        IsSuccess = true,
                        Message = "Create new lecturer successfully"
                    };
                }
            }
            /*catch (UniqueConstraintException ex)
            {
                string message = "Created new lecturer failed";
                if (ex.InnerException!.Message.Contains("IX_Lecturer_Email"))
                {
                    message = "Email is already taken";
                }
                if (ex.InnerException!.Message.Contains("IX_Lecturer_PhoneNumber"))
                {
                    message = "Phonenumber is already taken";
                }

                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = message,
                    Errors = new List<string>() { ex.Message }
                };
            }*/
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
}
