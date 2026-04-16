using FluentValidation;
using Flavian.Persistence.UoW.Interface;
using Flavian.Shared.Dto;
using static Flavian.Shared.Dto.ApiResponseHelper;

namespace Flavian.Application.Helpers;

public static class ServiceHelpers
{
    public static (int page, int size) NormalizePaging(int? pageNumber, int? pageSize) =>
    (
        pageNumber is null or <= 0 ? FieldNames.DefaultPageNumber : pageNumber.Value,
        pageSize is null or <= 0 ? FieldNames.DefaultPageSize : pageSize.Value
    );

    private static string ExtractExceptionMessage(Exception ex) =>
        ex?.InnerException?.Message ?? ex?.Message ?? string.Empty;

    public static async Task<List<ErrorDetails>> ValidateAsync<TRequest>(
        TRequest request,
        IValidator<TRequest> validator)
    {
        var errors = await RequestValidator.ValidateAsync(request, validator);
        return errors.ToList();
    }

    public static async Task<StandardResponse> ExecuteInTransactionAsync(
        string entityName,
        IUnitOfWork unitOfWork,
        Func<Task<StandardResponse>> action)
    {
        await unitOfWork.BeginTransactionAsync();
        try
        {
            var response = await action();
            await unitOfWork.CommitTransactionAsync();
            return response;
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            return FailedToCreate(entityName,
            [
                new ErrorDetails(FieldNames.TryCatchBlock, ExtractExceptionMessage(ex))
            ]);
        }
    }
}
