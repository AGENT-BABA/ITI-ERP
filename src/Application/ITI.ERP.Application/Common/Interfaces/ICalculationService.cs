namespace ITI.ERP.Application.Common.Interfaces;

public interface ICalculationService
{
    decimal CalculatePercentage(int obtained, int total);
    int CalculateWorkingDays(DateTime start, DateTime end, List<DateTime> holidays);
    decimal CalculateAttendancePercentage(int presentDays, int workingDays);
    int CalculateVacantSeats(int totalSeats, int enrolledStudents);
    decimal CalculateSeatOccupancy(int enrolledStudents, int totalSeats);
}
