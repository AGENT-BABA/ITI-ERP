using ITI.ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITI.ERP.Infrastructure.Persistence.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.HasKey(ur => ur.Id);

        builder.HasIndex(ur => new { ur.UserId, ur.RoleId, ur.InstituteId })
            .IsUnique();

        builder.HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ur => ur.Institute)
            .WithMany()
            .HasForeignKey(ur => ur.InstituteId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ur => ur.AcademicSession)
            .WithMany()
            .HasForeignKey(ur => ur.AcademicSessionId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(ur => ur.Trade)
            .WithMany()
            .HasForeignKey(ur => ur.TradeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ur => ur.Batch)
            .WithMany()
            .HasForeignKey(ur => ur.BatchId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
