using AutoMapper;
using FTask.Repository.Entity;
using FTask.Service.IService;
using FTask.Service.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace FTask.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubjectController : ControllerBase
    {
        private readonly ISubjectService _subjectService;
        private readonly IMapper _mapper;

        public SubjectController(ISubjectService subjectService, IMapper mapper)
        {
            _subjectService = subjectService;
            _mapper = mapper;
        }

        [HttpGet("{subjectId}", Name = nameof(GetSubjectById))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SubjectResponseVM))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> GetSubjectById(int subjectId)
        {
            var subjectResult = await _subjectService.GetSubjectById(subjectId);
            if (subjectResult is null)
            {
                return NotFound("Not found");
            }
            return Ok(_mapper.Map<SubjectResponseVM>(subjectResult));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<SubjectResponseVM>))]
        public async Task<IActionResult> GetSubjects([FromQuery] int page, [FromQuery] int amount)
        {
            var subjectList = await _subjectService.GetSubjectAllSubject(page, amount);
            return Ok(_mapper.Map<IEnumerable<SubjectResponseVM>>(subjectList));
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
                    var existedSubject = await _subjectService.GetSubjectById(result.Id);
                    if (existedSubject is not null)
                    {
                        return CreatedAtAction(
                            nameof(GetSubjectById),
                            new { subjectId = result.Id }, 
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
