using AutoMapper;
using FTask.Service.IService;
using FTask.Service.ViewModel.RequestVM.CreateTaskLecturer;
using FTask.Service.ViewModel.ResposneVM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FTask.API.Controllers
{
    [Route("api/task-lecturers")]
    [ApiController]
    public class TaskLecturerController : ControllerBase
    {
        private readonly ITaskLecturerService _taskLecturerService;
        private readonly IMapper _mapper;

        public TaskLecturerController(ITaskLecturerService taskLecturerService, IMapper mapper)
        {
            _taskLecturerService = taskLecturerService;
            _mapper = mapper;
        }

        [HttpGet("{id}", Name = nameof(GetTaskLecturerById))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TaskLecturerResponseVM))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponseVM))]
        public async Task<IActionResult> GetTaskLecturerById(int id)
        {
            if (ModelState.IsValid)
            {
                var result = await _taskLecturerService.GetTaskLecturerById(id);
                if (result is null)
                {
                    return NotFound("Not found");
                }
                return Ok(_mapper.Map<TaskLecturerResponseVM>(result));
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TaskLecturerResponseVM>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponseVM))]
        public async Task<IActionResult> GetTaskLecturers([FromQuery] int page,
            [FromQuery] int quantity,
            [FromQuery] int? taskId,
            [FromQuery] Guid? lecturerId)
        {
            if (ModelState.IsValid)
            {
                var result = await _taskLecturerService.GetTaskLecturers(page, quantity, taskId, lecturerId);
                return Ok(_mapper.Map<IEnumerable<TaskLecturerResponseVM>>(result));
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
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(TaskLecturerResponseVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponseVM))]
        public async Task<IActionResult> CreateTaskLecturer([FromBody] CreateTaskLecturerVM resource)
        {
            if (ModelState.IsValid)
            {
                var result = await _taskLecturerService.CreateNewTaskLecturer(resource);
                if (result.IsSuccess)
                {
                    int id = Int32.Parse(result.Id!);
                    var existedTaskLecturer = await _taskLecturerService.GetTaskLecturerById(id);
                    if (existedTaskLecturer is not null)
                    {
                        return CreatedAtAction(nameof(GetTaskLecturerById), new
                        {
                            id = id,
                        },
                        _mapper.Map<TaskLecturerResponseVM>(existedTaskLecturer));
                    }
                    else
                    {
                        return BadRequest(new ServiceResponseVM
                        {
                            IsSuccess = false,
                            Message = "Failed to assign task",
                            Errors = new List<string> {"Assigned task not found" }
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
        public async Task<IActionResult> DeleteTaskLecturer([FromQuery] int id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _taskLecturerService.DeleteTaskLecturer(id);
                    if (result)
                    {
                        return Ok(new ServiceResponseVM
                        {
                            IsSuccess = true,
                            Message = "Unassign task successfully"
                        });
                    }
                    else
                    {
                        return BadRequest(new ServiceResponseVM
                        {
                            IsSuccess = false,
                            Message = "Failed to unassign task"
                        });
                    }
                }
                catch (DbUpdateException ex)
                {
                    return BadRequest(new ServiceResponseVM
                    {
                        IsSuccess = false,
                        Message = "Failed to unassign task",
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
