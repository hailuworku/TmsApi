using Microsoft.EntityFrameworkCore; // 👈 FIX: Connects Entity Framework [113]
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TmsApi.Domain.Entities; // 👈 FIX: Connects Student, Course, and Enrollment [113]

namespace TmsApi.Infrastructure.Persistence;

public class TmsDbContext : DbContext
{
    public TmsDbContext(DbContextOptions<TmsDbContext> options) : base(options) { }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<Certificate> Certificates => Set<Certificate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Automatically discovers and applies all configurations (IEntityTypeConfiguration) - Page 2
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TmsDbContext).Assembly);

        // 2. Configure advanced Student mappings - Page 4, 5
        modelBuilder.Entity<Student>(builder =>
        {
            // Exercise 8: Configure shadow property for audit trail stamp [4]
            builder.Property<DateTime>("LastUpdated");

            // Exercise 8: Configure system row version (xmin) for concurrency checks [4, 5]
            builder.Property(s => s.Version).IsRowVersion();

            // Exercise 9: Configure global soft-delete query filter [5]
            builder.HasQueryFilter(s => !s.IsDeleted);
        });
        modelBuilder.Entity<Certificate>().HasQueryFilter(c => !c.Student.IsDeleted);
        modelBuilder.Entity<Enrollment>().HasQueryFilter(e => !e.Student.IsDeleted);
        modelBuilder.Entity<Assessment>().HasQueryFilter(a => !a.Course.Enrollments.Any(e => e.Student.IsDeleted));
    }

    // --- Automatic Audit Stamping --- - Page 4
    public override int SaveChanges()
    {
        StampAuditProperties();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampAuditProperties();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void StampAuditProperties()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Student && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
            entry.Property("LastUpdated").CurrentValue = DateTime.UtcNow;
    }
}