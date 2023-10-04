using AutoMapper;
using FTask.Service.IService;
using FTask.Service.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FTask.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LecturerController : ControllerBase
    {
        private readonly ILecturerService _lecturerService;
        private readonly IMapper _mapper;

        public LecturerController(ILecturerService lecturerService, IMapper mapper)
        {
            _lecturerService = lecturerService;
            _mapper = mapper;
        }

        [Authorize(Policy = "Admin")]
        [HttpGet("{lecturerId}", Name = nameof(GetLecturerById))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserInformationResponseVM))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> GetLecturerById(Guid lecturerId)
        {
            var result = await _lecturerService.GetLectureById(lecturerId);
            if (result is null)
            {
                return NotFound("Not found");
            }
            return Ok(_mapper.Map<UserInformationResponseVM>(result));
        }
        
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UserInformationResponseVM>))]
        public async Task<IActionResult> GetLecturers([FromQuery] int page, [FromQuery] int amount)
        {
            var result = await _lecturerService.GetLecturers(page, amount);
            return Ok(_mapper.Map<IEnumerable<UserInformationResponseVM>>(result));
        }
    }
}
