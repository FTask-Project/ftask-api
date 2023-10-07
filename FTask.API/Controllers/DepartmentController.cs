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
    public class DepartmentController : ControllerBase
    {
        private readonly ICacheService<Department> _cacheService;
        private readonly IDepartmentService _departmentService;
        private readonly IMapper _mapper;

        public DepartmentController (
            ICacheService<Department> cacheService,
            IDepartmentService departmentService,
            IMapper mapper
            )
        {
            _departmentService = departmentService;
            _cacheService = cacheService;
            _mapper = mapper;
        }

        [HttpGet("{departmentId}", Name = nameof(GetDeaprtmentById))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DepartmentResponseVM))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> GetDeaprtmentById(int departmentId)
        {
            if (ModelState.IsValid)
            {
                string key = $"department-{departmentId}";
                var cachedData = await _cacheService.GetAsync(key);
                if (cachedData is null)
                {
                    var departmentResult = await _departmentService.GetDepartmentById(departmentId);
                    if (departmentResult is null)
                    {
                        return NotFound("Not found");
                    }
                    await _cacheService.SetAsync(key, departmentResult);
                    return Ok(_mapper.Map<DepartmentResponseVM>(departmentResult));
                }

                return Ok(_mapper.Map<DepartmentResponseVM>(cachedData));
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
            string key = $"department-{page}-{quantity}";
            var cachedData = await _cacheService.GetAsyncArray(key);
            if (cachedData is null)
            {
                var departmentList = await _departmentService.GetDepartments(page, quantity);
                if (!departmentList.IsNullOrEmpty())
                {
                    await _cacheService.SetAsync(key, departmentList);
                }
                return Ok(_mapper.Map<IEnumerable<DepartmentResponseVM>>(departmentList));
            }

            return Ok(_mapper.Map<IEnumerable<DepartmentResponseVM>>(cachedData));
            
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
