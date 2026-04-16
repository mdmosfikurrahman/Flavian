namespace Flavian.Application.Dto.Request.Demos;

public class DemoAuditRequest
{
    public Guid EventId { get; set; }
    public string ActionName { get; set; } = string.Empty;
    public string? ActionDetails { get; set; }
}
