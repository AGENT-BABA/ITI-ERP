using FluentValidation;
using ITI.ERP.Application.DTOs.Student;

namespace ITI.ERP.Application.Validators.Student;

public class CreateStudentRequestValidator : AbstractValidator<CreateStudentRequest>
{
    public CreateStudentRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .Length(1, 100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .Length(1, 100);

        RuleFor(x => x.MiddleName)
            .MaximumLength(100);

        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .LessThan(DateTime.Today)
            .GreaterThanOrEqualTo(new DateTime(1950, 1, 1));

        RuleFor(x => x.Gender)
            .IsInEnum();

        RuleFor(x => x.BloodGroup)
            .MaximumLength(10)
            .When(x => !string.IsNullOrEmpty(x.BloodGroup));

        RuleFor(x => x.Phone)
            .NotEmpty()
            .Matches(@"^\d{10}$");

        RuleFor(x => x.Email)
            .EmailAddress()
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Address)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.City)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.City));

        RuleFor(x => x.State)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.State));

        RuleFor(x => x.PinCode)
            .Matches(@"^\d{6}$")
            .When(x => !string.IsNullOrEmpty(x.PinCode));

        RuleFor(x => x.TradeId)
            .NotEmpty();

        RuleFor(x => x.RollNumber)
            .NotEmpty()
            .Length(1, 20);

        RuleFor(x => x.AdmissionNumber)
            .NotEmpty()
            .Length(1, 20);

        RuleFor(x => x.AdmissionDate)
            .NotEmpty()
            .LessThanOrEqualTo(DateTime.Today);

        RuleFor(x => x.AadharNumber)
            .NotEmpty()
            .Matches(@"^\d{12}$");

        RuleFor(x => x.FatherName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.MotherName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.GuardianPhone)
            .NotEmpty()
            .Matches(@"^\d{10}$");

        RuleFor(x => x.EmergencyContactPhone)
            .Matches(@"^\d{10}$")
            .When(x => !string.IsNullOrEmpty(x.EmergencyContactPhone));
    }
}
