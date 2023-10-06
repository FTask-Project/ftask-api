using FTask.Repository.Data;
using FTask.Repository.Identity;
using FTask.Service.Validation;
using FTask.Service.ViewModel;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTask.Service.IService
{
    internal class RoleService : IRoleService
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICheckQuantityTaken _checkQuantityTaken;
        public RoleService(IUnitOfWork unitOfWork, RoleManager<Role> roleManager, ICheckQuantityTaken checkQuantityTaken)
        {
            _unitOfWork = unitOfWork;
            _roleManager = roleManager;
            _checkQuantityTaken = checkQuantityTaken;
        }
        public async Task<IEnumerable<Role>> GetRolesByName(IEnumerable<string> roleNames)
        {
            return await _roleManager.Roles.Where(r => roleNames.Contains(r.Name)).ToArrayAsync();
        }

        public async Task<Role?> GetRoleById(Guid id)
        {
            return await _roleManager.FindByIdAsync(id.ToString());
        }

        public async Task<IEnumerable<Role>> GetRoles(int page, int quantity)
        {
            if (page == 0)
            {
                page = 1;
            }
            quantity = _checkQuantityTaken.check(quantity);
            return await _roleManager.Roles
                .Skip((page - 1) * _checkQuantityTaken.PageQuantity)
                .Take(quantity)
                .ToArrayAsync();
        }

        public async Task<ServiceResponse> CreateNewRole(RoleVM newEntity)
        {
            try
            {
                var existedRole = await _roleManager.FindByNameAsync(newEntity.RoleName);
                if (existedRole is not null)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = $"Role name {newEntity.RoleName} is already taken"
                    };
                }
                Role newRole = new Role
                {
                    Name = newEntity.RoleName,
                };

                var result = await _roleManager.CreateAsync(newRole);
                if (result.Succeeded)
                {
                    return new ServiceResponse
                    {
                        Id = newRole.Id.ToString(),
                        IsSuccess = true,
                        Message = "Create new roles successfully"
                    };
                }
                else
                {
                    var errors = new List<string> { "Error at create new roles service", "Can not save changes" };
                    errors.AddRange(result.Errors.Select(e => e.Description));
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Create new roles failed",
                        Errors = errors
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
