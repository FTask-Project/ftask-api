using AutoMapper;
using FTask.API.Service;
using FTask.Service.IService;
using FTask.Service.ViewModel;
using FTask.Service.ViewModel.RequestVM;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Google.Apis.Auth;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;

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
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ServiceResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
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
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = "Invalid Input",
                Errors = new List<string>() { "Invalid input" }
            });
        }

        [HttpPost("login/lecturer")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthenticateResponseVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ServiceResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
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
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = "Invalid Input",
                Errors = new List<string>() { "Invalid input" }
            });

        }

        [HttpPost("login/google/lecturer")]
        public async Task<IActionResult> LoginGoogleLecturer([FromQuery] string idToken)
        {
            // Initialize the Firebase app
            if(FirebaseApp.DefaultInstance is null)
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile("serviceAccountKey.json"),
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
            return Ok(decodedToken.ToString());
        }
    }
}
