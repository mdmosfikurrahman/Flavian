using System.Text.Json.Serialization;

namespace Flavian.Shared.Dto;

public class StandardResponse
{
    public bool IsSuccess { get; set; }
    public string StatusCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Data { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ErrorDetails>? Errors { get; set; }
}
