using AutoMapper;
using FTask.Service.IService;
using FTask.Service.ViewModel.RequestVM.Semester;
using FTask.Service.ViewModel.ResposneVM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FTask.API.Controllers
{
    [Route("api/semesters")]
    [ApiController]
    public class SemesterController : ControllerBase
    {
        private readonly ISemesterService _semesterService;
        private readonly IMapper _mapper;

        public SemesterController(
            ISemesterService semesterService,
            IMapper mapper
            )
        {
            _mapper = mapper;
            _semesterService = semesterService;
        }

        [HttpGet("{id}", Name = nameof(GetSemesterById))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SemesterResponseVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponseVM))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> GetSemesterById(int id)
        {
            if (ModelState.IsValid)
            {
                var semesterResult = await _semesterService.GetSemesterById(id);
                if (semesterResult is null)
                {
                    return NotFound("Not found");
                }
                return Ok(_mapper.Map<SemesterResponseVM>(semesterResult));
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<SemesterResponseVM>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponseVM))]
        public async Task<IActionResult> GetSemester([FromQuery] int page, [FromQuery] int quantity, [FromQuery] string? filter)
        {
            if (ModelState.IsValid)
            {
                var semesterList = await _semesterService.GetSemesters(page, quantity, filter ?? "");
                return Ok(_mapper.Map<IEnumerable<SemesterResponseVM>>(semesterList));
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

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SemesterResponseVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponseVM))]
        public async Task<IActionResult> CreateSemester([FromBody] SemesterVM resource)
        {
            if (ModelState.IsValid)
            {
                var result = await _semesterService.CreateNewSemester(resource);
                if (result.IsSuccess)
                {
                    return CreatedAtAction(nameof(GetSemesterById), new
                    {
                        id = result.Entity!.SemesterId
                    }, _mapper.Map<SemesterResponseVM>(result.Entity!));
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
        public async Task<IActionResult> DeleteSemester([FromQuery] int id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _semesterService.DeleteSemester(id);
                    if (result)
                    {
                        return Ok(new ServiceResponseVM
                        {
                            IsSuccess = true,
                            Message = "Delete semester successfully"
                        });
                    }
                    else
                    {
                        return BadRequest(new ServiceResponseVM
                        {
                            IsSuccess = false,
                            Message = "Failed to delete semester"
                        });
                    }
                }
                catch (DbUpdateException ex)
                {
                    return BadRequest(new ServiceResponseVM
                    {
                        IsSuccess = false,
                        Message = "Failed to delete semester",
                        Errors = new string[1] { ex.Message }
                    });
                }
                catch (OperationCanceledException)
                {
                    return BadRequest(new ServiceResponseVM
                    {
                        IsSuccess = false,
                        Message = "Failed to delete semester",
                        Errors = new string[1] { "The operation has been cancelled" }
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

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SemesterResponseVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponseVM))]
        public async Task<IActionResult> UpdateSemester([FromBody] UpdateSemesterVM resource, int id)
        {
            if (ModelState.IsValid)
            {
                var result = await _semesterService.UpdateSemester(resource, id);
                if (result.IsSuccess)
                {
                    return Ok(_mapper.Map<SemesterResponseVM>(result.Entity));
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
                    Message = "Invalid input",
                });
            }
        }
    }
}
