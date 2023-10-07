using AutoMapper;
using Duende.IdentityServer.Extensions;
using FTask.Repository.Entity;
using FTask.Service.Caching;
using FTask.Service.IService;
using FTask.Service.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace FTask.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SemesterController : ControllerBase
    {
        private readonly ICacheService<Semester> _cacheService;
        private readonly ISemesterService _semesterService;
        private readonly IMapper _mapper;

        public SemesterController(
            ICacheService<Semester> cacheService,
            ISemesterService semesterService,
            IMapper mapper
            )
        {
            _mapper = mapper;
            _semesterService = semesterService;
            _cacheService = cacheService;
        }

        [HttpGet("{semesterId}", Name = nameof(GetSemesterById))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SemesterResponseVM))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> GetSemesterById(int semesterId)
        {
            if (ModelState.IsValid)
            {
                string key = $"semester-{semesterId}";
                var cachedData = await _cacheService.GetAsync(key);
                if (cachedData is null)
                {
                    var semesterResult = await _semesterService.GetSemesterById(semesterId);
                    if (semesterResult is null)
                    {
                        return NotFound("Not found");
                    }
                    await _cacheService.SetAsync(key, semesterResult);
                    return Ok(_mapper.Map<SemesterResponseVM>(semesterResult));
                }

                return Ok(_mapper.Map<SemesterResponseVM>(cachedData));
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
        public async Task<IActionResult> GetSemester([FromQuery] int page, [FromQuery] int quantity)
        {
            string key = $"semester-{page}-{quantity}";
            var cachedData = await _cacheService.GetAsyncArray(key);
            if (cachedData is null)
            {
                var semesterList = await _semesterService.GetSemesters(page, quantity);
                if (!semesterList.IsNullOrEmpty())
                {
                    await _cacheService.SetAsync(key, semesterList);
                }

                return Ok(_mapper.Map<IEnumerable<SemesterResponseVM>>(semesterList));
            }

            return Ok(_mapper.Map<IEnumerable<SemesterResponseVM>>(cachedData));
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
