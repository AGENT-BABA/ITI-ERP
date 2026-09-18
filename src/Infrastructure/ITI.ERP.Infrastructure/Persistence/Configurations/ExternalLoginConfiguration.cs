using ITI.ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITI.ERP.Infrastructure.Persistence.Configurations;

public class ExternalLoginConfiguration : IEntityTypeConfiguration<ExternalLogin>
{
    public void Configure(EntityTypeBuilder<ExternalLogin> builder)
    {
        builder.HasKey(el => el.Id);

        builder.Property(el => el.Provider)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(el => el.ExternalUserId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(el => el.Email)
            .HasMaxLength(200);

        builder.HasIndex(el => new { el.Provider, el.ExternalUserId })
            .IsUnique();

        builder.HasOne(el => el.User)
            .WithMany()
            .HasForeignKey(el => el.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
