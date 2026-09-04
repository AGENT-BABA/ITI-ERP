using System.Net;
using System.Text.Json;
using System.Data.Common;
using ITI.ERP.Application.Common.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace ITI.ERP.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _env;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public GlobalExceptionMiddleware(RequestDelegate next, IHostEnvironment env)
    {
        _next = next;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = exception switch
        {
            NotFoundException => (int)HttpStatusCode.NotFound,
            ForbiddenAccessException => (int)HttpStatusCode.Forbidden,
            BadRequestException => (int)HttpStatusCode.BadRequest,
            ConflictException => (int)HttpStatusCode.Conflict,
            FluentValidation.ValidationException => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.InternalServerError
        };

        var message = exception switch
        {
            FluentValidation.ValidationException => exception.Message,
            DbUpdateException dbEx => dbEx.InnerException?.Message ?? dbEx.Message,
            DbException dbSqlEx => dbSqlEx.Message,
            _ => exception.Message
        };

        Log.Error(exception, "Unhandled exception occurred: {Message}", message);

        var isProduction = _env.IsProduction();

        var response = exception switch
        {
            FluentValidation.ValidationException validationEx => new ErrorResponse
            {
                StatusCode = statusCode,
                Message = "Validation failed",
                Errors = validationEx.Errors.Select(e => new ValidationError
                {
                    Property = e.PropertyName,
                    Message = e.ErrorMessage
                })
            },
            NotFoundException notFoundEx => new ErrorResponse
            {
                StatusCode = statusCode,
                Message = notFoundEx.Message
            },
            ForbiddenAccessException forbiddenEx => new ErrorResponse
            {
                StatusCode = statusCode,
                Message = forbiddenEx.Message
            },
            BadRequestException badRequestEx => new ErrorResponse
            {
                StatusCode = statusCode,
                Message = badRequestEx.Message
            },
            ConflictException conflictEx => new ErrorResponse
            {
                StatusCode = statusCode,
                Message = conflictEx.Message
            },
            _ => new ErrorResponse
            {
                StatusCode = statusCode,
                Message = isProduction ? "An unexpected error occurred. Please try again later." : message
            }
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var json = JsonSerializer.Serialize(response, JsonOptions);
        await context.Response.WriteAsync(json);
    }
}

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public IEnumerable<ValidationError>? Errors { get; set; }
}

public class ValidationError
{
    public string Property { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
