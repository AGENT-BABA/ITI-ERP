using FluentAssertions;
using ITI.ERP.Application.Services;
using ITI.ERP.Domain.Entities;
using ITI.ERP.Domain.Enums;
using Xunit;

namespace ITI.ERP.Application.Tests.Services;

public class BatchYearLevelCalculatorTests
{
    private static Trade CreateTrade(int durationInMonths = 24) => new()
    {
        Id = Guid.NewGuid(),
        Name = "COPA",
        Code = "COPA",
        DurationInMonths = durationInMonths,
        TotalSeats = 40,
        AcademicSessionId = Guid.NewGuid(),
        InstituteId = Guid.NewGuid(),
        DraftStatus = DraftStatus.Finalized
    };

    private static Batch CreateBatch(DateTime startDate, Guid? tradeId = null, Guid? startSessionId = null) => new()
    {
        Id = Guid.NewGuid(),
        Name = "COPA-A",
        StartDate = startDate,
        TradeId = tradeId ?? Guid.NewGuid(),
        StartAcademicSessionId = startSessionId ?? Guid.NewGuid(),
        InstituteId = Guid.NewGuid(),
        IsActive = true
    };

    private static AcademicSession CreateSession(DateTime startDate, DateTime endDate) => new()
    {
        Id = Guid.NewGuid(),
        SessionYear = $"{startDate.Year}-{endDate.Year}",
        StartDate = startDate,
        EndDate = endDate,
        IsActive = true,
        InstituteId = Guid.NewGuid()
    };

    [Fact]
    public void August_Intake_2YearCourse_FirstSession_ReturnsFirstYear()
    {
        var trade = CreateTrade(24);
        var batch = CreateBatch(new DateTime(2025, 8, 15), trade.Id);
        var session = CreateSession(new DateTime(2025, 7, 1), new DateTime(2026, 6, 30));

        var result = BatchYearLevelCalculator.Calculate(batch, trade, session);

        result.Status.Should().Be(BatchComputedStatus.FirstYear);
        result.YearLevel.Should().Be(YearLevel.FirstYear);
        result.Label.Should().Be("First Year");
    }

    [Fact]
    public void August_Intake_2YearCourse_SecondSession_ReturnsSecondYear()
    {
        var trade = CreateTrade(24);
        var batch = CreateBatch(new DateTime(2025, 8, 15), trade.Id);
        var session = CreateSession(new DateTime(2026, 7, 1), new DateTime(2027, 6, 30));

        var result = BatchYearLevelCalculator.Calculate(batch, trade, session);

        result.Status.Should().Be(BatchComputedStatus.SecondYear);
        result.YearLevel.Should().Be(YearLevel.SecondYear);
        result.Label.Should().Be("Second Year");
    }

    [Fact]
    public void August_Intake_2YearCourse_ThirdSession_ReturnsCompleted()
    {
        var trade = CreateTrade(24);
        var batch = CreateBatch(new DateTime(2025, 8, 15), trade.Id);
        var session = CreateSession(new DateTime(2027, 7, 1), new DateTime(2028, 6, 30));

        var result = BatchYearLevelCalculator.Calculate(batch, trade, session);

        result.Status.Should().Be(BatchComputedStatus.Completed);
        result.YearLevel.Should().BeNull();
        result.Label.Should().Be("Completed");
    }

    [Fact]
    public void September_Intake_2YearCourse_FirstSession_ReturnsFirstYear()
    {
        var trade = CreateTrade(24);
        var batch = CreateBatch(new DateTime(2025, 9, 10), trade.Id);
        var session = CreateSession(new DateTime(2025, 7, 1), new DateTime(2026, 6, 30));

        var result = BatchYearLevelCalculator.Calculate(batch, trade, session);

        result.Status.Should().Be(BatchComputedStatus.FirstYear);
        result.YearLevel.Should().Be(YearLevel.FirstYear);
    }

    [Fact]
    public void September_Intake_2YearCourse_SecondSession_ReturnsSecondYear()
    {
        var trade = CreateTrade(24);
        var batch = CreateBatch(new DateTime(2025, 9, 10), trade.Id);
        var session = CreateSession(new DateTime(2026, 7, 1), new DateTime(2027, 6, 30));

        var result = BatchYearLevelCalculator.Calculate(batch, trade, session);

        result.Status.Should().Be(BatchComputedStatus.SecondYear);
        result.YearLevel.Should().Be(YearLevel.SecondYear);
    }

