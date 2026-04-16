using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using Flavian.Shared.Dto;
using Flavian.Shared.Exceptions;

namespace Flavian.Configuration.Middlewares;

public class GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var source = ex.TargetSite?.DeclaringType?.FullName ?? "Unknown Project";
            var methodName = ex.TargetSite?.Name ?? "Unknown Method";
            logger.LogError("PROJECT: {Source} | METHOD: {Method} | ERROR: {Message}",
                source, methodName, ex.Message);

            await HandleGlobalExceptionAsync(context, ex);
        }
    }

    private Task HandleGlobalExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = HttpStatusCode.InternalServerError;
        string message = "An unexpected error occurred.";
        List<ErrorDetails> errors;

        switch (exception)
        {
            case NotFoundException e:
                statusCode = HttpStatusCode.NotFound;
                message = e.Message;
                errors = [new("Not Found", e.Message)];
                break;

            case FeatureNotImplementedException e:
                statusCode = HttpStatusCode.NotImplemented;
                message = e.Message;
                errors = [new("Not Implemented", e.Message)];
                break;

            case Shared.Exceptions.ValidationException e:
                statusCode = HttpStatusCode.BadRequest;
                message = e.Message;
                errors = e.Errors;
                break;

            case InactiveResourceException e:
                statusCode = HttpStatusCode.BadRequest;
                message = e.Message;
                errors = [new("Inactive Resource", e.Message)];
                break;

            case DeletedResourceException e:
                statusCode = HttpStatusCode.BadRequest;
                message = e.Message;
                errors = [new("Resource Deleted", e.Message)];
                break;

            case AlreadyExistsException e:
                statusCode = HttpStatusCode.BadRequest;
                message = e.Message;
                errors = [new("Resource Exists Already", e.Message)];
                break;

            default:
                errors = [new("Internal Error", exception.Message)];
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = ServiceResponse<object>.Error((int)statusCode, message, errors);

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        return context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}
