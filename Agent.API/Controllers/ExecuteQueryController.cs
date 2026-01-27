using Agent.Application.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Agent.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExecuteQueryController : Controller
    {
        private readonly ISchoolService _schoolService;
        
        public ExecuteQueryController(ISchoolService schoolService)
        {
            _schoolService = schoolService;
        }
        [HttpPost]
        public async Task<ActionResult<String>> ExecuteQuery (String sql)
        {
            var response = await _schoolService.ExecuteQueryAsync (sql);
            return Ok(response);

        }

    }
}
