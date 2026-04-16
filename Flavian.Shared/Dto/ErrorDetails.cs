namespace Flavian.Shared.Dto;

public class ErrorDetails(string field, string message)
{
    public string Field { get; set; } = field;
    public string Message { get; set; } = message;
}
