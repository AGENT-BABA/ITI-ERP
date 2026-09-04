using ITI.ERP.Application.Common.Interfaces;

namespace ITI.ERP.Infrastructure.Services;

public class CalculationService : ICalculationService
{
    public decimal CalculatePercentage(int obtained, int total)
    {
        if (total == 0)
            return 0;

        return Math.Round((decimal)obtained / total * 100, 2);
    }

    public int CalculateWorkingDays(DateTime start, DateTime end, List<DateTime> holidays)
    {
        int workingDays = 0;
        var current = start.Date;

        while (current <= end.Date)
        {
            if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
            {
                if (!holidays.Contains(current.Date))
                {
                    workingDays++;
                }
            }
            current = current.AddDays(1);
        }

        return workingDays;
    }

    public decimal CalculateAttendancePercentage(int presentDays, int workingDays)
    {
        if (workingDays == 0)
            return 0;

        return Math.Round((decimal)presentDays / workingDays * 100, 2);
    }

    public int CalculateVacantSeats(int totalSeats, int enrolledStudents)
    {
        return Math.Max(0, totalSeats - enrolledStudents);
    }

    public decimal CalculateSeatOccupancy(int enrolledStudents, int totalSeats)
    {
        if (totalSeats == 0)
            return 0;

        return Math.Round((decimal)enrolledStudents / totalSeats * 100, 2);
    }
}