    [Fact]
    public void October_Intake_2YearCourse_FirstSession_ReturnsFirstYear()
    {
        var trade = CreateTrade(24);
        var batch = CreateBatch(new DateTime(2025, 10, 1), trade.Id);
        var session = CreateSession(new DateTime(2025, 7, 1), new DateTime(2026, 6, 30));

        var result = BatchYearLevelCalculator.Calculate(batch, trade, session);

        result.Status.Should().Be(BatchComputedStatus.FirstYear);
        result.YearLevel.Should().Be(YearLevel.FirstYear);
    }

    [Fact]
    public void October_Intake_2YearCourse_ThirdSession_ReturnsCompleted()
    {
        var trade = CreateTrade(24);
        var batch = CreateBatch(new DateTime(2025, 10, 1), trade.Id);
        var session = CreateSession(new DateTime(2027, 7, 1), new DateTime(2028, 6, 30));

        var result = BatchYearLevelCalculator.Calculate(batch, trade, session);

        result.Status.Should().Be(BatchComputedStatus.Completed);
        result.YearLevel.Should().BeNull();
    }

    [Fact]
    public void January_Intake_1YearCourse_SameYear_ReturnsFirstYear()
    {
        var trade = CreateTrade(12);
        var batch = CreateBatch(new DateTime(2026, 1, 15), trade.Id);
        var session = CreateSession(new DateTime(2025, 7, 1), new DateTime(2026, 6, 30));

        var result = BatchYearLevelCalculator.Calculate(batch, trade, session);

        result.Status.Should().Be(BatchComputedStatus.FirstYear);
        result.YearLevel.Should().Be(YearLevel.FirstYear);
    }

    [Fact]
    public void January_Intake_1YearCourse_NextSession_ReturnsCompleted()
    {
        var trade = CreateTrade(12);
        var batch = CreateBatch(new DateTime(2026, 1, 15), trade.Id);
        var session = CreateSession(new DateTime(2026, 7, 1), new DateTime(2027, 6, 30));

        var result = BatchYearLevelCalculator.Calculate(batch, trade, session);

        result.Status.Should().Be(BatchComputedStatus.Completed);
        result.YearLevel.Should().BeNull();
    }

    [Fact]
    public void FutureBatch_BeforeStartDate_ReturnsNotStarted()
    {
        var trade = CreateTrade(24);
        var batch = CreateBatch(new DateTime(2027, 8, 1), trade.Id);
        var session = CreateSession(new DateTime(2025, 7, 1), new DateTime(2026, 6, 30));

        var result = BatchYearLevelCalculator.Calculate(batch, trade, session);

        result.Status.Should().Be(BatchComputedStatus.NotStarted);
        result.YearLevel.Should().BeNull();
        result.Label.Should().Be("Not Started");
    }

    [Fact]
    public void BatchStartsDuringSession_ReturnsFirstYear()
    {
        var trade = CreateTrade(24);
        var batch = CreateBatch(new DateTime(2026, 3, 1), trade.Id);
        var session = CreateSession(new DateTime(2025, 7, 1), new DateTime(2026, 6, 30));

        var result = BatchYearLevelCalculator.Calculate(batch, trade, session);

        result.Status.Should().Be(BatchComputedStatus.FirstYear);
    }

    [Fact]
    public void BatchStartsAfterSessionEnd_ReturnsNotStarted()
    {
        var trade = CreateTrade(24);
        var batch = CreateBatch(new DateTime(2026, 8, 1), trade.Id);
        var session = CreateSession(new DateTime(2025, 7, 1), new DateTime(2026, 6, 30));

        var result = BatchYearLevelCalculator.Calculate(batch, trade, session);

        result.Status.Should().Be(BatchComputedStatus.NotStarted);
    }

    [Fact]
    public void MultipleIntakesInSameSession_SameBatch_SameYearLevel()
    {
        var trade = CreateTrade(24);

        var augBatch = CreateBatch(new DateTime(2025, 8, 1), trade.Id);
        var sepBatch = CreateBatch(new DateTime(2025, 9, 1), trade.Id);
        var octBatch = CreateBatch(new DateTime(2025, 10, 1), trade.Id);

        var session = CreateSession(new DateTime(2025, 7, 1), new DateTime(2026, 6, 30));

        var augResult = BatchYearLevelCalculator.Calculate(augBatch, trade, session);
        var sepResult = BatchYearLevelCalculator.Calculate(sepBatch, trade, session);
        var octResult = BatchYearLevelCalculator.Calculate(octBatch, trade, session);

        augResult.Status.Should().Be(BatchComputedStatus.FirstYear);
        sepResult.Status.Should().Be(BatchComputedStatus.FirstYear);
        octResult.Status.Should().Be(BatchComputedStatus.FirstYear);
    }

