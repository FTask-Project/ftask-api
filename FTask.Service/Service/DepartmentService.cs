using CloudinaryDotNet;
using FTask.Repository.Common;
using FTask.Repository.Data;
using FTask.Repository.Entity;
using FTask.Repository.Identity;
using FTask.Service.Caching;
using FTask.Service.Validation;
using FTask.Service.ViewModel.RequestVM.Department;
using FTask.Service.ViewModel.ResposneVM;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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
                Expression<Func<Department, object>>[] includes = new Expression<Func<Department, object>>[3]
                {
                    d => d.Lecturers,
                    d => d.DepartmentHead!,
                    d => d.Subjects
                };
                var department = await _unitOfWork.DepartmentRepository.Get(d => !d.Deleted && d.DepartmentId == id, includes).FirstOrDefaultAsync();
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

        public async Task<ServiceResponse<Department>> CreateNewDepartment(Department newEntity)
        {
            // Check if lecturer with the given id exist
            if (newEntity.DepartmentHeadId is not null)
            {
                var existedLecturer = await _unitOfWork.LecturerRepository
                    .Get(l => l.Id == newEntity.DepartmentHeadId,
                    new Expression<Func<Lecturer, object>>[]
                    {
                        l => l.DepartmentHead!
                    })
                    .FirstOrDefaultAsync();
                if (existedLecturer is null)
                {
                    return new ServiceResponse<Department>
                    {
                        IsSuccess = false,
                        Message = "Failed to create new department",
                        Errors = new string[] { "Lecturer not found" }
                    };
                }
                if(existedLecturer.DepartmentHead is not null)
                {
                    return new ServiceResponse<Department>
                    {
                        IsSuccess = false,
                        Message = "Failed to create new department",
                        Errors = new string[] { $"Lecturer already is head of department '{existedLecturer.DepartmentHead.DepartmentName}'" }
                    };
                }
            }

            // check code and name are unique or not
            var existedDepartment = await _unitOfWork.DepartmentRepository
                .Get(d => d.DepartmentCode == newEntity.DepartmentCode || d.DepartmentName == newEntity.DepartmentName)
                .FirstOrDefaultAsync();
            if (existedDepartment is not null)
            {
                return new ServiceResponse<Department>
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

            // For create update delete, should catch DbUpdateException
            try
            {
                var result = await _unitOfWork.SaveChangesAsync();
                if (result)
                {
                    return new ServiceResponse<Department>
                    {
                        IsSuccess = true,
                        Message = "Created new department successfully",
                        Entity = newEntity
                    };
                }
                else
                {
                    return new ServiceResponse<Department>
                    {
                        IsSuccess = false,
                        Message = "Failed to create new department",
                        Errors = new List<string>() { "Can not save changes" }
                    };
                }
            }
            catch (DbUpdateException ex)
            {
                return new ServiceResponse<Department>
                {
                    IsSuccess = false,
                    Message = "Failed to create new department",
                    Errors = new string[1] { ex.Message }
                };
            }
            catch(OperationCanceledException)
            {
                return new ServiceResponse<Department>
                {
                    IsSuccess = false,
                    Message = "Failed to create new department",
                    Errors = new string[1] {"The operation has been cancelled"}
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

        public async Task<ServiceResponse<Department>> UpdateDepartment(UpdateDepartmentVM updateDepartment, int id)
        {
            var existedDepartment = await _unitOfWork.DepartmentRepository.Get(d => !d.Deleted && d.DepartmentId == id).FirstOrDefaultAsync();
            if(existedDepartment is null)
            {
                return new ServiceResponse<Department>
                {
                    IsSuccess = false,
                    Message = "Failed to update department",
                    Errors = new string[] { "Department not found" }
                };
            }

            if(updateDepartment.DepartmentHeadId is not null)
            {
                if(updateDepartment.DepartmentHeadId == new Guid("00000000-0000-0000-0000-000000000000"))
                {
                    existedDepartment.DepartmentHeadId = null;
                }
                else
                {
                    var existedLecturer = await _unitOfWork.LecturerRepository
                    .Get(l => l.Id == updateDepartment.DepartmentHeadId,
                    new Expression<Func<Lecturer, object>>[]
                    {
                        l => l.DepartmentHead!
                    })
                    .FirstOrDefaultAsync();
                    if (existedLecturer is null)
                    {
                        return new ServiceResponse<Department>
                        {
                            IsSuccess = false,
                            Message = "Failed to update department",
                            Errors = new string[] { "Lecturer not found" }
                        };
                    }
                    if (existedLecturer.DepartmentHead is not null)
                    {
                        return new ServiceResponse<Department>
                        {
                            IsSuccess = false,
                            Message = "Failed to update department",
                            Errors = new string[] { $"Lecturer is already head of department '{existedLecturer.DepartmentHead.DepartmentName}'" }
                        };
                    }
                    existedDepartment.DepartmentHeadId = updateDepartment.DepartmentHeadId;
                }
            }

            var checkDepartment = _unitOfWork.DepartmentRepository
                .Get(d => d.DepartmentName.Equals(updateDepartment.DepartmentName) || d.DepartmentCode.Equals(updateDepartment.DepartmentCode))
                .AsNoTracking();

            if(updateDepartment.DepartmentName is not null)
            {
                if(checkDepartment.Any(d => d.DepartmentName.Equals(updateDepartment.DepartmentName)))
                {
                    return new ServiceResponse<Department>
                    {
                        IsSuccess = false,
                        Message = "Failed to update department",
                        Errors = new string[] { "Department name is already taken" }
                    };
                }
                existedDepartment.DepartmentName = updateDepartment.DepartmentName;
            }

            if (updateDepartment.DepartmentCode is not null)
            {
                if(checkDepartment.Any(d => d.DepartmentCode.Equals(updateDepartment.DepartmentCode)))
                {
                    return new ServiceResponse<Department>
                    {
                        IsSuccess = false,
                        Message = "Failed to update department",
                        Errors = new string[] { "Department code is already taken" }
                    };
                }
                existedDepartment.DepartmentCode = updateDepartment.DepartmentCode;
            }

            try
            {
                var result = await _unitOfWork.SaveChangesAsync();
                if (result)
                {
                    string key = CacheKeyGenerator.GetKeyById(nameof(Department), existedDepartment.DepartmentId.ToString());
                    var task = _cacheService.RemoveAsync(key);

                    return new ServiceResponse<Department>
                    {
                        IsSuccess = true,
                        Message = "Update department successfully",
                        Entity = existedDepartment
                    };
                }
                else
                {
                    return new ServiceResponse<Department>
                    {
                        IsSuccess = false,
                        Message = "Failed to update department",
                        Errors = new List<string>() { "Can not save changes" }
                    };
                }
            }
            catch (DbUpdateException ex)
            {
                return new ServiceResponse<Department>
                {
                    IsSuccess = false,
                    Message = "Failed to update department",
                    Errors = new string[1] { ex.Message }
                };
            }
            catch (OperationCanceledException)
            {
                return new ServiceResponse<Department>
                {
                    IsSuccess = false,
                    Message = "Failed to update department",
                    Errors = new string[1] { "The operation has been cancelled" }
                };
            }
        }
    }
}
