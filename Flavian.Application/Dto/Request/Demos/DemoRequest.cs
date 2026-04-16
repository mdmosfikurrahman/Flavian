namespace Flavian.Application.Dto.Request.Demos;

public class DemoRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}