using AutoMapper;
using FTask.Repository.Entity;
using FTask.Service.IService;
using FTask.Service.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace FTask.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;
        private readonly IMapper _mapper;

        public DepartmentController(IDepartmentService departmentService, IMapper mapper)
        {
            _departmentService = departmentService;
            _mapper = mapper;
        }

        [HttpGet("{departmentId}", Name = nameof(GetDeaprtmentById))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DepartmentResponseVM))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> GetDeaprtmentById(int departmentId)
        {
            var result = await _departmentService.GetDepartmentById(departmentId);
            if(result is null)
            {
                return NotFound("Not found");
            }
            return Ok(_mapper.Map<DepartmentResponseVM>(result));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<DepartmentResponseVM>))]
        public async Task<IActionResult> GetDepartments([FromQuery]int page, [FromQuery]int amount)
        {
            var result = await _departmentService.GetDepartments(page, amount);
            return Ok(_mapper.Map<IEnumerable<DepartmentResponseVM>>(result));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(DepartmentResponseVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
        public async Task<IActionResult> CreateDepartment([FromBody] DepartmentVM resource)
        {
            if (ModelState.IsValid)
            {
                var result = await _departmentService.CreateNewDepartment(_mapper.Map<Department>(resource));
                if (result.IsSuccess)
                {
                    var existedDepartment = await _departmentService.GetDepartmentById(result.Id);
                    if (existedDepartment is not null)
                    {
                        return CreatedAtAction(nameof(GetDeaprtmentById),
                        new
                        {
                            departmentId = result.Id
                        }, _mapper.Map<DepartmentResponseVM>(existedDepartment));
                    }
                    else
                    {
                        return BadRequest(new ServiceResponse
                        {
                            IsSuccess = false,
                            Message = "Some error happened"
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
