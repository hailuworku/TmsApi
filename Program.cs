//exercise:1
// ============================================================
// Step 1: Reproduce the legacy bug (warnings, not errors)
// ============================================================

// string region = null;                // ⚠ CS8600: assigning null to non-nullable
// Console.WriteLine(region.ToUpper()); // ⚠ CS8602: possible null dereference

// ============================================================
// Step 2: Fix it three ways
// ============================================================

// '?' means: "this variable is allowed to be null"
string? region = null;

// '?.' null-conditional: skips ToUpper() if region is null — no crash
string? upperRegion = region?.ToUpper();
Console.WriteLine($"Region (conditional): {upperRegion}");

// '??' null-coalescing: use fallback value if region is null
string displayRegion = region ?? "Unassigned";
Console.WriteLine($"Region (coalesced): {displayRegion}");

// '??=' null-coalescing assignment: assign only if currently null
region ??= "Addis Ababa";
Console.WriteLine($"Region (assigned): {region}");

// ============================================================
// Step 3: TMS variables used throughout this workbook
// ============================================================

string studentName = "Abeba";
string studentId = "STU-001";
int enrollmentCount = 3;
decimal grantAmount = 1999.99m;   // 'm' suffix = decimal literal (no float drift)
DateTime enrolledAt = DateTime.UtcNow;
string? campusRegion = null;

Console.WriteLine($"Student:  {studentName} ({studentId})");
Console.WriteLine($"Courses:  {enrollmentCount}");
Console.WriteLine($"Grant:    {grantAmount:F2}");
Console.WriteLine($"Enrolled: {enrolledAt:yyyy-MM-dd}");
Console.WriteLine($"Campus:   {campusRegion ?? "Not assigned"}");
////Exersise:2
// Legacy implementation — the bug that caused the audit failure
// double grantPerStudent = 1999.99;
// double totalAllocation = grantPerStudent * 100_000;
// Console.WriteLine($"Total allocated (double): {totalAllocation}");
// Fixed implementation — exact financial math
decimal grantPerStudent = 1999.99m;
decimal totalAllocation = grantPerStudent * 100_000m;
Console.WriteLine($"Total allocated (decimal): {totalAllocation}");
Console.WriteLine($"Total allocated (formatted): {totalAllocation:F2}");
//exercise:3
// Legacy implementation — what the logging service did to the data
// public class Enrollment
// {
//     public string StudentId { get; set; } = string.Empty;
//     public string CourseCode { get; set; } = string.Empty;
//     public DateTime ProcessedAt { get; set; }
// }
// Somewhere in the logging pipeline:
//enrollment.CourseCode = null; // ← No compiler error. Data silently corrupted.

var enrollment = new EnrollmentRecord("STU-001", "CS-401", DateTime.UtcNow);
Console.WriteLine(enrollment);
// Try to mutate it — uncomment this line and see the compiler error:
//enrollment.CourseCode = "HACKED"; // ERROR: init-only property
// Non-destructive copy — creates a NEW record with one field changed
var corrected = enrollment with { CourseCode = "CS-402" };
Console.WriteLine(corrected);
// Value equality — two records with the same data are equal
var duplicate = new EnrollmentRecord("STU-001", "CS-401", enrollment.EnrolledAt);
Console.WriteLine($"Same data? {enrollment == duplicate}"); // True

//exercise:3 part 3 — Student validation
var s = new Student { Id = "S1", Name = "Abeba", Age = 20, GPA = 3.8m };
Console.WriteLine($"Student: {s.Name}, GPA: {s.GPA}");

// Invalid name — empty string
try { var s2 = new Student { Id = "S2", Name = "", Age = 20, GPA = 3.0m }; }
catch (ArgumentException ex) { Console.WriteLine($"Caught: {ex.Message}"); }

// Invalid age — too young
try { var s3 = new Student { Id = "S3", Name = "Test", Age = 12, GPA = 3.0m }; }
catch (ArgumentOutOfRangeException ex) { Console.WriteLine($"Caught: {ex.Message}"); }

// Invalid GPA — above 4.0
try { var s4 = new Student { Id = "S4", Name = "Test", Age = 20, GPA = 5.0m }; }
catch (ArgumentOutOfRangeException ex) { Console.WriteLine($"Caught: {ex.Message}"); }
//exercise:3 part 2

var course = new Course { Code = "CS-401", Title = "Advanced C#", Capacity = 30 };
Console.WriteLine($"Course: {course.Title} (Capacity: {course.Capacity})");
// Invalid capacity — should throw
try
{
    course.Capacity = -5;
}
catch (ArgumentOutOfRangeException ex)
{
    Console.WriteLine($"Caught: {ex.Message}");
}
// Invalid title — should throw
try
{
    course.Title = "";
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Caught: {ex.Message}");
}
//exercise:3 part 3
var student = new Student { Id = "S1", Name = "Abeba", Age = 20, GPA = 3.8m };
Console.WriteLine($"Student: {student.Name}, GPA: {student.GPA}");
// These should throw — try each one:
try
{
    new Student { Id = "S2", Name = "Hailu", Age = 20, GPA = 3.0m };
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Caught: {ex.Message}");
}
try
{
    new Student { Id = "S3", Name = "Test", Age = 12, GPA = 3.0m };
}
catch (ArgumentOutOfRangeException ex)
{
    Console.WriteLine($"Caught: {ex.Message}");
}
try
{
    new Student { Id = "S4", Name = "Test", Age = 20, GPA = 5.0m };
}
catch (ArgumentOutOfRangeException ex)
{
    Console.WriteLine($"Caught: {ex.Message}");
}
//exercise:3B oop contract
void PrintGradeReport(IEnumerable<IGradable> assessments)
{
    Console.WriteLine("--- Grade Report---");
    foreach (var item in assessments)
    {
        Console.WriteLine($"{item.Title}: {item.CalculateGrade():F2}%");
    }
}
// Test it — one array holds two completely different types
IGradable[] cohortAssessments = [
new Quiz { Title = "C# Basics", CorrectAnswers = 18, TotalQuestions = 20 },
new LabAssignment { Title = "Registration API", FunctionalityScore = 90m, CodeQualityScore =85m}];
PrintGradeReport(cohortAssessments);
