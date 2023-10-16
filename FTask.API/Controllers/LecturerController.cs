using AutoMapper;
using FTask.Service.IService;
using FTask.Service.ViewModel.RequestVM.CreateLecturer;
using FTask.Service.ViewModel.ResposneVM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FTask.API.Controllers
{
    [Route("api/lecturers")]
    [ApiController]
    public class LecturerController : ControllerBase
    {
        private readonly ILecturerService _lecturerService;
        private readonly IMapper _mapper;

        public LecturerController(
            ILecturerService lecturerService,
            IMapper mapper
            )
        {
            _lecturerService = lecturerService;
            _mapper = mapper;
        }

        [HttpGet("{id}", Name = nameof(GetLecturerById))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserInformationResponseVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponseVM))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> GetLecturerById(Guid id)
        {
            if (ModelState.IsValid)
            {
                var lecturerResult = await _lecturerService.GetLectureById(id);
                if (lecturerResult is null)
                {
                    return NotFound("Not found");
                }
                return Ok(_mapper.Map<UserInformationResponseVM>(lecturerResult));
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

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UserInformationResponseVM>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponseVM))]
        public async Task<IActionResult> GetLecturers([FromQuery] int page, [FromQuery] int quantity, [FromQuery] string? filter, [FromQuery] int? departmentId, [FromQuery] int? subjectId)
        {
            if (ModelState.IsValid)
            {
                var lecturerList = await _lecturerService.GetLecturers(page, quantity, filter ?? "", departmentId, subjectId);
                return Ok(_mapper.Map<IEnumerable<UserInformationResponseVM>>(lecturerList));
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
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserInformationResponseVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponseVM))]
        public async Task<IActionResult> CreateNewLecturer([FromForm] LecturerVM resource)
        {
            if (ModelState.IsValid)
            {
                var result = await _lecturerService.CreateNewLecturer(resource);
                if (result.IsSuccess)
                {
                    var id = Guid.Parse(result.Id!);
                    var existedLecturer = await _lecturerService.GetLectureById(id);
                    if (existedLecturer is not null)
                    {
                        return CreatedAtAction(nameof(GetLecturerById), new
                        {
                            id = id
                        }, _mapper.Map<UserInformationResponseVM>(existedLecturer));
                    }
                    else
                    {
                        return BadRequest(new ServiceResponseVM
                        {
                            IsSuccess = false,
                            Message = "Failed to create new lecturer",
                            Errors = new List<string> { "Created lecturer not found" }
                        });
                    }
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
        public async Task<IActionResult> DeleteLecturer([FromQuery] Guid id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _lecturerService.DeleteLecturer(id);
                    if (result)
                    {
                        return Ok(new ServiceResponseVM
                        {
                            IsSuccess = true,
                            Message = "Delete lecturer successfully"
                        });
                    }
                    else
                    {
                        return BadRequest(new ServiceResponseVM
                        {
                            IsSuccess = false,
                            Message = "Failed to delete lecturer"
                        });
                    }
                }
                catch (DbUpdateException ex)
                {
                    return BadRequest(new ServiceResponseVM
                    {
                        IsSuccess = false,
                        Message = "Failed to delete lecturer",
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
