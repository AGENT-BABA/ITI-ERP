using ITI.ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITI.ERP.Infrastructure.Persistence.Configurations;

public class YearlyPracticalConfiguration : IEntityTypeConfiguration<YearlyPractical>
{
    public void Configure(EntityTypeBuilder<YearlyPractical> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.HasIndex(p => new { p.InstituteId, p.AcademicSessionId, p.TradeId, p.BatchId, p.Year })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
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
