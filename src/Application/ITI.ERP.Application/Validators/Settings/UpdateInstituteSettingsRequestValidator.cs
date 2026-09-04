using FluentValidation;
using ITI.ERP.Application.DTOs.Settings;

namespace ITI.ERP.Application.Validators.Settings;

public class UpdateInstituteSettingsRequestValidator : AbstractValidator<UpdateInstituteSettingsRequest>
{
    public UpdateInstituteSettingsRequestValidator()
    {
        RuleFor(x => x.AttendanceThresholdPercentage)
            .InclusiveBetween(0, 100)
            .When(x => x.AttendanceThresholdPercentage.HasValue);

        RuleFor(x => x.PassMarksPercentage)
            .InclusiveBetween(0, 100)
            .When(x => x.PassMarksPercentage.HasValue);

        RuleFor(x => x.NotificationEmail)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.NotificationEmail));

        RuleFor(x => x.AcademicSessionFormat)
            .MaximumLength(20)
            .When(x => x.AcademicSessionFormat is not null);

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Website)
            .MaximumLength(200)
            .When(x => x.Website is not null);

        RuleFor(x => x.PrincipalName)
            .MaximumLength(200)
            .When(x => x.PrincipalName is not null);

        RuleFor(x => x.AffiliationNumber)
            .MaximumLength(100)
            .When(x => x.AffiliationNumber is not null);

        RuleFor(x => x.RecognitionNumber)
            .MaximumLength(100)
            .When(x => x.RecognitionNumber is not null);
    }
}
