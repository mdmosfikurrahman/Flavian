using Flavian.Shared.Dto;

namespace Flavian.Shared.Exceptions;

public class ValidationException : Exception
{
    public List<ErrorDetails> Errors { get; }

    public ValidationException(List<ErrorDetails> errors)
        : base("Validation failed")
    {
        Errors = errors;
    }

    public ValidationException(string field, string message)
        : this([new ErrorDetails(field, message)])
    {
    }

    public ValidationException(string message)
        : this([new ErrorDetails("Validation", message)])
    {
    }

    public ValidationException(string message, List<ErrorDetails> errors)
        : base(message)
    {
        Errors = errors;
    }
}
