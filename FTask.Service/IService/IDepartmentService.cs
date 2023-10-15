﻿using FTask.Repository.Entity;
using FTask.Service.ViewModel;

namespace FTask.Service.IService
{
    public interface IDepartmentService
    {
        Task<Department?> GetDepartmentById(int id);
        Task<IEnumerable<Department>> GetDepartments(int page, int quantity, string filter, Guid? headerId);
        Task<ServiceResponse> CreateNewDepartment(Department newEntity);
    }
}
