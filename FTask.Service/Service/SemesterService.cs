using Duende.IdentityServer.Extensions;
using FTask.Repository.Data;
using FTask.Repository.Entity;
using FTask.Service.Validation;
using FTask.Service.ViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTask.Service.IService
{
    internal class SemesterService : ISemesterService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICheckQuantityTaken _checkQuantityTaken;
        private readonly ICheckSemesterPeriod _checkSemesterPeriod;
        public SemesterService(IUnitOfWork unitOfWork, ICheckQuantityTaken checkQuantityTaken, ICheckSemesterPeriod checkSemesterPeriod)
        {
            _unitOfWork = unitOfWork;
            _checkQuantityTaken = checkQuantityTaken;
            _checkSemesterPeriod = checkSemesterPeriod;
        }

        public async Task<Semester?> GetSemesterById(int id)
        {
            return await _unitOfWork.SemesterRepository.FindAsync(id);
        }

        public async Task<IEnumerable<Semester>> GetSemesters(int page, int quantity)
        {
            if (page == 0)
            {
                page = 1;
            }
            quantity = _checkQuantityTaken.check(quantity);
            return await _unitOfWork.SemesterRepository
                .FindAll()
                .Skip((page - 1) * _checkQuantityTaken.PageQuantity)
                .Take(quantity)
                .ToArrayAsync();
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
                        Message = "Invalid semester code"
                    };
                }

                var existedSemester = await _unitOfWork.SemesterRepository.Get(s => newEntity.SemesterCode!.Equals(s.SemesterCode)).FirstOrDefaultAsync();
                if (existedSemester is not null)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Semester code is already taken"
                    };
                }

                if (newEntity.EndDate < newEntity.StartDate)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "End date must be greater than start date"
                    };
                }

                if (!_checkSemesterPeriod.CheckValidDuration(newEntity.StartDate, newEntity.EndDate))
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = $"The duration of semester must be greater than {_checkSemesterPeriod.MinimumDuration} days and less than {_checkSemesterPeriod.MaximumDuration} days"
                    };
                }

                var newSemester = new Semester
                {
                    SemesterCode = newEntity.SemesterCode,
                    StartDate = newEntity.StartDate,
                    EndDate = newEntity.EndDate
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
                        Message = "Create new semester failed",
                        Errors = new List<string>() { "Error at create new semester service", "Can not save changes" }
                    };
                }
            }
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
