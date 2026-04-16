using Flavian.Domain.Attributes;

namespace Flavian.Domain.Models.Common;

public abstract class BaseEntity
{
    [AuditIgnore] public Guid Id { get; set; } = Guid.CreateVersion7();
    [AuditIgnore] public long RowId { get; private set; }
    [AuditIgnore] public Guid CreatedBy { get; set; }
    [AuditIgnore] public int BusinessUnit { get; set; }
    [AuditIgnore] public DateTime CreatedDate { get; set; }
    [AuditIgnore] public Guid? ModifiedBy { get; set; }
    [AuditIgnore] public DateTime? ModifiedDate { get; set; }
    [AuditIgnore] public bool IsDeleted { get; set; } = false;
}