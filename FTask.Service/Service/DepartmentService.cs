using FTask.Repository.Common;
using FTask.Repository.Data;
using FTask.Repository.Entity;
using FTask.Service.Caching;
using FTask.Service.Validation;
using FTask.Service.ViewModel.ResposneVM;
using Microsoft.EntityFrameworkCore;

namespace FTask.Service.IService
{
    internal class DepartmentService : IDepartmentService
    {
        private readonly ICheckQuantityTaken _checkQuantityTaken;
        private readonly ICacheService<Department> _cacheService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        public DepartmentService(
            ICheckQuantityTaken checkQuantityTaken,
            ICacheService<Department> cacheService,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService
            )
        {
            _checkQuantityTaken = checkQuantityTaken;
            _cacheService = cacheService;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Department?> GetDepartmentById(int id)
        {
            string key = CacheKeyGenerator.GetKeyById(nameof(Department), id.ToString());
            var cachedData = await _cacheService.GetAsync(key);

            if (cachedData is null)
            {
                var department = await _unitOfWork.DepartmentRepository.Get(d => !d.Deleted && d.DepartmentId == id).FirstOrDefaultAsync();
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
                    .Get(d => !d.Deleted && (d.DepartmentName.Contains(filter) || d.DepartmentCode.Contains(filter)))
                    .Skip((page - 1) * _checkQuantityTaken.PageQuantity)
                    .Take(quantity)
                    .AsNoTracking();

            if (headerId is not null)
            {
                departmentList = departmentList.Where(d => d.DepartmentHeadId == headerId);
            }

            return await departmentList.ToArrayAsync();
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
                        Message = "Failed to create new department",
                        Errors = new string[1] { "Department code or name already exist" }
                    };
                }

                newEntity.CreatedBy = _currentUserService.UserId;
                newEntity.CreatedAt = DateTime.Now;

                await _unitOfWork.DepartmentRepository.AddAsync(newEntity);

                if (newEntity.Subjects.Count() > 0)
                {
                    var list = newEntity.Subjects.ToList();
                    for (int i = 0; i < list.Count(); i++)
                    {
                        list[i].CreatedBy = _currentUserService.UserId;
                        list[i].CreatedAt = DateTime.Now;
                    }
                    newEntity.Subjects = list;
                    await _unitOfWork.SubjectRepository.AddRangeAsync(newEntity.Subjects);
                }

                var result = await _unitOfWork.SaveChangesAsync();
                if (result)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = true,
                        Message = "Created new department successfully",
                        Id = newEntity.DepartmentId.ToString()
                    };
                }
                else
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Failed to create new department",
                        Errors = new List<string>() { "Can not save changes" }
                    };
                }
            }
            catch (DbUpdateException ex)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Failed to create new department",
                    Errors = new List<string>() { ex.Message }
                };
            }
        }

        public async Task<bool> DeleteDepartment(int id)
        {
            var existedDepartment = await _unitOfWork.DepartmentRepository.Get(d => !d.Deleted && d.DepartmentId == id).FirstOrDefaultAsync();
            if(existedDepartment is null)
            {
                return false;
            }
            _unitOfWork.DepartmentRepository.Remove(existedDepartment);
            var result = await _unitOfWork.SaveChangesAsync();

            if (result)
            {
                string key = CacheKeyGenerator.GetKeyById(nameof(Department), id.ToString());
                await _cacheService.RemoveAsync(key);
            }

            return result;
        }
    }
}
