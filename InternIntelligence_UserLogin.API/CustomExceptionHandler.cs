using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace InternIntelligence_UserLogin.API
{
    public class CustomExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var problemDetails = GenerateProblemDetails(exception, httpContext);

            return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                Exception = exception,
                HttpContext = httpContext,
                ProblemDetails = problemDetails
            });
        }

        private static ProblemDetails GenerateProblemDetails(Exception exception, HttpContext httpContext)
        {
            var statusCode = exception switch
            {
                ArgumentException => StatusCodes.Status400BadRequest,
                ValidationException => StatusCodes.Status400BadRequest,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                KeyNotFoundException => StatusCodes.Status404NotFound,
                NotImplementedException => StatusCodes.Status501NotImplemented,
                _ => StatusCodes.Status500InternalServerError
            };

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = GetTitleForStatusCode(statusCode),
                Type = exception.GetType().Name,
                Detail = exception.Message,
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
            };

            // Add extra details for ValidationException
            if (exception is ValidationException validationException)
            {
                problemDetails.Extensions["errors"] = validationException.Data;
            }

            // Include stack trace in Development environment
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            }

            return problemDetails;
        }

        private static string GetTitleForStatusCode(int statusCode) => statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad Request",
            StatusCodes.Status401Unauthorized => "Unauthorized",
            StatusCodes.Status404NotFound => "Not Found",
            StatusCodes.Status500InternalServerError => "Internal Server Error",
            StatusCodes.Status501NotImplemented => "Not Implemented",
            _ => "An error occurred"
        };
    }
}
