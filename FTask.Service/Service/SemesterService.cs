using AutoMapper;
using Duende.IdentityServer.Extensions;
using FTask.Repository.Common;
using FTask.Repository.Data;
using FTask.Repository.Entity;
using FTask.Service.Caching;
using FTask.Service.Validation;
using FTask.Service.ViewModel.RequestVM.Semester;
using FTask.Service.ViewModel.ResposneVM;
using Microsoft.EntityFrameworkCore;

namespace FTask.Service.IService
{
    internal class SemesterService : ISemesterService
    {
        private readonly ISemesterValidation _semesterValidation;
        private readonly ICheckQuantityTaken _checkQuantityTaken;
        private readonly ICacheService<Semester> _cacheService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        public SemesterService(
            ISemesterValidation semesterValidation,
            ICheckQuantityTaken checkQuantityTaken,
            ICacheService<Semester> cacheService,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IMapper mapper
            )
        {
            _semesterValidation = semesterValidation;
            _checkQuantityTaken = checkQuantityTaken;
            _cacheService = cacheService;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<Semester?> GetSemesterById(int id)
        {
            string key = CacheKeyGenerator.GetKeyById(nameof(Semester), id.ToString());
            var cachedData = await _cacheService.GetAsync(key);

            if (cachedData is null)
            {
                var semester = await _unitOfWork.SemesterRepository
                    .Get(s => !s.Deleted && s.SemesterId == id)
                    .FirstOrDefaultAsync();
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

        public async Task<ServiceResponse<Semester>> CreateNewSemester(SemesterVM newEntity)
        {
            if (newEntity.SemesterCode.IsNullOrEmpty())
            {
                return new ServiceResponse<Semester>
                {
                    IsSuccess = false,
                    Message = "Failed to create new semester",
                    Errors = new string[1] { "Invalid semester code" }
                };
            }

            var existedSemester = await _unitOfWork.SemesterRepository.Get(s => newEntity.SemesterCode!.Equals(s.SemesterCode)).FirstOrDefaultAsync();
            if (existedSemester is not null)
            {
                return new ServiceResponse<Semester>
                {
                    IsSuccess = false,
                    Message = "Failed to create new semester",
                    Errors = new string[1] { "Semester code is already taken" }
                };
            }

            var validationResult = await _semesterValidation.ValidateSemester(newEntity.StartDate, newEntity.EndDate, _unitOfWork.SemesterRepository);
            if (!validationResult.IsSuccess)
            {
                return validationResult;
            }

            var newSemester = _mapper.Map<Semester>(newEntity);

            await _unitOfWork.SemesterRepository.AddAsync(newSemester);

            try
            {
                var result = await _unitOfWork.SaveChangesAsync();
                if (result)
                {
                    return new ServiceResponse<Semester>
                    {
                        Entity = newSemester,
                        IsSuccess = true,
                        Message = "Create new semester successfully"
                    };
                }
                else
                {
                    return new ServiceResponse<Semester>
                    {
                        IsSuccess = false,
                        Message = "Failed to create new semester",
                        Errors = new List<string>() { "Can not save changes" }
                    };
                }
            }
            catch (DbUpdateException ex)
            {
                return new ServiceResponse<Semester>
                {
                    IsSuccess = false,
                    Message = "Failed to create new semester",
                    Errors = new List<string>() { ex.Message }
                };
            }
            catch (OperationCanceledException)
            {
                return new ServiceResponse<Semester>
                {
                    IsSuccess = false,
                    Message = "Failed to create new semester",
                    Errors = new string[1] { "The operation has been cancelled" }
                };
            }
        }

        public async Task<bool> DeleteSemester(int id)
        {
            var existedSemester = await _unitOfWork.SemesterRepository.Get(s => !s.Deleted && s.SemesterId == id).FirstOrDefaultAsync();
            if (existedSemester is null)
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

        public async Task<ServiceResponse<Semester>> UpdateSemester(UpdateSemesterVM updateSemester, int semesterId)
        {
            var existedSemester = await _unitOfWork.SemesterRepository.Get(s => !s.Deleted && s.SemesterId == semesterId).FirstOrDefaultAsync();
            if (existedSemester is null)
            {
                return new ServiceResponse<Semester>
                {
                    IsSuccess = false,
                    Message = "Failed to update semester",
                    Errors = new string[] { "Semester not found" }
                };
            }

            var validationResult = await _semesterValidation.ValidateSemester(updateSemester.StartDate ?? existedSemester.StartDate, updateSemester.EndDate ?? existedSemester.EndDate, _unitOfWork.SemesterRepository);
            if (!validationResult.IsSuccess)
            {
                return validationResult;
            }

            if (updateSemester.SemesterCode is not null)
            {
                var checkSemester = _unitOfWork.SemesterRepository.Get(s => s.SemesterCode.Equals(updateSemester.SemesterCode));
                if (checkSemester.Count() > 0)
                {
                    return new ServiceResponse<Semester>
                    {
                        IsSuccess = false,
                        Message = "Failed to update semester",
                        Errors = new string[] { "Semester code is already taken" }
                    };
                }
                existedSemester.SemesterCode = updateSemester.SemesterCode;
            }

            existedSemester.StartDate = updateSemester.StartDate ?? existedSemester.StartDate;
            existedSemester.EndDate = updateSemester.EndDate ?? existedSemester.EndDate;

            try
            {
                var result = await _unitOfWork.SaveChangesAsync();
                if (result)
                {
                    string key = CacheKeyGenerator.GetKeyById(nameof(Semester), existedSemester.SemesterId.ToString());
                    var task = _cacheService.RemoveAsync(key);

                    return new ServiceResponse<Semester>
                    {
                        Entity = existedSemester,
                        IsSuccess = true,
                        Message = "Update semester successfully"
                    };
                }
                else
                {
                    return new ServiceResponse<Semester>
                    {
                        IsSuccess = false,
                        Message = "Failed to update semester",
                        Errors = new List<string>() { "Can not save changes" }
                    };
                }
            }
            catch (DbUpdateException ex)
            {
                return new ServiceResponse<Semester>
                {
                    IsSuccess = false,
                    Message = "Failed to update semester",
                    Errors = new List<string>() { ex.Message }
                };
            }
            catch (OperationCanceledException)
            {
                return new ServiceResponse<Semester>
                {
                    IsSuccess = false,
                    Message = "Failed to update semester",
                    Errors = new string[1] { "The operation has been cancelled" }
                };
            }
        }
    }
}
