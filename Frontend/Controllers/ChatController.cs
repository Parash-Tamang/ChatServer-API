using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers
{

    public class ChatController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [Authorize]
        public IActionResult Chat(string userid,string prompt,int chatid=0,bool newFlag=false)
        {
            //userid
            //call api chat
            //api- database add// call llm
            //llm response -update database
            return Json(new {status="200",responseitem="" } );
        }
    }
}
