using FluentValidation;

namespace Flavian.Application.Dto.Request.Demos.Validator;

public class DemoRequestValidator : AbstractValidator<DemoRequest>
{
    public DemoRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty().WithMessage("Name cannot be empty.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

        RuleFor(request => request.Description)
            .MaximumLength(255).WithMessage("Description cannot exceed 255 characters.");
    }
}
