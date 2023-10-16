using AutoMapper;
using FTask.Repository.Entity;
using FTask.Service.IService;
using FTask.Service.ViewModel.RequestVM.CreateDepartment;
using FTask.Service.ViewModel.ResposneVM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponseVM))]
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
                return BadRequest(new ServiceResponseVM
                {
                    IsSuccess = false,
                    Message = "Invalid input"
                });
            }
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<DepartmentResponseVM>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponseVM))]
        public async Task<IActionResult> GetDepartments([FromQuery] int page, [FromQuery] int quantity, [FromQuery] string? filter, [FromQuery] Guid? headerId)
        {
            if (ModelState.IsValid)
            {
                var departmentList = await _departmentService.GetDepartments(page, quantity, filter ?? "", headerId);
                return Ok(_mapper.Map<IEnumerable<DepartmentResponseVM>>(departmentList));
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

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(DepartmentResponseVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponseVM))]
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
                        return BadRequest(new ServiceResponseVM
                        {
                            IsSuccess = false,
                            Message = "Failed to create new department",
                            Errors = new List<string> { "Created department not found" }
                        });
                    }
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
        public async Task<IActionResult> DeleteDepartment([FromQuery] int id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _departmentService.DeleteDepartment(id);
                    if (result)
                    {
                        return Ok(new ServiceResponseVM
                        {
                            IsSuccess = true,
                            Message = "Delete department successfully"
                        });
                    }
                    else
                    {
                        return BadRequest(new ServiceResponseVM
                        {
                            IsSuccess = false,
                            Message = "Failed to delete department"
                        });
                    }
                }
                catch(DbUpdateException ex)
                {
                    return BadRequest(new ServiceResponseVM
                    {
                        IsSuccess = false,
                        Message = "Failed to delete department",
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
    }
}
