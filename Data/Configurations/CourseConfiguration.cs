using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;

namespace TmsApi.Data.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.HasKey(c => c.Id); // Configures 'Id' as the primary key [2]

        // Enforce maximum string lengths and required constraints
        builder.Property(c => c.Code).HasMaxLength(20).IsRequired();
        builder.Property(c => c.Title).HasMaxLength(200).IsRequired();
        builder.Property(c => c.MaxCapacity).IsRequired(); // 
        builder.HasIndex(c => c.Code).IsUnique(); // 
    }
}