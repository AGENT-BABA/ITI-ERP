using ITI.ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITI.ERP.Infrastructure.Persistence.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.MiddleName)
            .HasMaxLength(100);

        builder.Property(s => s.BloodGroup)
            .HasMaxLength(10);

        builder.Property(s => s.Phone)
            .HasMaxLength(15);

        builder.Property(s => s.Email)
            .HasMaxLength(200);

        builder.Property(s => s.Address)
            .HasMaxLength(500);

        builder.Property(s => s.City)
            .HasMaxLength(100);

        builder.Property(s => s.State)
            .HasMaxLength(100);

        builder.Property(s => s.PinCode)
            .HasMaxLength(10);

        builder.Property(s => s.FatherName)
            .HasMaxLength(200);

        builder.Property(s => s.MotherName)
            .HasMaxLength(200);

        builder.Property(s => s.GuardianPhone)
            .HasMaxLength(15);

        builder.Property(s => s.GuardianRelation)
            .HasMaxLength(50);

        builder.Property(s => s.RollNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(s => s.AdmissionNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(s => s.CasteCategory)
            .HasMaxLength(50);

        builder.Property(s => s.AllottedRound)
            .HasMaxLength(20);

        builder.Property(s => s.PreviousSchool)
            .HasMaxLength(200);

        builder.Property(s => s.PreviousQualification)
            .HasMaxLength(100);

        builder.Property(s => s.StatusReason)
            .HasMaxLength(500);

        builder.Property(s => s.AadharNumber)
            .HasMaxLength(12);

        builder.Property(s => s.PhotoPath)
            .HasMaxLength(500);

        builder.Property(s => s.EmergencyContactName)
            .HasMaxLength(200);

        builder.Property(s => s.EmergencyContactPhone)
            .HasMaxLength(15);

        builder.Property(s => s.EmergencyContactRelation)
            .HasMaxLength(50);

        builder.Property(s => s.AnnualIncome)
            .HasPrecision(12, 2);

        builder.Property(s => s.PreviousPercentage)
            .HasPrecision(5, 2);

        builder.HasIndex(s => s.TradeId);

        builder.HasIndex(s => new { s.InstituteId, s.AcademicSessionId, s.RollNumber })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(s => new { s.InstituteId, s.AcademicSessionId, s.AdmissionNumber })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasOne(s => s.Institute)
            .WithMany()
            .HasForeignKey(s => s.InstituteId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.AcademicSession)
            .WithMany()
            .HasForeignKey(s => s.AcademicSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Trade)
            .WithMany()
            .HasForeignKey(s => s.TradeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Batch)
            .WithMany()
            .HasForeignKey(s => s.BatchId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(s => !s.IsDeleted);

        builder.Property(s => s.RetentionUntil)
            .IsRequired(false);
    }
}
