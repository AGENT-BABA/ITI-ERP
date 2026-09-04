using ITI.ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITI.ERP.Infrastructure.Persistence.Configurations;

public class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Remarks)
            .HasMaxLength(500);

        builder.HasIndex(a => new { a.InstituteId, a.AcademicSessionId, a.TradeId, a.Date });
        builder.HasIndex(a => new { a.InstituteId, a.AcademicSessionId, a.StudentId, a.Date })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
        builder.HasIndex(a => a.BatchId);

        builder.HasOne(a => a.Institute)
            .WithMany()
            .HasForeignKey(a => a.InstituteId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.AcademicSession)
            .WithMany()
            .HasForeignKey(a => a.AcademicSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Trade)
            .WithMany()
            .HasForeignKey(a => a.TradeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Batch)
            .WithMany()
            .HasForeignKey(a => a.BatchId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Student)
            .WithMany()
            .HasForeignKey(a => a.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.MarkedByUser)
            .WithMany()
            .HasForeignKey(a => a.MarkedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}
