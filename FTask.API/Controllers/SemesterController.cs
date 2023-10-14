using AutoMapper;
using FTask.Service.IService;
using FTask.Service.ViewModel;
using FTask.Service.ViewModel.RequestVM.CreateSemester;
using Microsoft.AspNetCore.Mvc;

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
                return BadRequest(new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Invalid input"
                });
            }
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<SemesterResponseVM>))]
        public async Task<IActionResult> GetSemester([FromQuery] int page, [FromQuery] int quantity, [FromQuery] string? filter)
        {
            if (ModelState.IsValid)
            {
                var semesterList = await _semesterService.GetSemesters(page, quantity, filter ?? "");
                return Ok(_mapper.Map<IEnumerable<SemesterResponseVM>>(semesterList));
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

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SemesterResponseVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
        public async Task<IActionResult> CreateSemester([FromBody] SemesterVM resource)
        {
            if (ModelState.IsValid)
            {
                var result = await _semesterService.CreateNewSemester(resource);
                if (result.IsSuccess)
                {
                    var id = Int32.Parse(result.Id!);
                    var existedSemester = await _semesterService.GetSemesterById(id);
                    if (existedSemester is not null)
                    {
                        return CreatedAtAction(nameof(GetSemesterById), new
                        {
                            id = id
                        }, _mapper.Map<SemesterResponseVM>(existedSemester));
                    }
                    else
                    {
                        return BadRequest(new ServiceResponse
                        {
                            IsSuccess = false,
                            Message = "Create new semester failed",
                            Errors = new List<string> { "Error at create new semester action method", "Created semester not found" }
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
