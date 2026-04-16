namespace Flavian.Domain.Models.Common;

public abstract class BaseAuditEntity
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid EventId { get; set; }
    public string ActionName { get; set; } = default!;
    public string ActionDetails { get; set; } = default!;
    public Guid CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public long RowId { get; private set; }
}