using Flavian.Domain.Models.Common;

namespace Flavian.Domain.Models.Demos;

public class Demo : BaseEntity
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public ICollection<DemoAudit> Audits { get; set; } = new List<DemoAudit>();
}