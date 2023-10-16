﻿using FTask.Repository.Common;
using FTask.Repository.Data;
using FTask.Repository.Entity;
using FTask.Service.Caching;
using FTask.Service.Validation;
using FTask.Service.ViewModel.ResposneVM;
using Microsoft.EntityFrameworkCore;

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
                var subject = await _unitOfWork.SubjectRepository.Get(s => !s.Deleted && s.SubjectId == id).FirstOrDefaultAsync();
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
                    .Get(s => !s.Deleted && (s.SubjectName.Contains(filter) || s.SubjectCode.Contains(filter)))
                    .Skip((page - 1) * _checkQuantityTaken.PageQuantity)
                    .Take(quantity)
                    .ToArrayAsync();

            if (departmentId is not null)
            {
                subjectList = subjectList.Where(s => s.DepartmentId == departmentId).ToArray();
            }

            return subjectList;
        }

        public async Task<ServiceResponse> CreateNewSubject(Subject subjectEntity)
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
                    Message = "Failed to create new subject",
                    Errors = new string[1] { "Subject code or name already exist" }
                };
            }

            var existedDepartment = await _unitOfWork.DepartmentRepository.FindAsync(subjectEntity.DepartmentId);
            if (existedDepartment is null)
            {
                return new ServiceResponse
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
                    return new ServiceResponse
                    {
                        IsSuccess = true,
                        Message = "Create new subject successfully",
                        Id = subjectEntity.SubjectId.ToString()
                    };
                }
                else
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Failed to create new subject",
                        Errors = new string[1] {"Can not save changes"}
                    };
                }
            }
            catch (DbUpdateException ex)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Failed to create new subject",
                    Errors = new List<string>() { ex.Message }
                };
            }
        }

        public async Task<bool> DeleteSubject(int id)
        {
            var existedSubject = await _unitOfWork.SubjectRepository.Get(s => !s.Deleted && s.SubjectId == id).FirstOrDefaultAsync();
            if(existedSubject is null)
            {
                return false;
            }
            _unitOfWork.SubjectRepository.Remove(existedSubject);
            var result = await _unitOfWork.SaveChangesAsync();

            if (result)
            {
                string key = CacheKeyGenerator.GetKeyById(nameof(Subject), id.ToString());
                await _cacheService.RemoveAsync(key);
            }

            return result;
        }
    }
}
