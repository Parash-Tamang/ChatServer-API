using System.Text;
using System.Text.RegularExpressions;

public class JsonEscapeFixMiddleware
{
    private readonly RequestDelegate _next;

    public JsonEscapeFixMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.ContentType?.Contains("application/json") == true)
        {
            context.Request.EnableBuffering();

            using var reader = new StreamReader(
                context.Request.Body,
                Encoding.UTF8,
                leaveOpen: true);

            var body = await reader.ReadToEndAsync();

            context.Request.Body.Position = 0;

            if (!string.IsNullOrEmpty(body))
            {
                // Replace single backslashes not already escaped
                var fixedJson = Regex.Replace(
                    body,
                    @"(?<!\\)\\(?![\\\""/bfnrtu])",
                    @"\\");

                var bytes = Encoding.UTF8.GetBytes(fixedJson);
                context.Request.Body = new MemoryStream(bytes);
            }
        }

        await _next(context);
    }
}