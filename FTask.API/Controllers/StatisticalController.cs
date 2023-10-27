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
        public async Task<IActionResult> GetTaskStatusStatistics([FromQuery]DateTime? from, [FromQuery]DateTime? to)
        {
            if (ModelState.IsValid)
            {
                var result = await _taskService.GetTaskStatusStatistics(from, to);
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
        public async Task<IActionResult> GetTaskCompletionRateStatistics([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int? taskId)
        {
            if (ModelState.IsValid)
            {
                var result = await _taskService.GetTaskCompletionRateStatistics(from, to, (int)TaskStatus.InProgress, taskId);
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
    }
}
