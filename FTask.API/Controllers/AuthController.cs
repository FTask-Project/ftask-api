﻿using AutoMapper;
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

        [HttpPost("login/user")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthenticateResponseVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponseVM))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ServiceResponseVM))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponseVM))]
        public async Task<IActionResult> LoginUser([FromBody] LoginUserVM resource)
        {
            if (ModelState.IsValid)
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
                Message = "Login failed",
                Errors = new List<string>() { "Invalid input" }
            });
        }

        [HttpPost("login/lecturer")]
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
                Message = "Login failed",
                Errors = new List<string>() { "Invalid input" }
            });

        }

        [HttpPost("login/google/lecturer")]
        public async Task<IActionResult> LoginGoogleLecturer([FromQuery] string idToken)
        {
            /*// Initialize the Firebase app
            if (FirebaseApp.DefaultInstance is null)
            {
                FirebaseApp.Create(new AppOptions()
                {
                    //Credential = GoogleCredential.FromFile("serviceAccountKey.json"),
                });
            }

            // Verify the ID token
            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);

            // Get user data
            var uid = decodedToken.Uid;
            var name = decodedToken.Claims["name"].ToString();
            var email = decodedToken.Claims["email"].ToString();
            var pictureUrl = decodedToken.Claims["picture"].ToString();

            // Use the user data
            return Ok(decodedToken.ToString());*/

            try
            {
                // Verify the ID token using Google's libraries
                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);

                // Extract user data from the payload
                var uid = payload.Subject;
                var displayName = payload.Name;
                var email = payload.Email;

                // Handle user data (e.g., store in database)
                // ...

                // Return a response, such as an access token
                return Ok(new { Message = "User data handled successfully" });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error handling user data: {ex}");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }
    }
}
