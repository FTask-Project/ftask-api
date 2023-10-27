using FTask.Service.IService;
using FTask.Service.ViewModel.ResposneVM;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
            var result = await _taskService.GetTaskStatusStatistics(from, to);
            return Ok(result);
        }
    }
}
