using ITI.ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITI.ERP.Infrastructure.Persistence.Configurations;

public class InstituteConfiguration : IEntityTypeConfiguration<Institute>
{
    public void Configure(EntityTypeBuilder<Institute> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.GRNumber)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasIndex(i => i.GRNumber)
            .IsUnique();

        builder.Property(i => i.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.District)
            .HasMaxLength(100);
    }
}
