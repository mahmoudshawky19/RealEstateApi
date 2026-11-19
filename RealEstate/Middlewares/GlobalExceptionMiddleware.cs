
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace API.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
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
            _logger.LogError(
                ex,
                "An unhandled exception occurred. Method: {Method}, Path: {Path}",
                context.Request.Method,
                context.Request.Path);

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.Clear();

            var (statusCode, title, detail) = ex switch
            {
                KeyNotFoundException =>
                    (404, "Resource Not Found",
                        "The requested resource was not found."),

                UnauthorizedAccessException =>
                    (403, "Forbidden",
                        "You are not allowed to perform this action."),

                ArgumentException =>
                    (400, "Bad Request",
                        "The request contains invalid arguments."),

                _ =>
                    (500, "Internal Server Error",
                        "An unexpected error occurred.")
            };

            context.Response.StatusCode = statusCode;

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            };

            problemDetails.Extensions["traceId"] =
                context.TraceIdentifier;

            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}
