using System;

namespace TmsApi.Domain.Entities;

public class Enrollment
{
    public int Id { get; set; } // Primary Key
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public decimal? Grade { get; set; }
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    // Exercise 9: Bulk archive flag - Page 5
    public bool IsArchived { get; set; } = false;

    // Navigation properties [111]
    public Student Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
}