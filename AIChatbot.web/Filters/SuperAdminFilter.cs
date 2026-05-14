using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using AIChatbot.web.Filters; // This must be here

namespace AIChatbot.web.Filters
{
    public class SuperAdminFilter : IActionFilter
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SuperAdminFilter(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var token = httpContext?.Request.Cookies["accessToken"];

            if (string.IsNullOrEmpty(token))
            {
                context.Result = new NotFoundResult();
                return;
            }

            var role = AuthFilter.ExtractRoleFromToken(token); // Works — it's internal static

            if (role != "SuperAdmin")
            {
                context.Result = new NotFoundResult();
                return;
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}