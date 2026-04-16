namespace Flavian.Application.Dto.Response.Demos;

public class DemoAuditResponse
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string ActionName { get; set; } = string.Empty;
    public string? ActionDetails { get; set; }
}