    [Fact]
    public void MultipleIntakesInSameSession_SecondYear_AllSecondYear()
    {
        var trade = CreateTrade(24);

        var augBatch = CreateBatch(new DateTime(2025, 8, 1), trade.Id);
        var sepBatch = CreateBatch(new DateTime(2025, 9, 1), trade.Id);

        var session = CreateSession(new DateTime(2026, 7, 1), new DateTime(2027, 6, 30));

        var augResult = BatchYearLevelCalculator.Calculate(augBatch, trade, session);
        var sepResult = BatchYearLevelCalculator.Calculate(sepBatch, trade, session);

        augResult.Status.Should().Be(BatchComputedStatus.SecondYear);
        sepResult.Status.Should().Be(BatchComputedStatus.SecondYear);
    }

    [Fact]
    public void MidCourseBatch_StillActive()
    {
        var trade = CreateTrade(24);
        var batch = CreateBatch(new DateTime(2025, 8, 15), trade.Id);

        var session = CreateSession(new DateTime(2025, 7, 1), new DateTime(2026, 6, 30));
        var result = BatchYearLevelCalculator.Calculate(batch, trade, session);
        result.Status.Should().Be(BatchComputedStatus.FirstYear);

        var session2 = CreateSession(new DateTime(2026, 7, 1), new DateTime(2027, 6, 30));
        var result2 = BatchYearLevelCalculator.Calculate(batch, trade, session2);
        result2.Status.Should().Be(BatchComputedStatus.SecondYear);
    }

    [Fact]
    public void HistoricalReport_CompletedBatch_CanStillBeRetrieved()
    {
        var trade = CreateTrade(24);
        var batch = CreateBatch(new DateTime(2025, 8, 1), trade.Id);

        var historicalSession = CreateSession(new DateTime(2025, 7, 1), new DateTime(2026, 6, 30));
        var result = BatchYearLevelCalculator.Calculate(batch, trade, historicalSession);
        result.Status.Should().Be(BatchComputedStatus.FirstYear);

        var currentSession = CreateSession(new DateTime(2027, 7, 1), new DateTime(2028, 6, 30));
        var currentResult = BatchYearLevelCalculator.Calculate(batch, trade, currentSession);
        currentResult.Status.Should().Be(BatchComputedStatus.Completed);
    }

    [Fact]
    public void IsBatchActiveInSession_ReturnsTrueForActiveBatches()
    {
        var trade = CreateTrade(24);
        var batch = CreateBatch(new DateTime(2025, 8, 1), trade.Id);
        var session = CreateSession(new DateTime(2025, 7, 1), new DateTime(2026, 6, 30));

        BatchYearLevelCalculator.IsBatchActiveInSession(batch, trade, session).Should().BeTrue();
    }

    [Fact]
    public void IsBatchActiveInSession_ReturnsFalseForCompletedBatches()
    {
        var trade = CreateTrade(24);
        var batch = CreateBatch(new DateTime(2025, 8, 1), trade.Id);
        var session = CreateSession(new DateTime(2027, 7, 1), new DateTime(2028, 6, 30));

        BatchYearLevelCalculator.IsBatchActiveInSession(batch, trade, session).Should().BeFalse();
    }

    [Fact]
    public void IsBatchActiveInSession_ReturnsFalseForFutureBatches()
    {
        var trade = CreateTrade(24);
        var batch = CreateBatch(new DateTime(2027, 8, 1), trade.Id);
        var session = CreateSession(new DateTime(2025, 7, 1), new DateTime(2026, 6, 30));

        BatchYearLevelCalculator.IsBatchActiveInSession(batch, trade, session).Should().BeFalse();
    }

    [Fact]
    public void BatchStartDateExactlyOnSessionStart_ReturnsFirstYear()
    {
        var trade = CreateTrade(24);
        var batch = CreateBatch(new DateTime(2025, 7, 1), trade.Id);
        var session = CreateSession(new DateTime(2025, 7, 1), new DateTime(2026, 6, 30));

        var result = BatchYearLevelCalculator.Calculate(batch, trade, session);

        result.Status.Should().Be(BatchComputedStatus.FirstYear);
    }

    [Fact]
    public void BatchStartDateExactlyAtSessionEnd_ReturnsFirstYear()
    {
        var trade = CreateTrade(24);
        var batch = CreateBatch(new DateTime(2026, 6, 30), trade.Id);
        var session = CreateSession(new DateTime(2025, 7, 1), new DateTime(2026, 6, 30));

        var result = BatchYearLevelCalculator.Calculate(batch, trade, session);

        result.Status.Should().Be(BatchComputedStatus.FirstYear);
    }
}
