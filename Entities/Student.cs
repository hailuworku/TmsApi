namespace TmsApi.Entities;

public class Student
{
    // Exercise 8: Concurrency token row version (xmin) - Page 4
    public uint Version { get; set; }
    public int Id { get; set; }
    public required string RegistrationNumber { get; set; }
    public required string Name { get; set; }

    public decimal GPA { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}


