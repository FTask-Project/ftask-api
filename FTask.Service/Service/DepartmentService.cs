﻿using FTask.Repository.Data;
using FTask.Repository.Entity;
using FTask.Repository.Identity;
using FTask.Service.Utilize;
using FTask.Service.Validation;
using FTask.Service.ViewModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FTask.Service.IService
{
    internal class DepartmentService : IDepartmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICheckQuantityTaken _checkQuantityTaken;
        private readonly IDistributedCache _distributedCache;
        public DepartmentService(IUnitOfWork unitOfWork, ICheckQuantityTaken checkQuantityTaken, IDistributedCache distributedCache)
        {
            _unitOfWork = unitOfWork;
            _checkQuantityTaken = checkQuantityTaken;
            _distributedCache = distributedCache;
        }

        public async Task<Department?> GetDepartmentById(int id)
        {
            string key = $"department-{id}";
            string? cache = await _distributedCache.GetStringAsync(key);

            Department? department;
            if (string.IsNullOrEmpty(cache))
            {
                department = await _unitOfWork.DepartmentRepository.FindAsync(id);
                if(department is null)
                {
                    return department;
                }

                await _distributedCache.SetStringAsync(
                    key, 
                    JsonConvert.SerializeObject(department));

                return department;
            }

            department = JsonConvert.DeserializeObject<Department>(cache,
                new JsonSerializerSettings
                {
                    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                    ContractResolver = new PrivateResolver()
                });

            return department;
        }

        public async Task<IEnumerable<Department>> GetDepartments(int page, int quantity)
        {
            if(page == 0)
            {
                page = 1;
            }
            quantity = _checkQuantityTaken.check(quantity);
            return await _unitOfWork.DepartmentRepository.FindAll().Skip((page - 1) * _checkQuantityTaken.PageQuantity).Take(quantity).ToArrayAsync();
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
                    Message = "Some error happend",
                    Errors = new List<string>() { ex.Message }
                };
            }
        }
    }
}
