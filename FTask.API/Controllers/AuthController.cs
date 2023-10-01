using AutoMapper;
using FTask.API.Common;
using FTask.API.Service;
using FTask.Service.IService;
using FTask.Service.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections;

namespace FTask.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJWTTokenService<IdentityUser<Guid>> _jwtTokenService;
        private readonly IMapper _mapper;

        public AuthController(IUserService userService, IJWTTokenService<IdentityUser<Guid>> jwtTokenService, IMapper mapper)
        {
            _userService = userService;
            _jwtTokenService = jwtTokenService;
            _mapper = mapper;
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthenticateResponseVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ServiceResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
        public async Task<IActionResult> Login([FromBody] LoginUserVM resource)
        {
            if (ModelState.IsValid)
            {
                if (resource.IsLecturer)
                {
                    var result = await _userService.LoginLecturer(resource);
                    if (result.IsSuccess)
                    {
                        var tokenString = _jwtTokenService.CreateToken(result.LoginUser!, new string[1] { "Lecturer" });
                        if (tokenString is not null)
                        {
                            return Ok(new AuthenticateResponseVM
                            {
                                Token = tokenString,
                                UserInformation = _mapper.Map<UserInformationResponseVM>(result.LoginUser!)
                            });
                        }
                        else
                        {
                            return StatusCode(500, new ServiceResponse
                            {
                                IsSuccess = false,
                                Message = "Some errors happened",
                                Errors = new List<string>() { "Can not create token" }
                            });
                        }
                    }
                    else
                    {
                        return Unauthorized(new ServiceResponse
                        {
                            IsSuccess = false,
                            Message = result.Message
                        });
                    }
                }
                else if (resource.IsMember)
                {
                    var result = await _userService.LoginMember(resource);
                    if (result.IsSuccess)
                    {
                        var tokenString = _jwtTokenService.CreateToken(result.LoginUser!, result.RoleNames!);
                        if (tokenString is not null)
                        {
                            return Ok(new AuthenticateResponseVM
                            {
                                Token = tokenString,
                                UserInformation = _mapper.Map<UserInformationResponseVM>(result.LoginUser!)
                            });
                        }
                        else
                        {
                            return StatusCode(500, new ServiceResponse
                            {
                                IsSuccess = false,
                                Message = "Some errors happened",
                                Errors = new List<string>() { "Can not create token" }
                            });
                        }
                    }
                    else
                    {
                        return Unauthorized(new ServiceResponse
                        {
                            IsSuccess = false,
                            Message = result.Message
                        });
                    }
                }
            }
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = "Invalid Input",
                Errors = new List<string>() { "Invalid input" }
            });

        }
    }
}
