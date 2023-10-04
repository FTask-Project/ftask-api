using FTask.Repository.Data;
using FTask.Repository.Identity;
using FTask.Service.Validation;
using FTask.Service.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTask.Service.IService
{
    internal class LecturerService : ILecturerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICheckQuantityTaken _checkQuantityTaken;
        private readonly UserManager<Lecturer> _userManager;
        public LecturerService(IUnitOfWork unitOfWork, ICheckQuantityTaken checkQuantityTaken, UserManager<Lecturer> userManager)
        {
            _unitOfWork = unitOfWork;
            _checkQuantityTaken = checkQuantityTaken;
            _userManager = userManager;
        }

        public async Task<IEnumerable<Lecturer>> GetLecturers(int page, int quantity)
        {
            if(page == 0)
            {
                page = 1;
            }
            quantity = _checkQuantityTaken.check(quantity);
            return await _unitOfWork.LecturerRepository
                .FindAll()
                .Skip((page - 1) * quantity)
                .Take(quantity).ToArrayAsync();
        }

        public async Task<Lecturer?> GetLectureById(Guid id)
        {
            return await _unitOfWork.LecturerRepository.FindAsync(id);
        }

        /*public async Task<ServiceResponse> CreateNewLecturer(Lecturer newEntity, string password, IEnumerable<int> subjectIds)
        {
            var existedLecturer = await _unitOfWork.LecturerRepository.Get(l => l.Email == newEntity.Email).FirstOrDefaultAsync();
            if (existedLecturer is not null)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Email already exist"
                };
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

            var result = await _userManager.CreateAsync(newEntity, password);
            if (!result.Succeeded)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Created new lecturer failed",
                    Errors = new List<string>() { }
                };
            }

            if (subjectIds.Count() > 0)
            {
                foreach(int id in subjectIds)
                {
                    var existedSubject = _unitOfWork.
                }
            }
        }*/
    }
}
