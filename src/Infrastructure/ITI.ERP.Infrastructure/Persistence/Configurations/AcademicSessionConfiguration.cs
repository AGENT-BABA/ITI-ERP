using ITI.ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITI.ERP.Infrastructure.Persistence.Configurations;

public class AcademicSessionConfiguration : IEntityTypeConfiguration<AcademicSession>
{
    public void Configure(EntityTypeBuilder<AcademicSession> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.SessionYear)
            .IsRequired()
            .HasMaxLength(9);

        builder.Property(s => s.InstituteId)
            .IsRequired();

        builder.HasIndex(s => new { s.InstituteId, s.SessionYear })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}
