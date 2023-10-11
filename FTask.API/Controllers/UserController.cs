using AutoMapper;
using FTask.Service.IService;
using FTask.Service.ViewModel;
using FTask.Service.ViewModel.RequestVM.CreateUser;
using Microsoft.AspNetCore.Mvc;

namespace FTask.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UserController(
            IUserService userService,
            IMapper mapper
            )
        {
            _userService = userService;
            _mapper = mapper;
        }

        [HttpGet("{userId}", Name = nameof(GetUserById))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserInformationResponseVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> GetUserById(Guid userId)
        {
            if (ModelState.IsValid)
            {
                var userResult = await _userService.GetUserById(userId);
                if (userResult is null)
                {
                    return NotFound("Not Found");
                }
                return Ok(_mapper.Map<UserInformationResponseVM>(userResult));
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

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UserInformationResponseVM>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
        public async Task<IActionResult> GetUsers([FromQuery] int page, [FromQuery] int quantity)
        {
            if (ModelState.IsValid)
            {
                var userList = await _userService.GetUsers(page, quantity);
                return Ok(_mapper.Map<IEnumerable<UserInformationResponseVM>>(userList));
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
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserInformationResponseVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
        public async Task<IActionResult> CreateNewUser([FromForm] UserVM resource)
        {
            if (ModelState.IsValid)
            {
                var result = await _userService.CreateNewUser(resource);
                if (result.IsSuccess)
                {
                    var id = Guid.Parse(result.Id!);
                    var existedUser = await _userService.GetUserById(id);
                    if (existedUser is not null)
                    {
                        return CreatedAtAction(nameof(GetUserById), new
                        {
                            userId = id
                        }, _mapper.Map<UserInformationResponseVM>(existedUser));
                    }
                    else
                    {
                        return BadRequest(new ServiceResponse
                        {
                            IsSuccess = false,
                            Message = "Create new user failed",
                            Errors = new List<string> { "Error at create new user action method", "Created user not found" }
                        });
                    }
                }
                return BadRequest(result);
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
