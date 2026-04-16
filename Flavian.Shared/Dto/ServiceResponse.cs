using System.Text.Json.Serialization;

namespace Flavian.Shared.Dto;

public class ServiceResponse<T>
{
    [JsonPropertyName("isSuccess")]
    public bool IsSuccess => Status is >= 200 and < 300 && (Errors == null || Errors.Count == 0);

    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public T? Data { get; set; }

    [JsonPropertyName("errors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ErrorDetails>? Errors { get; set; }

    private ServiceResponse(int status, string message, T? data = default, List<ErrorDetails>? errors = null)
    {
        Status = status;
        Message = message;
        Data = data;
        Errors = errors;
    }

    public static ServiceResponse<T> Success(int status, string message, T data) =>
        new(status, message, data);

    public static ServiceResponse<T> Error(int status, string message, List<ErrorDetails> errors) =>
        new(status, message, default, errors);
}
