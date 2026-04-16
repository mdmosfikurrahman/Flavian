namespace Flavian.Shared.Dto;

public static class ApiResponseHelper
{
    public static StandardResponse Success(string entityName, object? data = null, string? customMessage = null) =>
        new()
        {
            IsSuccess = true,
            StatusCode = StatusCodes.Success200,
            Message = customMessage ?? $"{entityName} retrieved successfully.",
            Data = data
        };

    public static StandardResponse Created(string entityName, object? data = null, string? customMessage = null) =>
        new()
        {
            IsSuccess = true,
            StatusCode = StatusCodes.Created201,
            Message = customMessage ?? $"{entityName} created successfully.",
            Data = data
        };

    public static StandardResponse Updated(string entityName, object? data = null, string? customMessage = null) =>
        new()
        {
            IsSuccess = true,
            StatusCode = StatusCodes.Success200,
            Message = customMessage ?? $"{entityName} updated successfully.",
            Data = data
        };

    public static StandardResponse Deleted(string entityName, object? data = null) =>
        new()
        {
            IsSuccess = true,
            StatusCode = StatusCodes.Success200,
            Message = $"{entityName} deleted successfully.",
            Data = data
        };

    public static StandardResponse NotFound(string entityName, object? data = null) =>
        new()
        {
            IsSuccess = false,
            StatusCode = StatusCodes.NotFound404,
            Message = $"{entityName} not found.",
            Data = data
        };

    public static StandardResponse ValidationError(string message, List<ErrorDetails> errorList) =>
        new()
        {
            IsSuccess = false,
            StatusCode = StatusCodes.UnprocessableEntity422,
            Message = message ?? (errorList.FirstOrDefault()?.Message ?? string.Empty),
            Errors = errorList
        };

    public static StandardResponse FailedToCreate(string entityName, List<ErrorDetails> errorList) =>
        new()
        {
            IsSuccess = false,
            StatusCode = StatusCodes.ServerError500,
            Message = $"{entityName} could not be created at the moment. Please verify the information and try again.",
            Errors = errorList
        };

    public static StandardResponse FailedToUpdate(string entityName, List<ErrorDetails> errorList, string errorMessage) =>
        new()
        {
            IsSuccess = false,
            StatusCode = StatusCodes.UnprocessableEntity422,
            Message = errorMessage ?? string.Empty,
            Errors = errorList
        };

    public static StandardResponse Forbidden(string entityName, string? customMessage = null, object? data = null) =>
        new()
        {
            IsSuccess = false,
            StatusCode = StatusCodes.Forbidden403,
            Message = customMessage ?? $"{entityName} access is forbidden.",
            Data = data
        };

    public static StandardResponse Unauthorized(string entity, string? message = null, object? data = null) =>
        new()
        {
            IsSuccess = false,
            StatusCode = StatusCodes.Unauthorized401,
            Message = message ?? $"{entity}: You are not authorized.",
            Data = data
        };

    public static StandardResponse Conflict(string entityName, string? customMessage = null, object? data = null) =>
        new()
        {
            IsSuccess = false,
            StatusCode = StatusCodes.Conflict409,
            Message = customMessage ?? $"{entityName} already exists.",
            Data = data
        };
}
