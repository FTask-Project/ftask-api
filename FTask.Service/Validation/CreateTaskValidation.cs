using FTask.Repository.Common;
using FTask.Repository.Entity;
using FTask.Repository.IRepository;
using FTask.Service.ViewModel.ResposneVM;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Task = FTask.Repository.Entity.Task;

namespace FTask.Service.Validation
{
    public interface ICreateTaskValidation
    {
        Task<ServiceResponse<Task>> CanAssignTask(IEnumerable<Guid> taskRecipients, IDepartmentRepository departmentRepository);
    }

    internal class CreateTaskValidation : ICreateTaskValidation
    {
        private static readonly string[] ALLOWED_ROLES = new string[2] { "Manager", "Admin" };
        private static readonly string[] OTHER_ROLES = new string[1] { "Lecturer" };
        private static readonly bool HEADER_CAN_ASSIGN_TASK = true;
        private static readonly int MAXIMUM_ASSIGN = 4;

        private readonly ICurrentUserService _currentUserService;

        public CreateTaskValidation(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        public async Task<ServiceResponse<Task>> CanAssignTask(IEnumerable<Guid> taskRecipients, IDepartmentRepository departmentRepository)
        {
            var check = Guid.TryParse(_currentUserService.UserId, out var currentUserId);
            if (!check)
            {
                return new ServiceResponse<Task>
                {
                    IsSuccess = false,
                    Message = "Failed to create new task",
                    Errors = new string[1] { "Unauthenticated user" }
                };
            }

            var roles = _currentUserService.Roles;
            if (roles.Any(r => ALLOWED_ROLES.Contains(r)))
            {
                return new ServiceResponse<Task>
                {
                    IsSuccess = true
                };
            }

            if (!HEADER_CAN_ASSIGN_TASK || !roles.Any(r => OTHER_ROLES.Contains(r)))
            {
                return new ServiceResponse<Task>
                {
                    IsSuccess = false,
                    Message = "Failed to create new task",
                    Errors = new string[1] { "You don't have sufficient privileges" }
                };
            }

            var department = await departmentRepository
                .Get(d => d.DepartmentHeadId == currentUserId,
                new Expression<Func<Department, object>>[1]
                {
                    d => d.Lecturers
                }).FirstOrDefaultAsync();
            if (department is null)
            {
                return new ServiceResponse<Task>
                {
                    IsSuccess = false,
                    Message = "Failed to create new task",
                    Errors = new string[1] { "You don't have sufficient privileges" }
                };
            }

            if (taskRecipients.Count() > MAXIMUM_ASSIGN)
            {
                return new ServiceResponse<Task>
                {
                    IsSuccess = false,
                    Message = "Failed to create new task",
                    Errors = new string[1] { $"You can only assign a maximum to {MAXIMUM_ASSIGN} lecturers" }
                };
            }

            var lecturerIds = department.Lecturers.Select(l => l.Id);
            foreach (var lecturer in taskRecipients)
            {
                if (!lecturerIds.Contains(lecturer))
                {
                    return new ServiceResponse<Task>
                    {
                        IsSuccess = false,
                        Message = "Failed to create new task",
                        Errors = new string[1] { "You are not allowed to assign task for lecturers who are not belong to your department" }
                    };
                }
            }

            return new ServiceResponse<Task>
            {
                IsSuccess = true
            };
        }
    }
}
