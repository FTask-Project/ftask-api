using AutoMapper;
using FTask.Repository.Entity;
using FTask.Service.IService;
using FTask.Service.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FTask.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SemesterController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ISemesterService _semesterService;

        public SemesterController(IMapper mapper, ISemesterService semesterService)
        {
            _mapper = mapper;
            _semesterService = semesterService;
        }

        [HttpGet("{semesterId}",Name = nameof(GetSemesterById))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SemesterResponseVM))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> GetSemesterById(int semesterId)
        {
            if (ModelState.IsValid)
            {
                var result = await _semesterService.GetSemesterById(semesterId);
                if(result is null)
                {
                    return NotFound("Not found");
                }
                return Ok(_mapper.Map<SemesterResponseVM>(result));
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
        public async Task<IActionResult> GetSemester([FromQuery]int page, [FromQuery]int quantity)
        {
            var result = await _semesterService.GetSemesters(page,quantity);
            return Ok(_mapper.Map<IEnumerable<SemesterResponseVM>>(result));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SemesterResponseVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
        public async Task<IActionResult> CreateSemester([FromBody]SemesterVM resource)
        {
            if (ModelState.IsValid)
            {
                var result = await _semesterService.CreateNewSemester(resource);
                if (result.IsSuccess)
                {
                    var id = Int32.Parse(result.Id!);
                    var existedSemester = await _semesterService.GetSemesterById(id);
                    if(existedSemester is not null)
                    {
                        return CreatedAtAction(nameof(GetSemesterById), new
                        {
                            semesterId = id
                        }, _mapper.Map<SemesterResponseVM>(existedSemester));
                    }
                    else
                    {
                        return BadRequest(new ServiceResponse
                        {
                            IsSuccess = false,
                            Message = "Some error happened",
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
