using FluentValidation;
using Flavian.Application.Dto.Request.Demos;
using Flavian.Application.Dto.Response.Demos;
using Flavian.Application.Helpers;
using Flavian.Application.Mappers.Demos;
using Flavian.Application.Services.Interfaces.Auth;
using Flavian.Application.Services.Interfaces.Demos;
using Flavian.Persistence.UoW.Interface;
using Flavian.Shared.Dto;
using static Flavian.Shared.Dto.ApiResponseHelper;

namespace Flavian.Application.Services.Implementations.Demos;

public class DemoService(
    IUnitOfWork unitOfWork,
    IValidator<DemoRequest> validator,
    IUserContext userContext) : IDemoService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IValidator<DemoRequest> _validator = validator;
    private readonly IUserContext _user = userContext;

    private const string EntityName = CommonStrings.EntityDemo;

    public async Task<StandardResponse> GetAllAsync(int? pageNumber, int? pageSize)
    {
        var (page, size) = ServiceHelpers.NormalizePaging(pageNumber, pageSize);

        var result = await _unitOfWork.Demos.GetAllAsync(page, size);

        if (result.Data?.Any() != true)
            return NotFound($"{EntityName}s", result);

        var mappedData = DemoMapper.MapEntityListToResponseList(result.Data);

        var response = new PaginationResponse<DemoResponse>
        {
            Data = mappedData,
            TotalRecords = result.TotalRecords,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize
        };

        return Success($"{EntityName}s", response);
    }

    public async Task<StandardResponse> GetByIdAsync(Guid id)
    {
        var demo = await _unitOfWork.Demos.FindByIdAsync(id);
        if (demo is null)
            return NotFound(string.Format(CommonStrings.NotFoundByIdTemplate, EntityName, id));

        var response = DemoMapper.MapEntityToResponse(demo);
        return Success(EntityName, response);
    }

    public async Task<StandardResponse> AddAsync(DemoRequest request)
    {
        var validationErrors = await ServiceHelpers.ValidateAsync(request, _validator);
        if (validationErrors.Count > 0)
            return ValidationError(EntityName, validationErrors);

        var createdBy = _user.PartnerId;
        var entity = DemoMapper.MapRequestToEntity(request, createdBy);
        var audit = DemoMapper.CreateAuditFromRequest(request, entity, createdBy);
        entity.Audits.Add(audit);

        return await ServiceHelpers.ExecuteInTransactionAsync(EntityName, _unitOfWork, async () =>
        {
            await _unitOfWork.Demos.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            var response = DemoMapper.MapEntityToResponse(entity);
            return Created(EntityName, response);
        });
    }

    public async Task<StandardResponse> UpdateAsync(Guid id, DemoRequest request)
    {
        var validationErrors = await ServiceHelpers.ValidateAsync(request, _validator);
        if (validationErrors.Count > 0)
            return ValidationError(EntityName, validationErrors);

        var existing = await _unitOfWork.Demos.FindByIdAsync(id);
        if (existing is null)
            return NotFound(string.Format(CommonStrings.NotFoundByIdTemplate, EntityName, id));

        var modifiedBy = _user.PartnerId;

        var original = DemoMapper.CloneEntity(existing);
        var updated = DemoMapper.MapRequestToUpdatedEntity(existing, request, modifiedBy);

        var audit = DemoMapper.CreateUpdateAudit(original, updated);
        updated.Audits.Add(audit);

        return await ServiceHelpers.ExecuteInTransactionAsync(EntityName, _unitOfWork, async () =>
        {
            await _unitOfWork.Demos.UpdateAsync(updated);
            await _unitOfWork.SaveChangesAsync();

            var response = DemoMapper.MapEntityToResponse(updated);
            return Updated(EntityName, response);
        });
    }

    public async Task<StandardResponse> DeleteAsync(Guid id)
    {
        var existing = await _unitOfWork.Demos.FindByIdAsync(id);
        if (existing is null)
            return NotFound(string.Format(CommonStrings.NotFoundByIdTemplate, EntityName, id));

        var deletedBy = _user.PartnerId;
        var audit = DemoMapper.CreateDeleteAudit(deletedBy);
        existing.Audits.Add(audit);

        return await ServiceHelpers.ExecuteInTransactionAsync(EntityName, _unitOfWork, async () =>
        {
            await _unitOfWork.Demos.DeleteByIdAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return Deleted(EntityName);
        });
    }
}
