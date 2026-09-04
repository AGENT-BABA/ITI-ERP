using ITI.ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITI.ERP.Infrastructure.Persistence.Configurations;

public class MonthlyPracticalConfiguration : IEntityTypeConfiguration<MonthlyPractical>
{
    public void Configure(EntityTypeBuilder<MonthlyPractical> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.AssessorName)
            .HasMaxLength(200);

        builder.Property(p => p.LearningOutcome)
            .HasMaxLength(500);

        builder.Property(p => p.ProfessionalSkillName)
            .HasMaxLength(200);

        builder.HasIndex(p => new { p.InstituteId, p.AcademicSessionId, p.TradeId, p.BatchId, p.Month, p.Year, p.ProfessionalSkillName })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false AND \"ProfessionalSkillName\" IS NOT NULL");
        builder.HasIndex(p => p.BatchId);

        builder.HasOne(p => p.Institute)
            .WithMany()
            .HasForeignKey(p => p.InstituteId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.AcademicSession)
            .WithMany()
            .HasForeignKey(p => p.AcademicSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Trade)
            .WithMany()
            .HasForeignKey(p => p.TradeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Batch)
            .WithMany()
            .HasForeignKey(p => p.BatchId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.LockedByUser)
            .WithMany()
            .HasForeignKey(p => p.LockedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
