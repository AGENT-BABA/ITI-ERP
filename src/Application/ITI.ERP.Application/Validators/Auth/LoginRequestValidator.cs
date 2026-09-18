using FluentValidation;
using ITI.ERP.Application.DTOs.Auth;

namespace ITI.ERP.Application.Validators.Auth;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.GRNumber)
            .NotEmpty()
            .Matches(@"^[A-Za-z0-9]{1,10}$")
            .WithMessage("GR Number must be alphanumeric.");

        RuleFor(x => x.Username)
            .NotEmpty()
            .Matches(@"^[a-zA-Z0-9._@-]{3,50}$")
            .WithMessage("Username must be alphanumeric.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .Length(8, 128);
    }
}
