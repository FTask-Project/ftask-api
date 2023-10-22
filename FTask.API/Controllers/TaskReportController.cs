using AutoMapper;
using FTask.Service.IService;
using FTask.Service.ViewModel.RequestVM.TaskReport;
using FTask.Service.ViewModel.ResposneVM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FTask.API.Controllers
{
    [Route("api/task-reports")]
    [ApiController]
    public class TaskReportController : ControllerBase
    {
        private readonly ITaskReportService _taskReportService;
        private readonly IMapper _mapper;

        public TaskReportController(ITaskReportService taskReportService, IMapper mapper)
        {
            _taskReportService = taskReportService;
            _mapper = mapper;
        }

        [HttpGet("{id}", Name = nameof(GetTaskReportById))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TaskReportResponseVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponseVM))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> GetTaskReportById(int id)
        {
            if (ModelState.IsValid)
            {
                var result = await _taskReportService.GetTaskReportById(id);
                if (result is null)
                {
                    return NotFound("Not found");
                }
                return Ok(_mapper.Map<TaskReportResponseVM>(result));
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TaskReportResponseVM>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponseVM))]
        public async Task<IActionResult> GetTaskReports([FromQuery] int page,
            [FromQuery] int quantity,
            [FromQuery] int? taskActivityId)
        {
            if (ModelState.IsValid)
            {
                var result = await _taskReportService.GetTaskReports(page, quantity, taskActivityId);
                return Ok(_mapper.Map<IEnumerable<TaskReportResponseVM>>(result));
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
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(TaskReportResponseVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponseVM))]
        public async Task<IActionResult> CreateTaskLecturer([FromForm] TaskReportVM resource)
        {
            if (ModelState.IsValid)
            {
                var result = await _taskReportService.CreateNewTaskReport(resource);
                if (result.IsSuccess)
                {
                    return CreatedAtAction(nameof(GetTaskReportById), new
                    {
                        id = result.Entity!.TaskReportId,
                    }, _mapper.Map<TaskReportResponseVM>(result.Entity!));
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
        public async Task<IActionResult> DeleteTaskReport([FromQuery] int id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _taskReportService.DeleteTaskReport(id);
                    if (result)
                    {
                        return Ok(new ServiceResponseVM
                        {
                            IsSuccess = true,
                            Message = "Delete task report successfully"
                        });
                    }
                    else
                    {
                        return BadRequest(new ServiceResponseVM
                        {
                            IsSuccess = false,
                            Message = "Failed to delete task report"
                        });
                    }
                }
                catch (DbUpdateException ex)
                {
                    return BadRequest(new ServiceResponseVM
                    {
                        IsSuccess = false,
                        Message = "Failed to delete task report",
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

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TaskReportResponseVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponseVM))]
        public async Task<IActionResult> UpdateTaskReport([FromForm] UpdateTaskReportVM resource, int id)
        {
            if (ModelState.IsValid)
            {
                var result = await _taskReportService.UpdateTaskReport(resource, id);
                if (result.IsSuccess)
                {
                    return Ok(_mapper.Map<TaskReportResponseVM>(result.Entity));
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
