using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Domain.Entities;// 👈 FIX: Connects your Student entity class - Page 2

namespace TmsApi.Application.Application.Data.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(s => s.Id); // Configures 'Id' as the primary key [2]

        // Enforce maximum string lengths and required constraints [2]
        builder.Property(s => s.RegistrationNumber).HasMaxLength(50).IsRequired();
        builder.Property(s => s.Name).HasMaxLength(100).IsRequired();
        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}