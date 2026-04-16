namespace Flavian.Application.Dto.Common;

public class ServiceInfoOptions
{
    public Dictionary<string, ServiceInfoItem> Services { get; set; } = [];
}

public class ServiceInfoItem
{
    public string BaseUrl { get; set; } = string.Empty;
    public string? StatusEndpoint { get; set; }
    public bool ExposeStatusData { get; set; } = false;
}

public class AppInfoOptions
{
    public string EnvironmentName { get; set; } = string.Empty;
    public string ConfigSource { get; set; } = string.Empty;
}