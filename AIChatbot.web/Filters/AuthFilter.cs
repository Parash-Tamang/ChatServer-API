using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using AIChatbot.web.Services;

namespace AIChatbot.web.Filters
{
    public class AuthFilter : IActionFilter
    {
        private readonly TokenService _token;

        public AuthFilter(TokenService token)
        {
            _token = token;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var http = context.HttpContext;
            var accessToken = http.Request.Cookies["accessToken"];

            if (string.IsNullOrEmpty(accessToken))
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
