using AutoMapper;
using FTask.Repository.Entity;
using FTask.Service.IService;
using FTask.Service.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Utilities;

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
            if (ModelState.IsValid)
            {
                var result = await _departmentService.GetDepartmentById(departmentId);
                if (result is null)
                {
                    return NotFound("Not found");
                }
                return Ok(_mapper.Map<DepartmentResponseVM>(result));
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<DepartmentResponseVM>))]
        public async Task<IActionResult> GetDepartments([FromQuery]int page, [FromQuery]int quantity)
        {
            var result = await _departmentService.GetDepartments(page, quantity);
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
                    int id = Int32.Parse(result.Id!);
                    var existedDepartment = await _departmentService.GetDepartmentById(id);
                    if (existedDepartment is not null)
                    {
                        return CreatedAtAction(nameof(GetDeaprtmentById),
                        new
                        {
                            departmentId = id
                        }, _mapper.Map<DepartmentResponseVM>(existedDepartment));
                    }
                    else
                    {
                        return BadRequest(new ServiceResponse
                        {
                            IsSuccess = false,
                            Message = "Some error happened",
                            Errors = new List<string> { "Error at create new department action method", "Created department not found" }
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
