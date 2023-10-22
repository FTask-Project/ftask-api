using FTask.Repository.Common;
using FTask.Repository.Data;
using FTask.Repository.Identity;
using FTask.Service.Caching;
using FTask.Service.Validation;
using FTask.Service.ViewModel.RequestVM.Role;
using FTask.Service.ViewModel.ResposneVM;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FTask.Service.IService
{
    internal class RoleService : IRoleService
    {
        private readonly ICheckQuantityTaken _checkQuantityTaken;
        private readonly ICacheService<Role> _cacheService;
        private readonly RoleManager<Role> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        public RoleService(
            ICheckQuantityTaken checkQuantityTaken,
            ICacheService<Role> cacheService,
            RoleManager<Role> roleManager,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService
            )
        {
            _checkQuantityTaken = checkQuantityTaken;
            _cacheService = cacheService;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }
        public async Task<IEnumerable<Role>> GetRolesByName(IEnumerable<string> roleNames)
        {
            return await _roleManager.Roles.Where(r => !r.Deleted && roleNames.Contains(r.Name)).ToArrayAsync();
        }

        public async Task<Role?> GetRoleById(Guid id)
        {
            string key = CacheKeyGenerator.GetKeyById(nameof(Role), id.ToString());
            var cachedData = await _cacheService.GetAsync(key);

            if (cachedData is null)
            {
                var role = await _unitOfWork.RoleRepository.Get(r => !r.Deleted && r.Id == id).FirstOrDefaultAsync();
                if (role is not null)
                {
                    await _cacheService.SetAsync(key, role);
                }
                return role;
            }

            return cachedData;
        }

        public async Task<IEnumerable<Role>> GetRoles(int page, int quantity, string filter)
        {
            if (page == 0)
            {
                page = 1;
            }
            quantity = _checkQuantityTaken.check(quantity);

            var roleList = _roleManager.Roles
                    .Where(r => !r.Deleted && r.Name.Contains(filter))
                    .Skip((page - 1) * _checkQuantityTaken.PageQuantity)
                    .Take(quantity);
            return await roleList.ToArrayAsync();
        }

        public async Task<ServiceResponse<Role>> CreateNewRole(RoleVM newEntity)
        {
            var existedRole = await _roleManager.FindByNameAsync(newEntity.RoleName);
            if (existedRole is not null)
            {
                return new ServiceResponse<Role>
                {
                    IsSuccess = false,
                    Message = "Failed to create new role",
                    Errors = new string[1] { $"Role name {newEntity.RoleName} is already taken" }
                };
            }
            Role newRole = new Role
            {
                Name = newEntity.RoleName,
                CreatedBy = _currentUserService.UserId,
                CreatedAt = DateTime.Now
            };

            try
            {
                var result = await _roleManager.CreateAsync(newRole);
                if (result.Succeeded)
                {
                    return new ServiceResponse<Role>
                    {
                        Entity = newRole,
                        IsSuccess = true,
                        Message = "Create new role successfully"
                    };
                }
                else
                {
                    var errors = new List<string> { "Error at create new roles service", "Can not save changes" };
                    errors.AddRange(result.Errors.Select(e => e.Description));
                    return new ServiceResponse<Role>
                    {
                        IsSuccess = false,
                        Message = "Failed to create new role",
                        Errors = errors
                    };
                }
            }
            catch (DbUpdateException ex)
            {
                return new ServiceResponse<Role>
                {
                    IsSuccess = false,
                    Message = "Failed to create new role",
                    Errors = new List<string>() { ex.Message }
                };
            }
            catch (OperationCanceledException)
            {
                return new ServiceResponse<Role>
                {
                    IsSuccess = false,
                    Message = "Failed to create new role",
                    Errors = new string[1] { "The operation has been cancelled" }
                };
            }
        }

        public async Task<bool> DeleteRole(Guid id)
        {
            var existedRole = await _unitOfWork.RoleRepository.Get(r => !r.Deleted && r.Id == id).FirstOrDefaultAsync();
            if(existedRole is null)
            {
                return false;
            }
            _unitOfWork.RoleRepository.Remove(existedRole);
            var result = await _unitOfWork.SaveChangesAsync();

            if (result)
            {
                string key = CacheKeyGenerator.GetKeyById(nameof(Role), id.ToString());
                await _cacheService.RemoveAsync(key);
            }

            return result;
        }

        public async Task<ServiceResponse<Role>> UpdateRole(UpdateRoleVM updateRole, Guid id)
        {
            var existedRole = await _unitOfWork.RoleRepository.Get(r => !r.Deleted && r.Id == id).FirstOrDefaultAsync();
            if(existedRole is null)
            {
                return new ServiceResponse<Role>
                {
                    IsSuccess = false,
                    Message = "Failed to update role",
                    Errors = new string[] { "Role not found" }
                };
            }

            if(updateRole.RoleName is not null)
            {
                var checkRoleName = _unitOfWork.RoleRepository.Get(r => r.Name.Equals(updateRole.RoleName)).FirstOrDefault() is not null;
                if (checkRoleName)
                {
                    return new ServiceResponse<Role>
                    {
                        IsSuccess = false,
                        Message = "Failed to update role",
                        Errors = new string[] { "Role name is already taken" }
                    };
                }
                existedRole.Name = updateRole.RoleName;
            }

            try
            {
                var result = await _unitOfWork.SaveChangesAsync();
                if (result)
                {
                    string key = CacheKeyGenerator.GetKeyById(nameof(Role), existedRole.Id.ToString());
                    var task = _cacheService.RemoveAsync(key);

                    return new ServiceResponse<Role>
                    {
                        Entity = existedRole,
                        IsSuccess = true,
                        Message = "Update role successfully"
                    };
                }
                else
                {
                    return new ServiceResponse<Role>
                    {
                        IsSuccess = false,
                        Message = "Failed to update role",
                        Errors = new string[] {"Can not save changes"}
                    };
                }
            }
            catch (DbUpdateException ex)
            {
                return new ServiceResponse<Role>
                {
                    IsSuccess = false,
                    Message = "Failed to update role",
                    Errors = new List<string>() { ex.Message }
                };
            }
            catch (OperationCanceledException)
            {
                return new ServiceResponse<Role>
                {
                    IsSuccess = false,
                    Message = "Failed to update role",
                    Errors = new string[1] { "The operation has been cancelled" }
                };
            }
        }
    }
}
