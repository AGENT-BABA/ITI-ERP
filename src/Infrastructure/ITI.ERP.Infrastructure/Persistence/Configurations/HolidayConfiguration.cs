using ITI.ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITI.ERP.Infrastructure.Persistence.Configurations;

public class HolidayConfiguration : IEntityTypeConfiguration<Holiday>
{
    public void Configure(EntityTypeBuilder<Holiday> builder)
    {
        builder.HasKey(h => h.Id);

        builder.Property(h => h.Date)
            .IsRequired();

        builder.Property(h => h.Name)
            .HasMaxLength(200);

        builder.Property(h => h.InstituteId)
            .IsRequired();

        builder.HasIndex(h => new { h.InstituteId, h.AcademicSessionId, h.Date })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasQueryFilter(h => !h.IsDeleted);
    }
}
