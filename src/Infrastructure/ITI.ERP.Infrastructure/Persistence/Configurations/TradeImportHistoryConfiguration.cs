using ITI.ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITI.ERP.Infrastructure.Persistence.Configurations;

public class TradeImportHistoryConfiguration : IEntityTypeConfiguration<TradeImportHistory>
{
    public void Configure(EntityTypeBuilder<TradeImportHistory> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(t => t.NumberOfTradesImported)
            .IsRequired();

        builder.Property(t => t.Status)
            .IsRequired();

        builder.Property(t => t.ValidationErrors)
            .HasMaxLength(4000);

        builder.HasOne(t => t.Institute)
            .WithMany()
            .HasForeignKey(t => t.InstituteId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
