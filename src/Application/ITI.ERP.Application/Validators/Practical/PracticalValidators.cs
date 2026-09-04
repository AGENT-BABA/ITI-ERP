using FluentValidation;
using ITI.ERP.Application.DTOs.Practical;

namespace ITI.ERP.Application.Validators.Practical;

public class CreateMonthlyPracticalRequestValidator : AbstractValidator<CreateMonthlyPracticalRequest>
{
    public CreateMonthlyPracticalRequestValidator()
    {
        RuleFor(x => x.TradeId)
            .NotEmpty();

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12);

        RuleFor(x => x.Year)
            .InclusiveBetween(2000, 2100);

        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(1, 200);

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.ProfessionalSkillName)
            .NotEmpty()
            .Length(1, 200);

        RuleFor(x => x.AssessorName)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.AssessorName));

        RuleFor(x => x.LearningOutcome)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.LearningOutcome));

        RuleFor(x => x)
            .Must(x => !x.StartDate.HasValue || !x.EndDate.HasValue || x.StartDate <= x.EndDate)
            .WithMessage("Date of Completion must be on or after Date of Starting.")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);
    }
}

public class SubmitPracticalMarksRequestValidator : AbstractValidator<SubmitPracticalMarksRequest>
{
    private const int MAX_SAFETY = 15;
    private const int MAX_HYGIENE = 10;
    private const int MAX_ATTENDANCE = 10;
    private const int MAX_INSTRUCTIONS = 5;
    private const int MAX_KNOWLEDGE = 10;
    private const int MAX_TOOLS = 10;
    private const int MAX_SPEED = 10;
    private const int MAX_QUALITY = 15;
    private const int MAX_VIVA = 15;

    public SubmitPracticalMarksRequestValidator()
    {
        RuleFor(x => x.MonthlyPracticalId)
            .NotEmpty();

        RuleFor(x => x.Students)
            .NotEmpty()
            .WithMessage("At least one student mark is required.");

        RuleFor(x => x.Students)
            .Must(students => students.Select(s => s.StudentId).Distinct().Count() == students.Count)
            .WithMessage("Duplicate student IDs are not allowed.");

        RuleForEach(x => x.Students).ChildRules(student =>
        {
            student.RuleFor(s => s.StudentId)
                .NotEmpty();

            student.RuleFor(s => s.SafetyConsciousness)
                .InclusiveBetween(0, MAX_SAFETY);
            student.RuleFor(s => s.WorkplaceHygiene)
                .InclusiveBetween(0, MAX_HYGIENE);
            student.RuleFor(s => s.AttendancePunctuality)
                .InclusiveBetween(0, MAX_ATTENDANCE);
            student.RuleFor(s => s.FollowInstructions)
                .InclusiveBetween(0, MAX_INSTRUCTIONS);
            student.RuleFor(s => s.ApplicationKnowledge)
                .InclusiveBetween(0, MAX_KNOWLEDGE);
            student.RuleFor(s => s.SkillsToolsEquipment)
                .InclusiveBetween(0, MAX_TOOLS);
            student.RuleFor(s => s.SpeedDoingWork)
                .InclusiveBetween(0, MAX_SPEED);
            student.RuleFor(s => s.QualityWorkmanship)
                .InclusiveBetween(0, MAX_QUALITY);
            student.RuleFor(s => s.Viva)
                .InclusiveBetween(0, MAX_VIVA);
        });
    }
}
