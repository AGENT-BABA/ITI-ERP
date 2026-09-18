using ITI.ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITI.ERP.Infrastructure.Persistence.Configurations;

public class PracticalMarkConfiguration : IEntityTypeConfiguration<PracticalMark>
{
    public void Configure(EntityTypeBuilder<PracticalMark> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.InstituteId)
            .IsRequired();

        builder.Property(m => m.Remarks)
            .HasMaxLength(500);

        builder.HasIndex(m => new { m.MonthlyPracticalId, m.StudentId })
            .IsUnique();

        builder.HasOne(m => m.MonthlyPractical)
            .WithMany()
            .HasForeignKey(m => m.MonthlyPracticalId)
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
