using AutoMapper;
using FTask.Repository.Entity;
using FTask.Service.Caching;
using FTask.Service.IService;
using FTask.Service.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace FTask.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubjectController : ControllerBase
    {
        private readonly ICacheService<Subject> _cacheService;
        private readonly ISubjectService _subjectService;
        private readonly IMapper _mapper;

        public SubjectController(
            ICacheService<Subject> cacheService,
            ISubjectService subjectService,
            IMapper mapper
            )
        {
            _subjectService = subjectService;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        [HttpGet("{subjectId}", Name = nameof(GetSubjectById))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SubjectResponseVM))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> GetSubjectById(int subjectId)
        {
            string key = $"subject-{subjectId}";
            var cachedData = await _cacheService.GetAsync(key);
            if (cachedData is null)
            {
                var subjectResult = await _subjectService.GetSubjectById(subjectId);
                if (subjectResult is null)
                {
                    return NotFound("Not found");
                }

                await _cacheService.SetAsync(key, subjectResult);

                return Ok(_mapper.Map<SubjectResponseVM>(subjectResult));
            }

            return Ok(_mapper.Map<SubjectResponseVM>(cachedData));

        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<SubjectResponseVM>))]
        public async Task<IActionResult> GetSubjects([FromQuery] int page, [FromQuery] int amount)
        {
            string key = $"subject-{page}-{amount}";
            var cachedData = await _cacheService.GetAsyncArray(key);
            if (cachedData is null)
            {
                var subjectList = await _subjectService.GetSubjectAllSubject(page, amount);
                if (!subjectList.IsNullOrEmpty())
                {
                    await _cacheService.SetAsync(key, subjectList);
                }
                return Ok(_mapper.Map<IEnumerable<SubjectResponseVM>>(subjectList));
            }

            return Ok(_mapper.Map<IEnumerable<SubjectResponseVM>>(cachedData));

        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SubjectResponseVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
        public async Task<IActionResult> CreateSubject([FromBody] SubjectVM subject)
        {
            if (ModelState.IsValid)
            {
                var result = await _subjectService.CreateNewSubject(_mapper.Map<Subject>(subject));
                if (result.IsSuccess)
                {
                    var id = Int32.Parse(result.Id!);
                    var existedSubject = await _subjectService.GetSubjectById(id);
                    if (existedSubject is not null)
                    {
                        return CreatedAtAction(
                            nameof(GetSubjectById),
                            new { subjectId = id },
                            _mapper.Map<SubjectResponseVM>(existedSubject)
                        );
                    }
                    else
                    {
                        return BadRequest(new ServiceResponse
                        {
                            IsSuccess = false,
                            Message = "Error occur"
                        });
                    }
                }
                else
                {
                    return BadRequest(new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Create new subject fail"
                    });
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

        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
