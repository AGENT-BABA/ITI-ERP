using ITI.ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITI.ERP.Infrastructure.Persistence.Configurations;

public class StudentImportHistoryConfiguration : IEntityTypeConfiguration<StudentImportHistory>
{
    public void Configure(EntityTypeBuilder<StudentImportHistory> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(s => s.NumberOfStudentsImported)
            .IsRequired();

        builder.Property(s => s.Status)
            .IsRequired();

        builder.Property(s => s.ValidationErrors)
            .HasMaxLength(4000);

        builder.HasOne(s => s.Institute)
            .WithMany()
            .HasForeignKey(s => s.InstituteId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Trade)
            .WithMany()
            .HasForeignKey(s => s.TradeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.AcademicSession)
            .WithMany()
            .HasForeignKey(s => s.AcademicSessionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
