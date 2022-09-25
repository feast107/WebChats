using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MonitorHub.Apis
{
    [ApiController]
    [Route("Document")]
    public class DocumentController : ControllerBase
    {
        [Route("Swagger")]
        [HttpGet]
        public IActionResult Swagger()
        {
            return Redirect("/swagger/index.html");
        } 
    }
}
