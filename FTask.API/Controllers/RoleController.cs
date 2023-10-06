using AutoMapper;
using FTask.Service.IService;
using FTask.Service.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FTask.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly IMapper _mapper;

        public RoleController(IRoleService roleService, IMapper mapper)
        {
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
                var result = await _roleService.GetRoleById(roleId);
                if(result is null)
                {
                    return NotFound("Not found");
                }
                return Ok(_mapper.Map<RoleResponseVM>(result));
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
                var result = await _roleService.GetRoles(page, quantity);
                return Ok(_mapper.Map<IEnumerable<RoleResponseVM>>(result));
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
        public async Task<IActionResult> CreateRole([FromBody]RoleVM resource)
        {
            if (ModelState.IsValid)
            {
                var result = await _roleService.CreateNewRole(resource);
                if (result.IsSuccess)
                {
                    var id = Guid.Parse(result.Id!);
                    var role = await _roleService.GetRoleById(id);
                    if(role is not null)
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
