using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreApi.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = MapException(exception);

        var response = new ProblemDetails
        {
            Title = "Error occurred",
            Detail = exception.Message,
            Status = statusCode
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static (int statusCode, string message) MapException(Exception ex)
    {
        return ex switch
        {
            KeyNotFoundException => ((int)HttpStatusCode.NotFound, ex.Message),
            ArgumentException => ((int)HttpStatusCode.BadRequest, ex.Message),
            InvalidOperationException => ((int)HttpStatusCode.Conflict, ex.Message),
            _ => ((int)HttpStatusCode.InternalServerError, "Unexpected error occurred")
        };
    }
}