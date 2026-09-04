using ITI.ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITI.ERP.Infrastructure.Persistence.Configurations;

public class BatchConfiguration : IEntityTypeConfiguration<Batch>
{
    public void Configure(EntityTypeBuilder<Batch> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Code)
            .HasMaxLength(20);

        builder.Property(b => b.StartDate)
            .IsRequired();

        builder.HasIndex(b => new { b.InstituteId, b.TradeId, b.StartAcademicSessionId, b.Code })
            .IsUnique()
            .HasFilter("\"Code\" IS NOT NULL AND \"IsDeleted\" = false");

        builder.HasIndex(b => b.InstituteId);
        builder.HasIndex(b => b.TradeId);
        builder.HasIndex(b => b.StartAcademicSessionId);
        builder.HasIndex(b => b.StartDate);

        builder.HasOne(b => b.Institute)
            .WithMany()
            .HasForeignKey(b => b.InstituteId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Trade)
            .WithMany()
            .HasForeignKey(b => b.TradeId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.StartAcademicSession)
            .WithMany()
            .HasForeignKey(b => b.StartAcademicSessionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(b => !b.IsDeleted);
    }
}
