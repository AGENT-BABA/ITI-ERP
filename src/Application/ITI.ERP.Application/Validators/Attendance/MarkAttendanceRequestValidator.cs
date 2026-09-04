using FluentValidation;
using ITI.ERP.Application.DTOs.Attendance;

namespace ITI.ERP.Application.Validators.Attendance;

public class MarkAttendanceRequestValidator : AbstractValidator<MarkAttendanceRequest>
{
    public MarkAttendanceRequestValidator()
    {
        RuleFor(x => x.TradeId)
            .NotEmpty();

        RuleFor(x => x.Date)
            .NotEmpty()
            .LessThanOrEqualTo(DateTime.Today);

        RuleFor(x => x.Students)
            .NotEmpty()
            .WithMessage("At least one student attendance record is required.");

        RuleFor(x => x.Students)
            .Must(students => students.Select(s => s.StudentId).Distinct().Count() == students.Count)
            .WithMessage("Duplicate student IDs are not allowed.");

        RuleForEach(x => x.Students).ChildRules(student =>
        {
            student.RuleFor(s => s.StudentId)
                .NotEmpty();

            student.RuleFor(s => s.Status)
                .IsInEnum();
        });
    }
}
