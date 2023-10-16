using Duende.IdentityServer.Extensions;
using FTask.Repository.Common;
using FTask.Repository.Data;
using FTask.Repository.Entity;
using FTask.Service.Caching;
using FTask.Service.Validation;
using FTask.Service.ViewModel.RequestVM.CreateSemester;
using FTask.Service.ViewModel.ResposneVM;
using Microsoft.EntityFrameworkCore;

namespace FTask.Service.IService
{
    internal class SemesterService : ISemesterService
    {
        private readonly ICheckSemesterPeriod _checkSemesterPeriod;
        private readonly ICheckQuantityTaken _checkQuantityTaken;
        private readonly ICacheService<Semester> _cacheService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        public SemesterService(
            ICheckSemesterPeriod checkSemesterPeriod,
            ICheckQuantityTaken checkQuantityTaken,
            ICacheService<Semester> cacheService,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService
            )
        {
            _checkSemesterPeriod = checkSemesterPeriod;
            _checkQuantityTaken = checkQuantityTaken;
            _cacheService = cacheService;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Semester?> GetSemesterById(int id)
        {
            string key = CacheKeyGenerator.GetKeyById(nameof(Semester), id.ToString());
            var cachedData = await _cacheService.GetAsync(key);

            if (cachedData is null)
            {
                var semester = await _unitOfWork.SemesterRepository.Get(s => !s.Deleted && s.SemesterId == id).FirstOrDefaultAsync();
                if (semester is not null)
                {
                    await _cacheService.SetAsync(key, semester);
                }
                return semester;
            }

            return cachedData;
        }

        public async Task<IEnumerable<Semester>> GetSemesters(int page, int quantity, string filter)
        {
            if (page == 0)
            {
                page = 1;
            }
            quantity = _checkQuantityTaken.check(quantity);

            var semesterList = _unitOfWork.SemesterRepository
                    .Get(s => !s.Deleted && s.SemesterCode.Contains(filter))
                    .Skip((page - 1) * _checkQuantityTaken.PageQuantity)
                    .Take(quantity);
            return await semesterList.ToArrayAsync();
        }

        public async Task<ServiceResponse> CreateNewSemester(SemesterVM newEntity)
        {
            try
            {
                if (newEntity.SemesterCode.IsNullOrEmpty())
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Failed to create new semester",
                        Errors = new string[1] { "Invalid semester code" }
                    };
                }

                var existedSemester = await _unitOfWork.SemesterRepository.Get(s => newEntity.SemesterCode!.Equals(s.SemesterCode)).FirstOrDefaultAsync();
                if (existedSemester is not null)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Failed to create new semester",
                        Errors = new string[1] {"Semester code is already taken"}
                    };
                }

                if (newEntity.EndDate < newEntity.StartDate)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Failed to create new semester",
                        Errors = new string[1] {"End date must be greater than start date"}
                    };
                }

                if (!_checkSemesterPeriod.CheckValidDuration(newEntity.StartDate, newEntity.EndDate))
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Failed to create new semester",
                        Errors = new string[1] { $"The duration of semester must be greater than {_checkSemesterPeriod.MinimumDuration} days and less than {_checkSemesterPeriod.MaximumDuration} days" }
                    };
                }

                if (!(await _checkSemesterPeriod.IsValidStartDate(newEntity.StartDate, _unitOfWork)))
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Failed to create new semester",
                        Errors = new string[1] { "The new semester must start after the latest semester" }
                    };
                }

                var newSemester = new Semester
                {
                    SemesterCode = newEntity.SemesterCode,
                    StartDate = newEntity.StartDate,
                    EndDate = newEntity.EndDate,
                    CreatedAt = DateTime.Now,
                    CreatedBy = _currentUserService.UserId
                };

                await _unitOfWork.SemesterRepository.AddAsync(newSemester);
                var result = await _unitOfWork.SaveChangesAsync();
                if (result)
                {
                    return new ServiceResponse
                    {
                        Id = newSemester.SemesterId.ToString(),
                        IsSuccess = true,
                        Message = "Create new semester successfully"
                    };
                }
                else
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Failed to create new semester",
                        Errors = new List<string>() { "Error at create new semester service", "Can not save changes" }
                    };
                }
            }
            catch (DbUpdateException ex)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Failed to create new semester",
                    Errors = new List<string>() { ex.Message }
                };
            }
        }

        public async Task<bool> DeleteSemester(int id)
        {
            var existedSemester = await _unitOfWork.SemesterRepository.Get(s => !s.Deleted && s.SemesterId == id).FirstOrDefaultAsync();
            if(existedSemester is null)
            {
                return false;
            }
            _unitOfWork.SemesterRepository.Remove(existedSemester);
            var result = await _unitOfWork.SaveChangesAsync();

            if (result)
            {
                string key = CacheKeyGenerator.GetKeyById(nameof(Semester), id.ToString());
                await _cacheService.RemoveAsync(key);
            }

            return result;
        }
    }
}
