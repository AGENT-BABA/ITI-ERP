using ITI.ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITI.ERP.Infrastructure.Persistence.Configurations;

public class YearlyPracticalMarkConfiguration : IEntityTypeConfiguration<YearlyPracticalMark>
{
    public void Configure(EntityTypeBuilder<YearlyPracticalMark> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.InstituteId)
            .IsRequired();

        builder.Property(m => m.MarksObtained)
            .HasPrecision(8, 2);

        builder.Property(m => m.Remarks)
            .HasMaxLength(500);

        builder.Property(m => m.MonthlyManualEntries)
            .HasMaxLength(4000);

        builder.Property(m => m.AnnualTotal)
            .HasPrecision(10, 2);

        builder.Property(m => m.AnnualRemark)
            .HasMaxLength(500);

        builder.HasIndex(m => new { m.YearlyPracticalId, m.StudentId })
            .IsUnique();

        builder.HasOne(m => m.YearlyPractical)
            .WithMany()
            .HasForeignKey(m => m.YearlyPracticalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Student)
            .WithMany()
            .HasForeignKey(m => m.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.MarkedByUser)
            .WithMany()
            .HasForeignKey(m => m.MarkedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Institute>()
            .WithMany()
            .HasForeignKey(m => m.InstituteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => m.InstituteId);
    }
}
