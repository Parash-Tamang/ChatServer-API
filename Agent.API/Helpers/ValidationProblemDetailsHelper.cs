using FluentValidation.Results;

namespace Agent.API.Helpers
{
    public static class ValidationProblemDetailsHelper
    {
        public static HttpValidationProblemDetails Build(ValidationResult result,string title="Validation Failed")
        {
            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            return new HttpValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = title,
                //Detail = "One or more validation errors occurred."
            };

        }
    }
}
