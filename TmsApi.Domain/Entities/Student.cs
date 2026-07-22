using System.Collections.Generic;

namespace TmsApi.Domain.Entities;

public class Student
{
    public int Id { get; set; } // Primary Key [110]
    public required string RegistrationNumber { get; set; } // Unique natural key [110]
    public required string Name { get; set; }
    public decimal GPA { get; set; }
    public bool IsActive { get; set; } = true;

    // Exercise 9: Soft-delete flag - Page 5
    public bool IsDeleted { get; set; } = false;

    // Exercise 8: Concurrency token xmin - Page 4
    public uint Version { get; set; }

    // Navigation property [110]
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}