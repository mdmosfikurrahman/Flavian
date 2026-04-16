using Flavian.Application.Dto.Request.Demos;
using Flavian.Application.Dto.Response.Demos;
using Flavian.Domain.Models.Demos;
using Flavian.Shared.Dto;
using Flavian.Shared.Utils.Audit;

namespace Flavian.Application.Mappers.Demos;

public static class DemoMapper
{
    public static Demo MapRequestToEntity(DemoRequest request, Guid createdBy) => new()
    {
        Name = request.Name,
        Description = request.Description,
        IsActive = request.IsActive,
        CreatedDate = DateTime.UtcNow,
        IsDeleted = false,
        CreatedBy = createdBy
    };

    public static DemoResponse MapEntityToResponse(Demo model) => new()
    {
        Id = model.Id,
        Name = model.Name,
        Description = model.Description,
        IsActive = model.IsActive
    };

    public static List<DemoResponse> MapEntityListToResponseList(IEnumerable<Demo> models) =>
        models.Select(MapEntityToResponse).ToList();

    public static Demo CloneEntity(Demo model) => new()
    {
        Id = model.Id,
        Name = model.Name,
        Description = model.Description,
        IsActive = model.IsActive,
        CreatedDate = model.CreatedDate,
        CreatedBy = model.CreatedBy,
        ModifiedDate = model.ModifiedDate,
        ModifiedBy = model.ModifiedBy,
        IsDeleted = model.IsDeleted
    };

    public static DemoAudit CreateDeleteAudit(Guid deletedBy) => new()
    {
        ActionName = FieldNames.AuditDelete,
        ActionDetails = AuditDetailsBuilder.BuildDeleteActionDetails(),
        CreatedBy = deletedBy,
        CreatedDate = DateTime.UtcNow
    };

    public static DemoAudit CreateAuditFromRequest(DemoRequest request, Demo entity, Guid createdBy) => new()
    {
        ActionName = FieldNames.AuditCreate,
        ActionDetails = AuditDetailsBuilder.BuildCreateActionDetails(entity),
        CreatedBy = createdBy,
        CreatedDate = DateTime.UtcNow
    };

    public static Demo MapRequestToUpdatedEntity(Demo existingDemo, DemoRequest request, Guid modifiedBy)
    {
        existingDemo.ModifiedBy = modifiedBy;
        existingDemo.ModifiedDate = DateTime.UtcNow;
        existingDemo.Name = request.Name;
        existingDemo.Description = request.Description;
        existingDemo.IsActive = request.IsActive;
        return existingDemo;
    }

    public static DemoAudit CreateUpdateAudit(Demo oldDemo, Demo updatedDemo) => new()
    {
        ActionName = FieldNames.AuditUpdate,
        ActionDetails = AuditDetailsBuilder.BuildUpdateActionDetails(oldDemo, updatedDemo),
        CreatedBy = updatedDemo.ModifiedBy ?? Guid.Empty,
        CreatedDate = DateTime.UtcNow
    };
}
