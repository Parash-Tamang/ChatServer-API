using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Agent.API.Controllers
{
    public class ValuesController : ControllerBase
    {
            private readonly IHttpClientFactory _clientFactory;

            public ValuesController(IHttpClientFactory clientFactory)
            {
                _clientFactory = clientFactory;
            }

            public async Task <ActionResult> SendMessageAsync()
            {

                var client = _clientFactory.CreateClient();
                var response = await client.GetAsync("http://127.0.0.1:7060/api/jobschedule");
                return Ok(response);
                
            }
        }
    }
   

