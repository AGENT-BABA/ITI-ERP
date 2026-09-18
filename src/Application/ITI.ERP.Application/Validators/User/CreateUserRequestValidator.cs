using FluentValidation;
using ITI.ERP.Application.DTOs.User;

namespace ITI.ERP.Application.Validators.User;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .Length(3, 50)
            .Matches(@"^[a-zA-Z0-9._@-]+$")
            .WithMessage("Username must be alphanumeric.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Invalid email address.")
            .MaximumLength(200);

        RuleFor(x => x.Password)
            .NotEmpty()
            .Length(8, 128)
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#])[A-Za-z\d@$!%*?&#]+$")
            .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .Length(1, 100);

        RuleFor(x => x.LastName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.LastName));

        RuleFor(x => x.Phone)
            .Matches(@"^\d{10}$")
            .When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Phone must be exactly 10 digits.");

        RuleFor(x => x.RoleIds)
            .NotEmpty()
            .WithMessage("At least one role must be assigned.");
    }
}
