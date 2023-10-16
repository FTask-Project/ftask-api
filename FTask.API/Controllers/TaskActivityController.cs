using AutoMapper;
using FTask.Service.IService;
using FTask.Service.ViewModel.RequestVM.CreateTaskActivity;
using FTask.Service.ViewModel.ResposneVM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FTask.API.Controllers
{
    [Route("api/task-activities")]
    [ApiController]
    public class TaskActivityController : ControllerBase
    {
        private readonly ITaskActivityService _taskActivityService;
        private readonly IMapper _mapper;

        public TaskActivityController(ITaskActivityService taskActivityService, IMapper mapper)
        {
            _taskActivityService = taskActivityService;
            _mapper = mapper;
        }

        [HttpGet("{id}", Name = nameof(GetTaskActivityById))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TaskActivityResponseVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponseVM))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> GetTaskActivityById(int id)
        {
            if (ModelState.IsValid)
            {
                var result = await _taskActivityService.GetTaskActivityById(id);
                if (result is null)
                {
                    return NotFound("Not found");
                }
                return Ok(_mapper.Map<TaskActivityResponseVM>(result));
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TaskActivityResponseVM>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponseVM))]
        public async Task<IActionResult> GetTaskActivities([FromQuery] int page, [FromQuery] int quantity, [FromQuery] string? filter, [FromQuery] int? taskLecturerId)
        {
            if (ModelState.IsValid)
            {
                var taskActivityList = await _taskActivityService.GetTaskActivities(page, quantity, filter ?? "", taskLecturerId);
                return Ok(_mapper.Map<IEnumerable<TaskActivityResponseVM>>(taskActivityList));
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
        public async Task<IActionResult> CreateTaskActivity([FromBody] CreateTaskActivityVM resource)
        {
            if (ModelState.IsValid)
            {
                var result = await _taskActivityService.CreateNewActivity(resource);
                if (result.IsSuccess)
                {
                    int id = Int32.Parse(result.Id!);
                    var existedTaskActivity = await _taskActivityService.GetTaskActivityById(id);
                    if (existedTaskActivity is not null)
                    {
                        return CreatedAtAction(nameof(GetTaskActivityById),
                        new
                        {
                            id = id
                        }, _mapper.Map<TaskActivityResponseVM>(existedTaskActivity));
                    }
                    else
                    {
                        return BadRequest(new ServiceResponseVM
                        {
                            IsSuccess = false,
                            Message = "Failed to create new task activity",
                            Errors = new List<string> { "Created activity not found" }
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
        public async Task<IActionResult> DeleteTaskActivity([FromQuery] int id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _taskActivityService.DeleteTaskActivity(id);
                    if (result)
                    {
                        return Ok(new ServiceResponseVM
                        {
                            IsSuccess = true,
                            Message = "Delete task activity successfully"
                        });
                    }
                    else
                    {
                        return BadRequest(new ServiceResponseVM
                        {
                            IsSuccess = false,
                            Message = "Failed to delete task activity"
                        });
                    }
                }
                catch (DbUpdateException ex)
                {
                    return BadRequest(new ServiceResponseVM
                    {
                        IsSuccess = false,
                        Message = "Failed to delete task activity",
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
