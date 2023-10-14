using AutoMapper;
using FTask.Repository.Entity;
using FTask.Service.IService;
using FTask.Service.ViewModel;
using FTask.Service.ViewModel.RequestVM.CreateDepartment;
using Microsoft.AspNetCore.Mvc;

namespace FTask.API.Controllers
{
    [Route("api/departments")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;
        private readonly IMapper _mapper;

        public DepartmentController(
            IDepartmentService departmentService,
            IMapper mapper
            )
        {
            _departmentService = departmentService;
            _mapper = mapper;
        }

        [HttpGet("{id}", Name = nameof(GetDeaprtmentById))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DepartmentResponseVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> GetDeaprtmentById(int id)
        {
            if (ModelState.IsValid)
            {
                var departmentResult = await _departmentService.GetDepartmentById(id);
                if (departmentResult is null)
                {
                    return NotFound("Not found");
                }
                return Ok(_mapper.Map<DepartmentResponseVM>(departmentResult));
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
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
        public async Task<IActionResult> GetDepartments([FromQuery] int page, [FromQuery] int quantity, [FromQuery] string? filter, [FromQuery] Guid? headerId)
        {
            if (ModelState.IsValid)
            {
                var departmentList = await _departmentService.GetDepartments(page, quantity, filter ?? "", headerId);
                return Ok(_mapper.Map<IEnumerable<DepartmentResponseVM>>(departmentList));
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
                            id = id
                        }, _mapper.Map<DepartmentResponseVM>(existedDepartment));
                    }
                    else
                    {
                        return BadRequest(new ServiceResponse
                        {
                            IsSuccess = false,
                            Message = "Create department failed",
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
