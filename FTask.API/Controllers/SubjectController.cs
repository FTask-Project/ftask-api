using AutoMapper;
using FTask.Repository.Entity;
using FTask.Service.IService;
using FTask.Service.ViewModel.RequestVM.Subject;
using FTask.Service.ViewModel.ResposneVM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FTask.API.Controllers
{
    [Route("api/subjects")]
    [ApiController]
    public class SubjectController : ControllerBase
    {
        private readonly ISubjectService _subjectService;
        private readonly IMapper _mapper;

        public SubjectController(
            ISubjectService subjectService,
            IMapper mapper
            )
        {
            _subjectService = subjectService;
            _mapper = mapper;
        }

        [HttpGet("{id}", Name = nameof(GetSubjectById))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SubjectResponseVM))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponseVM))]
        public async Task<IActionResult> GetSubjectById(int id)
        {
            if (ModelState.IsValid)
            {
                var subjectResult = await _subjectService.GetSubjectById(id);
                if (subjectResult is null)
                {
                    return NotFound("Not found");
                }
                return Ok(_mapper.Map<SubjectResponseVM>(subjectResult));
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<SubjectResponseVM>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponseVM))]
        public async Task<IActionResult> GetSubjects(
            [FromQuery] int page,
            [FromQuery] int amount,
            [FromQuery] string? filter,
            [FromQuery] int? departmentId,
            [FromQuery] bool? status)
        {
            if (ModelState.IsValid)
            {
                var subjectList = await _subjectService.GetSubjectAllSubject(page, amount, filter ?? "", departmentId, status);
                return Ok(_mapper.Map<IEnumerable<SubjectResponseVM>>(subjectList));
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
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SubjectResponseVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponseVM))]
        public async Task<IActionResult> CreateSubject([FromBody] CreateSubjectVM subject)
        {
            if (ModelState.IsValid)
            {
                var result = await _subjectService.CreateNewSubject(_mapper.Map<Subject>(subject));
                if (result.IsSuccess)
                {
                    return CreatedAtAction(nameof(GetSubjectById),
                            new
                            {
                                id = result.Entity!.SubjectId
                            },
                            _mapper.Map<SubjectResponseVM>(result.Entity!)
                        );
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
        public async Task<IActionResult> DeleteSubject([FromQuery] int id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _subjectService.DeleteSubject(id);
                    if (result)
                    {
                        return Ok(new ServiceResponseVM
                        {
                            IsSuccess = true,
                            Message = "Delete subject successfully"
                        });
                    }
                    else
                    {
                        return BadRequest(new ServiceResponseVM
                        {
                            IsSuccess = false,
                            Message = "Failed to delete subject"
                        });
                    }
                }
                catch (DbUpdateException ex)
                {
                    return BadRequest(new ServiceResponseVM
                    {
                        IsSuccess = false,
                        Message = "Failed to delete subject",
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

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SubjectResponseVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponseVM))]
        public async Task<IActionResult> UpdateSubject([FromBody] UpdateSubjectVM resource, int id)
        {
            if (ModelState.IsValid)
            {
                var result = await _subjectService.UpdateSubject(resource, id);
                if (result.IsSuccess)
                {
                    return Ok(_mapper.Map<SubjectResponseVM>(result.Entity));
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
    }
}
