using FTask.Service.IService;
using FTask.Service.ViewModel.ResposneVM;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskStatus = FTask.Service.Enum.TaskStatus;

namespace FTask.API.Controllers
{
    [Route("api/statistics")]
    [ApiController]
    public class StatisticalController : ControllerBase
    {
        private readonly ITaskService _taskService;
        public StatisticalController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet("task-status")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TaskStatusStatisticResposneVM))]
        public async Task<IActionResult> GetTaskStatusStatistics([FromQuery]DateTime? from, [FromQuery]DateTime? to, [FromQuery] int? semesterId)
        {
            if (ModelState.IsValid)
            {
                var result = await _taskService.GetTaskStatusStatistics(from, to, semesterId);
                return Ok(result);
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

        [HttpGet("task-completion-rate")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TaskCompleteionRateStatisticResponseVM>))]
        public async Task<IActionResult> GetTaskCompletionRateStatistics([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int? taskId, [FromQuery] int? semesterId)
        {
            if (ModelState.IsValid)
            {
                var result = await _taskService.GetTaskCompletionRateStatistics(from, to, (int)TaskStatus.InProgress, taskId, semesterId);
                return Ok(result);
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

        [HttpGet("task-created")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<NumberOfCreatedTaskStatisticsResponseVM>))]
        public async Task<IActionResult> GetCreatedTaskCountStatistics([FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] int? semeterId)
        {
            if (ModelState.IsValid)
            {
                var result = await _taskService.GetCreatedTaskCountStatistics(from, to, semeterId);
                return Ok(result);
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

        [HttpGet("lecturer/task-status")]

        public IActionResult GetTaskStatusStatisticsOfLecturer([FromQuery]Guid lecturerId)
        {
            if (ModelState.IsValid)
            {
                return Ok(_taskService.GetTaskStatistics(lecturerId));
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
