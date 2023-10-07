using AutoMapper;
using Duende.IdentityServer.Extensions;
using FTask.Repository.Identity;
using FTask.Service.Caching;
using FTask.Service.IService;
using FTask.Service.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace FTask.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly ICacheService<Role> _cacheService;
        private readonly IRoleService _roleService;
        private readonly IMapper _mapper;

        public RoleController(
            ICacheService<Role> cacheService,
            IRoleService roleService,
            IMapper mapper
            )
        {
            _cacheService = cacheService;
            _roleService = roleService;
            _mapper = mapper;
        }

        [HttpGet("{roleId}", Name = nameof(GetRoleById))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RoleResponseVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> GetRoleById(Guid roleId)
        {
            if (ModelState.IsValid)
            {
                string key = $"role-{roleId}";
                var cacheData = await _cacheService.GetAsync(key);
                if (cacheData is null)
                {
                    var roleResult = await _roleService.GetRoleById(roleId);
                    if (roleResult is null)
                    {
                        return NotFound("Not Found");
                    }
                    await _cacheService.SetAsync(key, roleResult);
                    return Ok(_mapper.Map<RoleResponseVM>(roleResult));
                }
                return Ok(_mapper.Map<RoleResponseVM>(cacheData));
            }
            else
            {
                return BadRequest(new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Invalid input",
                });
            }
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<RoleResponseVM>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
        public async Task<IActionResult> GetRoles([FromQuery] int page, [FromQuery] int quantity)
        {
            if (ModelState.IsValid)
            {
                string key = $"role-{page}-{quantity}";
                var cacheData = await _cacheService.GetAsyncArray(key);
                if (cacheData is null)
                {
                    var roleList = await _roleService.GetRoles(page, quantity);
                    if (!roleList.IsNullOrEmpty())
                    {
                        await _cacheService.SetAsync(key, roleList);
                    }
                    return Ok(_mapper.Map<IEnumerable<RoleResponseVM>>(roleList));
                }

                return Ok(_mapper.Map<IEnumerable<RoleResponseVM>>(cacheData));
            }
            else
            {
                return BadRequest(new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Invalid input"
                });
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(RoleResponseVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
        public async Task<IActionResult> CreateRole([FromBody] RoleVM resource)
        {
            if (ModelState.IsValid)
            {
                var result = await _roleService.CreateNewRole(resource);
                if (result.IsSuccess)
                {
                    var id = Guid.Parse(result.Id!);
                    var role = await _roleService.GetRoleById(id);
                    if (role is not null)
                    {
                        return CreatedAtAction(nameof(GetRoleById), new
                        {
                            roleId = id,
                        }, _mapper.Map<RoleResponseVM>(role));
                    }
                    else
                    {
                        return BadRequest(new ServiceResponse
                        {
                            IsSuccess = false,
                            Message = "Some error happened",
                            Errors = new List<string> { "Error at create new role action method", "Created role not found" }
                        });
                    }
                }
                else
                {
                    return BadRequest(result);
                }
            }
            else
            {
                return BadRequest(new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Invalid input"
                });
            }
        }
    }
}
