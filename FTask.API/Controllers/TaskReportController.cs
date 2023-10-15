using AutoMapper;
using FTask.Service.IService;
using FTask.Service.ViewModel.RequestVM.CreateTaskReport;
using FTask.Service.ViewModel.ResposneVM;
using Microsoft.AspNetCore.Mvc;

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
                    int id = Int32.Parse(result.Id!);
                    var existedTaskReport = await _taskReportService.GetTaskReportById(id);
                    if (existedTaskReport is not null)
                    {
                        return CreatedAtAction(nameof(GetTaskReportById), new
                        {
                            id = id,
                        },
                        _mapper.Map<TaskReportResponseVM>(existedTaskReport));
                    }
                    else
                    {
                        return BadRequest(new ServiceResponseVM
                        {
                            IsSuccess = false,
                            Message = "Failed to create new task report",
                            Errors = new string[1] { "Created report not found" }
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
    }
}
