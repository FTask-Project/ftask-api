using AutoMapper;
using FTask.Service.IService;
using FTask.Service.ViewModel.RequestVM.Role;
using FTask.Service.ViewModel.ResposneVM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FTask.API.Controllers
{
    [Route("api/roles")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly IMapper _mapper;

        public RoleController(
            IRoleService roleService,
            IMapper mapper
            )
        {
            _roleService = roleService;
            _mapper = mapper;
        }

        [HttpGet("{id}", Name = nameof(GetRoleById))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RoleResponseVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponseVM))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> GetRoleById(Guid id)
        {
            if (ModelState.IsValid)
            {
                var roleResult = await _roleService.GetRoleById(id);
                if (roleResult is null)
                {
                    return NotFound("Not Found");
                }
                return Ok(_mapper.Map<RoleResponseVM>(roleResult));
            }
            else
            {
                return BadRequest(new ServiceResponseVM
                {
                    IsSuccess = false,
                    Message = "Invalid input",
                });
            }
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<RoleResponseVM>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponseVM))]
        public async Task<IActionResult> GetRoles([FromQuery] int page, [FromQuery] int quantity, [FromQuery] string? filter)
        {
            if (ModelState.IsValid)
            {
                var roleList = await _roleService.GetRoles(page, quantity, filter ?? "");
                return Ok(_mapper.Map<IEnumerable<RoleResponseVM>>(roleList));
            }
            else
            {
                return BadRequest(new ServiceResponseVM
                {
                    IsSuccess = false,
                    Message = "Invalid input"
                });
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(RoleResponseVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponseVM))]
        public async Task<IActionResult> CreateRole([FromBody] RoleVM resource)
        {
            if (ModelState.IsValid)
            {
                var result = await _roleService.CreateNewRole(resource);
                if (result.IsSuccess)
                {
                    return CreatedAtAction(nameof(GetRoleById), new
                    {
                        id = result.Entity!.Id,
                    }, _mapper.Map<RoleResponseVM>(result.Entity!));
                }
                else
                {
                    return BadRequest(_mapper.Map<ServiceResponseVM>(result));
                }
            }
            else
            {
                return BadRequest(new ServiceResponseVM
                {
                    IsSuccess = false,
                    Message = "Invalid input"
                });
            }
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ServiceResponseVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponseVM))]
        public async Task<IActionResult> DeleteRole([FromQuery] Guid id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _roleService.DeleteRole(id);
                    if (result)
                    {
                        return Ok(new ServiceResponseVM
                        {
                            IsSuccess = true,
                            Message = "Delete role successfully"
                        });
                    }
                    else
                    {
                        return BadRequest(new ServiceResponseVM
                        {
                            IsSuccess = false,
                            Message = "Failed to delete role"
                        });
                    }
                }
                catch (DbUpdateException ex)
                {
                    return BadRequest(new ServiceResponseVM
                    {
                        IsSuccess = false,
                        Message = "Failed to delete role",
                        Errors = new string[1] { ex.Message }
                    });
                }
            }
            else
            {
                return BadRequest(new ServiceResponseVM
                {
                    IsSuccess = false,
                    Message = "Invalid input"
                });
            }
        }
    }
}
