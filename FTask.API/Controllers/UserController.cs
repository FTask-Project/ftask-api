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
    public class UserController : ControllerBase
    {
        private readonly ICacheService<User> _cacheService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UserController (
            ICacheService<User> cacheService,
            IUserService userService,
            IMapper mapper
            )
        {
            _cacheService = cacheService;
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
                string key = $"user-{userId}";
                var cacheData = await _cacheService.GetAsync(key);
                if (cacheData is null)
                {
                    var userResult = await _userService.GetUserById(userId);
                    if (userResult is null)
                    {
                        return NotFound("Not Found");
                    }
                    await _cacheService.SetAsync(key, userResult);
                    return Ok(_mapper.Map<UserInformationResponseVM>(userResult));
                }

                return Ok(_mapper.Map<UserInformationResponseVM>(cacheData));
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
                string key = $"user-{page}-{quantity}";
                var cacheData = await _cacheService.GetAsyncArray(key);
                if (cacheData is null)
                {
                    var userList = await _userService.GetUsers(page, quantity);
                    if (!userList.IsNullOrEmpty())
                    {
                        await _cacheService.SetAsync(key, userList);
                    }
                    return Ok(_mapper.Map<IEnumerable<UserInformationResponseVM>>(userList));
                }
                return Ok(_mapper.Map<IEnumerable<UserInformationResponseVM>>(cacheData));
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
                            Message = "Some error happened",
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
