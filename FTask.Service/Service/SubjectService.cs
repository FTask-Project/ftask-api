using FTask.Repository.Common;
using FTask.Repository.Data;
using FTask.Repository.Entity;
using FTask.Service.Caching;
using FTask.Service.Validation;
using FTask.Service.ViewModel.RequestVM.Subject;
using FTask.Service.ViewModel.ResposneVM;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FTask.Service.IService
{
    internal class SubjectService : ISubjectService
    {
        private readonly ICheckQuantityTaken _checkQuantityTaken;
        private readonly ICacheService<Subject> _cacheService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        public SubjectService(
            ICheckQuantityTaken checkQuantityTaken,
            ICacheService<Subject> cacheService,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService
            )
        {
            _checkQuantityTaken = checkQuantityTaken;
            _cacheService = cacheService;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Subject?> GetSubjectById(int id)
        {
            string key = CacheKeyGenerator.GetKeyById(nameof(Subject), id.ToString());
            var cachedData = await _cacheService.GetAsync(key);

            if (cachedData is null)
            {
                var includes = new Expression<Func<Subject, object>>[]
                {
                    s => s.Department!
                };
                var subject = await _unitOfWork.SubjectRepository
                    .Get(s => !s.Deleted && s.SubjectId == id, includes)
                    .FirstOrDefaultAsync();
                if (subject is not null)
                {
                    await _cacheService.SetAsync(key, subject);
                }
                return subject;
            }

            return cachedData;
        }

        public async Task<IEnumerable<Subject>> GetSubjectAllSubject(int page, int quantity, string filter, int? departmentId, bool? status)
        {
            if (page == 0)
            {
                page = 1;
            }
            quantity = _checkQuantityTaken.check(quantity);

            var includes = new Expression<Func<Subject, object>>[]
            {
                s => s.Department!
            };

            var subjectList = _unitOfWork.SubjectRepository
                    .Get(s => !s.Deleted && (s.SubjectName.Contains(filter) || s.SubjectCode.Contains(filter)), includes)
                    .Skip((page - 1) * _checkQuantityTaken.PageQuantity)
                    .Take(quantity); ;

            if (departmentId is not null)
            {
                subjectList = subjectList.Where(s => s.DepartmentId == departmentId);
            }

            if (status is not null)
            {
                subjectList = subjectList.Where(s => s.Status == status);
            }

            return await subjectList.ToArrayAsync();
        }

        public async Task<ServiceResponse<Subject>> CreateNewSubject(Subject subjectEntity)
        {
            var isExist = await _unitOfWork.SubjectRepository
                    .Get(subject => subject.SubjectCode == subjectEntity.SubjectCode
                    || subject.SubjectName == subjectEntity.SubjectName)
                    .FirstOrDefaultAsync();
            if (isExist is not null)
            {
                return new ServiceResponse<Subject>
                {
                    IsSuccess = false,
                    Message = "Failed to create new subject",
                    Errors = new string[1] { "Subject code or name already exist" }
                };
            }

            var existedDepartment = await _unitOfWork.DepartmentRepository.FindAsync(subjectEntity.DepartmentId);
            if (existedDepartment is null)
            {
                return new ServiceResponse<Subject>
                {
                    IsSuccess = false,
                    Message = "Failed to create new subject",
                    Errors = new string[1] { "Department not found" }
                };
            }

            subjectEntity.CreatedBy = _currentUserService.UserId;
            subjectEntity.CreatedAt = DateTime.Now;

            await _unitOfWork.SubjectRepository.AddAsync(subjectEntity);

            try
            {
                var result = await _unitOfWork.SaveChangesAsync();
                if (result)
                {
                    string key = CacheKeyGenerator.GetKeyById(nameof(Subject), subjectEntity.SubjectId.ToString());
                    var task = _cacheService.RemoveAsync(key);

                    return new ServiceResponse<Subject>
                    {
                        IsSuccess = true,
                        Message = "Create new subject successfully",
                        Entity = subjectEntity
                    };
                }
                else
                {
                    return new ServiceResponse<Subject>
                    {
                        IsSuccess = false,
                        Message = "Failed to create new subject",
                        Errors = new string[1] { "Can not save changes" }
                    };
                }
            }
            catch (DbUpdateException ex)
            {
                return new ServiceResponse<Subject>
                {
                    IsSuccess = false,
                    Message = "Failed to create new subject",
                    Errors = new List<string>() { ex.Message }
                };
            }
            catch (OperationCanceledException)
            {
                return new ServiceResponse<Subject>
                {
                    IsSuccess = false,
                    Message = "Failed to create new subject",
                    Errors = new string[1] { "The operation has been cancelled" }
                };
            }
        }

        public async Task<bool> DeleteSubject(int id)
        {
            var existedSubject = await _unitOfWork.SubjectRepository.Get(s => !s.Deleted && s.SubjectId == id).FirstOrDefaultAsync();
            if (existedSubject is null)
            {
                return false;
            }
            existedSubject.Deleted = true;
            var result = await _unitOfWork.SaveChangesAsync();

            if (result)
            {
                string key = CacheKeyGenerator.GetKeyById(nameof(Subject), id.ToString());
                await _cacheService.RemoveAsync(key);
            }

            return result;
        }

        public async Task<ServiceResponse<Subject>> UpdateSubject(UpdateSubjectVM updateSubject, int id)
        {
            var existedSubject = await _unitOfWork.SubjectRepository.Get(s => !s.Deleted && s.SubjectId == id).FirstOrDefaultAsync();
            if (existedSubject is null)
            {
                return new ServiceResponse<Subject>
                {
                    IsSuccess = false,
                    Message = "Failed to update subject",
                    Errors = new string[] { "Subject not found" }
                };
            }

            var checkSubject = _unitOfWork.SubjectRepository
                .Get(s => s.SubjectName.Equals(updateSubject.SubjectName) || s.SubjectCode.Equals(updateSubject.SubjectCode))
                .AsNoTracking();

            if (updateSubject.SubjectName is not null)
            {
                if (checkSubject.Where(s => s.SubjectName.Equals(updateSubject.SubjectName)).Count() > 0)
                {
                    return new ServiceResponse<Subject>
                    {
                        IsSuccess = false,
                        Message = "Failed to update subject",
                        Errors = new string[] { $"Subject name '{updateSubject.SubjectName}' is already taken" }
                    };
                }
                existedSubject.SubjectName = updateSubject.SubjectName;
            }

            if (updateSubject.SubjectCode is not null)
            {
                if (checkSubject.Where(s => s.SubjectCode.Equals(updateSubject.SubjectCode)).Count() > 0)
                {
                    return new ServiceResponse<Subject>
                    {
                        IsSuccess = false,
                        Message = "Failed to update subject",
                        Errors = new string[] { $"Subject name '{updateSubject.SubjectName}' is already taken" }
                    };
                }
                existedSubject.SubjectCode = updateSubject.SubjectCode;
            }

            if (updateSubject.DepartmentId is not null)
            {
                var existedDepartment = await _unitOfWork.DepartmentRepository
                    .Get(d => d.DepartmentId == updateSubject.DepartmentId)
                    .FirstOrDefaultAsync();

                if (existedDepartment is null)
                {
                    return new ServiceResponse<Subject>
                    {
                        IsSuccess = false,
                        Message = "Failed to update subject",
                        Errors = new string[] { "Department not found" }
                    };
                }
                existedSubject.DepartmentId = (int)updateSubject.DepartmentId;
            }

            existedSubject.Status = updateSubject.Status ?? existedSubject.Status;

            try
            {
                var result = await _unitOfWork.SaveChangesAsync();
                if (result)
                {
                    string key = CacheKeyGenerator.GetKeyById(nameof(Subject), existedSubject.SubjectId.ToString());
                    var task = _cacheService.RemoveAsync(key);

                    return new ServiceResponse<Subject>
                    {
                        IsSuccess = true,
                        Message = "Update subject successfully",
                        Entity = existedSubject
                    };
                }
                else
                {
                    return new ServiceResponse<Subject>
                    {
                        IsSuccess = false,
                        Message = "Failed to update subject",
                        Errors = new string[1] { "Can not save changes" }
                    };
                }
            }
            catch (DbUpdateException ex)
            {
                return new ServiceResponse<Subject>
                {
                    IsSuccess = false,
                    Message = "Failed to update subject",
                    Errors = new List<string>() { ex.Message }
                };
            }
            catch (OperationCanceledException)
            {
                return new ServiceResponse<Subject>
                {
                    IsSuccess = false,
                    Message = "Failed to update subject",
                    Errors = new string[1] { "The operation has been cancelled" }
                };
            }
        }
    }
}
