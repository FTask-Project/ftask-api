using AutoMapper;
using Duende.IdentityServer.Extensions;
using FTask.Service.IService;
using FTask.Service.ViewModel.RequestVM.Task;
using FTask.Service.ViewModel.ResposneVM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace FTask.API.Controllers
{
    [Route("api/tasks")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ITaskService _taskService;

        public TaskController(IMapper mapper, ITaskService taskService)
        {
            _mapper = mapper;
            _taskService = taskService;
        }

        [HttpGet("{id}", Name = nameof(GetTaskById))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TaskResponseVM))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponseVM))]
        public async Task<IActionResult> GetTaskById(int id)
        {
            if (ModelState.IsValid)
            {
                var result = await _taskService.GetTaskById(id);
                if (result is null)
                {
                    return NotFound("Not found");
                }
                return Ok(_mapper.Map<TaskResponseVM>(result));
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TaskResponseVM>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponseVM))]
        public async Task<IActionResult> GetTasks([FromQuery] int page,
            [FromQuery] int quantity,
            [FromQuery] string? filter,
            [FromQuery] int? semsesterId,
            [FromQuery] int? departmentId,
            [FromQuery] int? subjectId,
            [FromQuery] int? status)
        {
            if (ModelState.IsValid)
            {
                var result = await _taskService.GetTasks(page, quantity, filter ?? "", semsesterId, departmentId, subjectId, status);
                return Ok(_mapper.Map<IEnumerable<TaskResponseVM>>(result));
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
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(TaskResponseVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponseVM))]
        public async Task<IActionResult> CreateTask([FromForm] TaskVM resource)
        {
            if (ModelState.IsValid)
            {
                var data = new string("[" + HttpContext.Request.Form["TaskLecturers"].ToString() + "]");
                var filePaths = new string("[" + HttpContext.Request.Form["FilePaths"].ToString() + "]");
                if (!data.IsNullOrEmpty())
                {
                    resource.TaskLecturers = JsonConvert.DeserializeObject<IEnumerable<TaskLecturerVM>>(data!) ?? new List<TaskLecturerVM>();
                }
                if (!filePaths.IsNullOrEmpty())
                {
                    resource.FilePaths = JsonConvert.DeserializeObject<IEnumerable<FilePathVM>>(filePaths) ?? new List<FilePathVM>();
                }

                var result = await _taskService.CreateNewTask(resource);
                if (result.IsSuccess)
                {
                    return CreatedAtAction(nameof(GetTaskById), new
                    {
                        id = result.Entity!.TaskId,
                    }, _mapper.Map<TaskResponseVM>(result.Entity!));
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
        public async Task<IActionResult> DeleteTask([FromQuery] int id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _taskService.DeleteTask(id);
                    if (result)
                    {
                        return Ok(new ServiceResponseVM
                        {
                            IsSuccess = true,
                            Message = "Delete task successfully"
                        });
                    }
                    else
                    {
                        return BadRequest(new ServiceResponseVM
                        {
                            IsSuccess = false,
                            Message = "Failed to delete task"
                        });
                    }
                }
                catch (DbUpdateException ex)
                {
                    return BadRequest(new ServiceResponseVM
                    {
                        IsSuccess = false,
                        Message = "Failed to delete task",
                        Errors = new string[1] { ex.Message }
                    });
                }
                catch (OperationCanceledException)
                {
                    return BadRequest(new ServiceResponseVM
                    {
                        IsSuccess = false,
                        Message = "Failed to delete task",
                        Errors = new string[1] { "The operation has been cancelled" }
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

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TaskResponseVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponseVM))]
        public async Task<IActionResult> UpdateTask([FromForm] UpdateTaskVM resource, int id)
        {
            if (ModelState.IsValid)
            {
                var filePaths = new string("[" + HttpContext.Request.Form["AddedFilePaths"].ToString() + "]");
                if (!filePaths.IsNullOrEmpty())
                {
                    resource.AddedFilePaths = JsonConvert.DeserializeObject<IEnumerable<FilePathVM>>(filePaths) ?? new List<FilePathVM>();
                }

                var result = await _taskService.UpdateTask(resource, id);
                if (result.IsSuccess)
                {
                    return Ok(_mapper.Map<TaskResponseVM>(result.Entity));
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
    }
}
