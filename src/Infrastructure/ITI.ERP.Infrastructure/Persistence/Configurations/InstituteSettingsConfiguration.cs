using ITI.ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITI.ERP.Infrastructure.Persistence.Configurations;

public class InstituteSettingsConfiguration : IEntityTypeConfiguration<InstituteSettings>
{
    public void Configure(EntityTypeBuilder<InstituteSettings> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.InstituteId)
            .IsRequired();

        builder.HasIndex(s => s.InstituteId)
            .IsUnique();

        builder.HasOne(s => s.Institute)
            .WithMany()
            .HasForeignKey(s => s.InstituteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(s => s.NotificationEmail)
            .HasMaxLength(200);

        builder.Property(s => s.AcademicSessionFormat)
            .HasMaxLength(20);

        builder.Property(s => s.LogoPath)
            .HasMaxLength(500);

        builder.Property(s => s.Address)
            .HasMaxLength(500);

        builder.Property(s => s.City)
            .HasMaxLength(100);

        builder.Property(s => s.State)
            .HasMaxLength(100);

        builder.Property(s => s.Phone)
            .HasMaxLength(20);

        builder.Property(s => s.Email)
            .HasMaxLength(200);

        builder.Property(s => s.Website)
            .HasMaxLength(200);

        builder.Property(s => s.PrincipalName)
            .HasMaxLength(200);

        builder.Property(s => s.AffiliationNumber)
            .HasMaxLength(100);

        builder.Property(s => s.RecognitionNumber)
            .HasMaxLength(100);
    }
}
