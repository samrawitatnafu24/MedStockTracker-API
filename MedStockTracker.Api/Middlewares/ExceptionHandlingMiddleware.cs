using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MedStockTracker.Api.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // Unhandled structural system exceptions are logged as errors; validation rules are handled gracefully
                if (ex is not ValidationException)
                {
                    _logger.LogError(ex, "An unhandled exception occurred in the application execution pipeline.");
                }

                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var statusCode = HttpStatusCode.InternalServerError;
            string message = "An internal server error occurred. Please try again later.";
            object? errorsDetails = null;

            // Intercept FluentValidation pipeline exceptions
            if (exception is ValidationException validationException)
            {
                statusCode = HttpStatusCode.BadRequest;
                message = "Validation failed for the requested operation.";

                // Group individual field level failures by their property target names
                errorsDetails = validationException.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => x.ErrorMessage).ToArray()
                    );
            }
            else if (exception is ArgumentException argumentException)
            {
                statusCode = HttpStatusCode.BadRequest;
                message = argumentException.Message;
            }

            context.Response.StatusCode = (int)statusCode;

            var errorPayload = new
            {
                success = false,
                statusCode = context.Response.StatusCode,
                message = message,
                errors = errorsDetails,
                exceptionMessage = exception.Message,
                stackTrace = exception.StackTrace,
                innerException = exception.InnerException?.Message,
                timestamp = DateTime.UtcNow
            };

            var jsonResponse = JsonSerializer.Serialize(errorPayload);
            return context.Response.WriteAsync(jsonResponse);
        }
    }
}
