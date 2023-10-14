using FTask.Repository.Data;
using FTask.Repository.Entity;
using FTask.Service.Caching;
using FTask.Service.Validation;
using FTask.Service.ViewModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Linq.Expressions;

namespace FTask.Service.IService
{
    internal class DepartmentService : IDepartmentService
    {
        private readonly ICheckQuantityTaken _checkQuantityTaken;
        private readonly ICacheService<Department> _cacheService;
        private readonly IUnitOfWork _unitOfWork;
        public DepartmentService(
            ICheckQuantityTaken checkQuantityTaken,
            ICacheService<Department> cacheService,
            IUnitOfWork unitOfWork
            )
        {
            _checkQuantityTaken = checkQuantityTaken;
            _cacheService = cacheService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Department?> GetDepartmentById(int id)
        {
            string key = CacheKeyGenerator.GetKeyById(nameof(Department), id.ToString());
            var cachedData = await _cacheService.GetAsync(key);

            if (cachedData is null)
            {
                var department = await _unitOfWork.DepartmentRepository.FindAsync(id);
                if (department is not null)
                {
                    await _cacheService.SetAsync(key, department);
                }
                return department;
            }

            return cachedData;
        }

        public async Task<IEnumerable<Department>> GetDepartments(int page, int quantity, string filter, Guid? headerId)
        {
            if (page == 0)
            {
                page = 1;
            }
            quantity = _checkQuantityTaken.check(quantity);

            var departmentList = _unitOfWork.DepartmentRepository
                    .Get(d => d.DepartmentName.Contains(filter) || d.DepartmentCode.Contains(filter))
                    .Skip((page - 1) * _checkQuantityTaken.PageQuantity)
                    .Take(quantity);

            if(headerId is not null)
            {
                departmentList = departmentList.Where(d => d.DepartmentHeadId == headerId);
            }

            return await departmentList.ToArrayAsync();

            /*string key = CacheKeyGenerator.GetKeyByPageAndQuantity(nameof(Department), page, quantity);
            var cachedData = await _cacheService.GetAsyncArray(key);
            if (cachedData.IsNullOrEmpty())
            {
                
                if (departmentList.Count() > 0)
                {
                    await _cacheService.SetAsyncArray(key, departmentList);
                }
                
            }

            return cachedData;*/
        }

        public async Task<ServiceResponse> CreateNewDepartment(Department newEntity)
        {
            // For create update delete, should catch DbUpdateException
            try
            {
                // Check if lecturer with the given id exist
                if (newEntity.DepartmentHeadId is not null)
                {
                    var existedLecturer = await _unitOfWork.LecturerRepository.FindAsync((Guid)newEntity.DepartmentHeadId);
                    if (existedLecturer is null)
                    {
                        return new ServiceResponse
                        {
                            IsSuccess = false,
                            Message = "Lecturer not found"
                        };
                    }
                }

                // check code and name are unique or not
                var existedDepartment = await _unitOfWork.DepartmentRepository
                    .Get(d => d.DepartmentCode == newEntity.DepartmentCode || d.DepartmentName == newEntity.DepartmentName)
                    .FirstOrDefaultAsync();
                if (existedDepartment is not null)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Department code or name already exist"
                    };
                }

                await _unitOfWork.DepartmentRepository.AddAsync(newEntity);

                if (newEntity.Subjects.Count() > 0)
                {
                    await _unitOfWork.SubjectRepository.AddRangeAsync(newEntity.Subjects);
                }

                var result = await _unitOfWork.SaveChangesAsync();
                if (result)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = true,
                        Message = "Created new department",
                        Id = newEntity.DepartmentId.ToString()
                    };
                }
                else
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Create new department failed",
                        Errors = new List<string>() { "Error at create new department service", "Can not save changes" }
                    };
                }
            }
            catch (DbUpdateException ex)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Create new department failed",
                    Errors = new List<string>() { ex.Message }
                };
            }
        }
    }
}
