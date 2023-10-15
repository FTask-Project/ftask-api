using FTask.Repository.Data;
using FTask.Repository.Identity;
using FTask.Service.Caching;
using FTask.Service.Validation;
using FTask.Service.ViewModel;
using FTask.Service.ViewModel.RequestVM.CreateRole;
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
        public RoleService(
            ICheckQuantityTaken checkQuantityTaken,
            ICacheService<Role> cacheService,
            RoleManager<Role> roleManager,
            IUnitOfWork unitOfWork
            )
        {
            _checkQuantityTaken = checkQuantityTaken;
            _cacheService = cacheService;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
        }
        public async Task<IEnumerable<Role>> GetRolesByName(IEnumerable<string> roleNames)
        {
            return await _roleManager.Roles.Where(r => roleNames.Contains(r.Name)).ToArrayAsync();
        }

        public async Task<Role?> GetRoleById(Guid id)
        {
            string key = CacheKeyGenerator.GetKeyById(nameof(Role), id.ToString());
            var cachedData = await _cacheService.GetAsync(key);

            if (cachedData is null)
            {
                var role = await _roleManager.FindByIdAsync(id.ToString());
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
                    .Where(r => r.Name.Contains(filter))
                    .Skip((page - 1) * _checkQuantityTaken.PageQuantity)
                    .Take(quantity);
            return await roleList.ToArrayAsync();
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
