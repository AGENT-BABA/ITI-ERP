using ITI.ERP.Domain.Entities;
using ITI.ERP.Domain.Enums;

namespace ITI.ERP.Application.Services;

public static class BatchYearLevelCalculator
{
    public record YearLevelResult(BatchComputedStatus Status, YearLevel? YearLevel, string Label);

    public static YearLevelResult Calculate(Batch batch, Trade trade, AcademicSession currentSession)
    {
        var batchStartDate = batch.StartDate;
        var sessionEndDate = currentSession.EndDate;

        var totalMonths = (sessionEndDate.Year - batchStartDate.Year) * 12
                        + (sessionEndDate.Month - batchStartDate.Month);

        if (totalMonths < 0)
        {
            return new YearLevelResult(BatchComputedStatus.NotStarted, null, "Not Started");
        }

        var yearIndex = totalMonths / 12;
        var totalYears = (int)Math.Ceiling(trade.DurationInMonths / 12.0);

        if (yearIndex >= totalYears)
        {
            return new YearLevelResult(BatchComputedStatus.Completed, null, "Completed");
        }

        var level = yearIndex + 1;
        return level switch
        {
            1 => new YearLevelResult(BatchComputedStatus.FirstYear, Domain.Enums.YearLevel.FirstYear, "First Year"),
            2 => new YearLevelResult(BatchComputedStatus.SecondYear, Domain.Enums.YearLevel.SecondYear, "Second Year"),
            _ => new YearLevelResult(BatchComputedStatus.Completed, null, "Completed")
        };
    }

    public static bool IsBatchActiveInSession(Batch batch, Trade trade, AcademicSession session)
    {
        var result = Calculate(batch, trade, session);
        return result.Status == BatchComputedStatus.FirstYear
            || result.Status == BatchComputedStatus.SecondYear;
    }
}
