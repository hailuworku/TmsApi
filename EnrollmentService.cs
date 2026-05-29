// Exercise 4: Guard Clauses & Pattern Matching
public class EnrollmentService
{
    public EnrollmentRecord ProcessRegistration(Student? student, Course? course)
    {
        // Guard clauses — fail fast
        if (student is null) throw new ArgumentNullException(nameof(student));
        if (course is null) throw new ArgumentNullException(nameof(course));
        if (course.EnrolledCount >= course.Capacity)
            throw new InvalidOperationException($"Course {course.Code} is full.");

        // Switch expression — classify academic standing
        string standing = student.GPA switch
        {
            >= 3.5m => "Honors",
            >= 2.5m => "Good Standing",
            _       => "Academic Warning"
        };
        Console.WriteLine($"{student.Name} is in {standing}.");

        return new EnrollmentRecord(student.Id, course.Code, DateTime.UtcNow);
    }
}
