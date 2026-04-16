using FluentValidation;
using Flavian.Shared.Dto;

namespace Flavian.Application.Helpers;

public static class RequestValidator
{
    public static async Task<ServiceResponse<TResponse>?> Validate<TRequest, TResponse>(
        TRequest request, IValidator<TRequest> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return ServiceResponse<TResponse>.Error(400, "Validation failed",
                validationResult.Errors
                    .Select(e => new ErrorDetails(e.PropertyName, e.ErrorMessage)).ToList());
        }

        return null;
    }

    public static async Task<List<ErrorDetails>> ValidateAsync<T>(T request, IValidator<T> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return validationResult.Errors
                .Select(e => new ErrorDetails(e.PropertyName, e.ErrorMessage))
                .ToList();
        }

        return new List<ErrorDetails>();
    }
}
