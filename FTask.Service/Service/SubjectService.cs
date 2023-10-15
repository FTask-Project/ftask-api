using FTask.Repository.Data;
using FTask.Repository.Entity;
using FTask.Service.Caching;
using FTask.Service.Validation;
using FTask.Service.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace FTask.Service.IService
{
    internal class SubjectService : ISubjectService
    {
        private readonly ICheckQuantityTaken _checkQuantityTaken;
        private readonly ICacheService<Subject> _cacheService;
        private readonly IUnitOfWork _unitOfWork;
        public SubjectService(
            ICheckQuantityTaken checkQuantityTaken,
            ICacheService<Subject> cacheService,
            IUnitOfWork unitOfWork
            )
        {
            _checkQuantityTaken = checkQuantityTaken;
            _cacheService = cacheService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Subject?> GetSubjectById(int id)
        {
            string key = CacheKeyGenerator.GetKeyById(nameof(Subject), id.ToString());
            var cachedData = await _cacheService.GetAsync(key);

            if (cachedData is null)
            {
                var subject = await _unitOfWork.SubjectRepository.FindAsync(id);
                if (subject is not null)
                {
                    await _cacheService.SetAsync(key, subject);
                }
                return subject;
            }

            return cachedData;
        }

        public async Task<IEnumerable<Subject>> GetSubjectAllSubject(int page, int quantity, string filter, int? departmentId)
        {
            if (page == 0)
            {
                page = 1;
            }
            quantity = _checkQuantityTaken.check(quantity);

            var subjectList = await _unitOfWork.SubjectRepository
                    .Get(s => s.SubjectName.Contains(filter) || s.SubjectCode.Contains(filter))
                    .Skip((page - 1) * _checkQuantityTaken.PageQuantity)
                    .Take(quantity)
                    .ToArrayAsync();

            if (departmentId is not null)
            {
                subjectList = subjectList.Where(s => s.DepartmentId == departmentId).ToArray();
            }

            return subjectList;

            /*string key = CacheKeyGenerator.GetKeyByPageAndQuantity(nameof(Subject), page, quantity);
            var cachedData = await _cacheService.GetAsyncArray(key);
            if (cachedData.IsNullOrEmpty())
            {
                

                if (subjectList.Count() > 0)
                {
                    await _cacheService.SetAsyncArray(key, subjectList);
                }
                
            }

            return cachedData;*/
        }

        public async Task<IEnumerable<Subject>> GetSubjectFromDepartment(int departmentId)
        {
            string key = CacheKeyGenerator.GetKeyByOtherId(nameof(Subject), nameof(Department), departmentId.ToString());
            var cachedData = await _cacheService.GetAsyncArray(key);
            if (cachedData is null)
            {
                var subjectList = await _unitOfWork.SubjectRepository
                  .Get(subject => subject.DepartmentId == departmentId)
                  .ToArrayAsync();

                if (subjectList.Count() > 0)
                {
                    await _cacheService.SetAsyncArray(key, subjectList);
                }
                return subjectList;
            }

            return cachedData;
        }

        public async Task<ServiceResponse> CreateNewSubject(Subject subjectEntity)
        {
            try
            {
                var isExist = await _unitOfWork.SubjectRepository
                    .Get(subject => subject.SubjectCode == subjectEntity.SubjectCode
                    || subject.SubjectName == subject.SubjectName)
                    .FirstOrDefaultAsync();
                if (isExist is not null)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Subject code or subject name already exist"
                    };
                }

                var existedDepartment = await _unitOfWork.DepartmentRepository.FindAsync(subjectEntity.DepartmentId);
                if (existedDepartment is null)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Department not found"
                    };
                }

                await _unitOfWork.SubjectRepository.AddAsync(subjectEntity);
                var result = await _unitOfWork.SaveChangesAsync();

                if (result)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = true,
                        Message = "Subject was created successfully",
                        Id = subjectEntity.SubjectId.ToString()
                    };
                }
                else
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Create new subject failed"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Some error happend",
                    Errors = new List<string>() { ex.Message }
                };
            }
        }

        public async Task<ServiceResponse> UpdateSubject(Subject subjectEntity)
        {
            try
            {
                var isExist = await _unitOfWork.SubjectRepository
                    .Get(subject => subject.SubjectCode == subjectEntity.SubjectCode
                    || subject.SubjectName == subject.SubjectName)
                    .FirstOrDefaultAsync();
                if (isExist is null)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Subject code or subject name not exist"
                    };
                }

                _unitOfWork.SubjectRepository.Update(subjectEntity);
                var result = await _unitOfWork.SaveChangesAsync();

                if (result)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = true,
                        Message = "Subject was updated successfully",
                        Id = subjectEntity.SubjectId.ToString(),
                    };
                }
                else
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Update subject failed"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Some error happend",
                    Errors = new List<string>() { ex.Message }
                };
            }
        }

        public async Task<ServiceResponse> DeleteSubject(Subject subjectEntity)
        {
            try
            {
                var isExist = await _unitOfWork.SubjectRepository
                    .Get(subject => subject.SubjectCode == subjectEntity.SubjectCode
                    || subject.SubjectName == subject.SubjectName)
                    .FirstOrDefaultAsync();
                if (isExist is null)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Subject code or subject name not exist"
                    };
                }

                _unitOfWork.SubjectRepository.Remove(subjectEntity);
                var result = await _unitOfWork.SaveChangesAsync();

                if (result)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = true,
                        Message = "Subject was deleted successfully"
                    };
                }
                else
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Delete subject failed"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Some error happend",
                    Errors = new List<string>() { ex.Message }
                };
            }
        }
    }
}
