﻿using AutoMapper;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using FTask.API.Service;
using FTask.Service.IService;
using FTask.Service.ViewModel.RequestVM;
using FTask.Service.ViewModel.ResposneVM;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FTask.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILecturerService _lecturerService;
        private readonly IJWTTokenService<IdentityUser<Guid>> _jwtTokenService;
        private readonly IMapper _mapper;

        public AuthController(IUserService userService, IJWTTokenService<IdentityUser<Guid>> jwtTokenService, IMapper mapper, ILecturerService lecturerService)
        {
            _userService = userService;
            _jwtTokenService = jwtTokenService;
            _mapper = mapper;
            _lecturerService = lecturerService;
        }

        [HttpPost("login-user")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthenticateResponseVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponseVM))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ServiceResponseVM))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponseVM))]
        public async Task<IActionResult> LoginUser([FromBody] LoginUserVM resource)
        {
            if (ModelState.IsValid)
            {
                var result = await _userService.LoginUser(resource);
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
                        return StatusCode(500, new ServiceResponseVM
                        {
                            IsSuccess = false,
                            Message = "Login failed",
                            Errors = new List<string>() { "Can not create token" }
                        });
                    }
                }
                else
                {
                    return Unauthorized(new ServiceResponseVM
                    {
                        IsSuccess = false,
                        Message = result.Message
                    });
                }
            }
            return BadRequest(new ServiceResponseVM
            {
                IsSuccess = false,
                Message = "Invalid input",
            });
        }

        [HttpPost("login-lecturer")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthenticateResponseVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponseVM))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ServiceResponseVM))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponseVM))]
        public async Task<IActionResult> LoginLecturer([FromBody] LoginUserVM resource)
        {
            if (ModelState.IsValid)
            {
                var result = await _lecturerService.LoginLecturer(resource);
                if (result.IsSuccess)
                {
                    var tokenString = _jwtTokenService.CreateToken(result.LoginUser!, new string[1] { "Lecturer" });
                    if (tokenString is not null)
                    {
                        return Ok(new AuthenticateResponseVM
                        {
                            Token = tokenString,
                            LecturerInformation = _mapper.Map<LecturerInformationResponseVM>(result.LoginUser!)
                        });
                    }
                    else
                    {
                        return StatusCode(500, new ServiceResponseVM
                        {
                            IsSuccess = false,
                            Message = "Login failed",
                            Errors = new List<string>() { "Can not create token" }
                        });
                    }
                }
                else
                {
                    return Unauthorized(new ServiceResponseVM
                    {
                        IsSuccess = false,
                        Message = result.Message
                    });
                }
            }
            return BadRequest(new ServiceResponseVM
            {
                IsSuccess = false,
                Message = "Invalid input",
            });

        }

        [HttpPost("login-google-lecturer")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthenticateResponseVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponseVM))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ServiceResponseVM))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponseVM))]
        public async Task<IActionResult> LoginLecturerWithGoogle([FromQuery] string idToken, [FromQuery] bool fromMobile, [FromQuery] string? deviceToken)
        {
            if (ModelState.IsValid)
            {
                var result = await _lecturerService.LoginWithGoogle(idToken, fromMobile, deviceToken);
                if (result.IsSuccess)
                {
                    var tokenString = _jwtTokenService.CreateToken(result.LoginUser!, new string[1] { "Lecturer" });
                    if (tokenString is not null)
                    {
                        return Ok(new AuthenticateResponseVM
                        {
                            Token = tokenString,
                            LecturerInformation = _mapper.Map<LecturerInformationResponseVM>(result.LoginUser!)
                        });
                    }
                    else
                    {
                        return StatusCode(500, new ServiceResponseVM
                        {
                            IsSuccess = false,
                            Message = "Login failed",
                            Errors = new List<string>() { "Can not create token" }
                        });
                    }
                }
                else
                {
                    return Unauthorized(new ServiceResponseVM
                    {
                        IsSuccess = false,
                        Message = result.Message
                    });
                }
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

        [HttpPost("login-google-user")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthenticateResponseVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponseVM))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ServiceResponseVM))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponseVM))]
        public async Task<IActionResult> LoginUserWithGoogle([FromQuery] string idToken)
        {
            if (ModelState.IsValid)
            {
                var result = await _userService.LoginWithGoogle(idToken);
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
                        return StatusCode(500, new ServiceResponseVM
                        {
                            IsSuccess = false,
                            Message = "Login failed",
                            Errors = new List<string>() { "Can not create token" }
                        });
                    }
                }
                else
                {
                    return Unauthorized(new ServiceResponseVM
                    {
                        IsSuccess = false,
                        Message = result.Message
                    });
                }
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
    }
}
