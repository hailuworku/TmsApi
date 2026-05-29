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
//end of module 1 code

// ============================================================
// Exercise 4: Guard Clauses & Pattern Matching
// ============================================================

var service = new EnrollmentService();

// Test 1: Valid registration
var validStudent = new Student { Id = "S1", Name = "Abeba", Age = 20, GPA = 3.8m };
var validCourse = new Course { Code = "CS-401", Title = "Advanced C#", Capacity = 30 };
var result = service.ProcessRegistration(validStudent, validCourse);
Console.WriteLine($"Enrolled: {result.StudentId} in {result.CourseCode}");

// Test 2: Null student — should throw
try
{
    service.ProcessRegistration(null, validCourse);
}
catch (ArgumentNullException ex)
{
    Console.WriteLine($"Guard caught: {ex.ParamName}");
}

// Test 3: Full course — should throw
var fullCourse = new Course { Code = "CS-402", Title = "Full Course", Capacity = 1 };
fullCourse.EnrolledCount = 1;
try
{
    service.ProcessRegistration(validStudent, fullCourse);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Business rule: {ex.Message}");
}

// ============================================================
// Exercise 5: Collections & LINQ
// ============================================================

List<Student> students = [
    new Student { Id = "S1", Name = "Abeba",    Age = 22, GPA = 3.8m },
    new Student { Id = "S2", Name = "Kidane",   Age = 21, GPA = 2.4m },
    new Student { Id = "S3", Name = "Dawit",    Age = 20, GPA = 3.1m },
    new Student { Id = "S4", Name = "Sara",     Age = 23, GPA = 3.9m },
    new Student { Id = "S5", Name = "Frehiwot", Age = 19, GPA = 2.0m },
    new Student { Id = "S6", Name = "Yonas",    Age = 24, GPA = 3.5m },
    new Student { Id = "S7", Name = "Meron",    Age = 22, GPA = 1.8m },
    new Student { Id = "S8", Name = "Tesfaye",  Age = 21, GPA = 2.9m }
];

// Step 2: Honors leaderboard
var leaderboard = students
    .Where(s => s.GPA >= 3.5m)
    .OrderByDescending(s => s.GPA)
    .Select(s => s.Name)
    .ToList();

Console.WriteLine($"Found {leaderboard.Count} Honors Students:");
foreach (var name in leaderboard)
    Console.WriteLine($"- {name}");

// Step 3: Class average
decimal averageGpa = students.Average(s => s.GPA);
Console.WriteLine($"\nClass Average GPA: {averageGpa:F2}");

// Step 4: Group by academic standing
var standingGroups = students.GroupBy(s => s.GPA switch
{
    >= 3.5m => "Honors",
    >= 2.5m => "Good Standing",
    >= 2.0m => "Probation",
    _ => "Academic Warning"
});

Console.WriteLine("\n--- Academic Standing Report ---");
foreach (var group in standingGroups)
{
    Console.WriteLine($"\n{group.Key} ({group.Count()}):");
    foreach (var st in group)
        Console.WriteLine($"  {st.Name} — GPA: {st.GPA}");
}

// Step 5: Collection expressions with spread
string[] backendCourses = ["C#", "ASP.NET Core"];
string[] frontendCourses = ["TypeScript", "Angular"];
string[] allCourses = [.. backendCourses, .. frontendCourses, "Capstone"];
Console.WriteLine($"\nFull curriculum: {string.Join(", ", allCourses)}");
//end of module session 2 
