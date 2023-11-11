using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FTask.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Ok()
        {
            return Ok("demo1111");
        }
    }
}
