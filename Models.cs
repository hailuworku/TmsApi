
// Immutable by design — the logging pipeline cannot corrupt this
public record EnrollmentRecord(string StudentId, string CourseCode, DateTime EnrolledAt);

// Legacy Pre-C# 14 Implementation (Verbose — commented out)
// public class Course
// {
//     private int _capacity;
//     public int Capacity
//     {
//         get => _capacity;
//         set
//         {
//             if (value <= 0)
//                 throw new ArgumentOutOfRangeException("Capacity must be positive.");
//             _capacity = value;
//         }
//     }
// }

// C# 14 fix — field keyword, validation in one line
public class Course
{
    public required string Code { get; init; }
    public required string Title
    {
        get;
        set => field = !string.IsNullOrWhiteSpace(value) ? value
        : throw new ArgumentException("Title cannot be empty or whitespace.", nameof(value));
    }
    // C# 14Auto-property validation using 'field'
    public int Capacity
    {
        get;
        set => field = value > 0
        ? value
        : throw new ArgumentOutOfRangeException(nameof(value), "System constraint: Capacit you must begreater than zero.");
    }
    public int EnrolledCount { get; set; }
}

//exercise:3 part 3
public class Student
{
    public required string Id { get; init; }
    public required string Name
    {
        get;
        set => field = !string.IsNullOrWhiteSpace(value)
        ? value
        : throw new ArgumentException("Name cannot be empty or whitespace.", nameof(value));
    }
    public int Age
    {
        get;
        set => field = value is >= 16 and <= 100
        ? value
        : throw new ArgumentOutOfRangeException(nameof(value), "Age must be between 16 and 100.");
    }
    public decimal GPA
    {
        get;
        set => field = value is >= 0.0m and <= 4.0m
        ? value
        : throw new ArgumentOutOfRangeException(nameof(value), "GPA must be between 0.0 and 4.0.");
    }
}
//exercise:3B oop contract
public interface IGradable
{
    string Title { get; }
    decimal CalculateGrade();
}
public class Quiz : IGradable
{
    public required string Title { get; init; }
    public required int CorrectAnswers { get; init; }
    public required int TotalQuestions { get; init; }
    public decimal CalculateGrade()
    {
        if (TotalQuestions == 0) return 0m;
        return (decimal)CorrectAnswers / TotalQuestions * 100m;
    }
}
public class LabAssignment : IGradable
{
    public required string Title { get; init; }
    public required decimal FunctionalityScore { get; init; }
    public required decimal CodeQualityScore { get; init; }
    public decimal CalculateGrade()
    {
        // 70% functionality, 30% code quality
        return (FunctionalityScore * 0.7m) + (CodeQualityScore * 0.3m);
    }
}