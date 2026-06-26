using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TmsApi.Data;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    private readonly TmsDbContext _context;

    public TestController(TmsDbContext context)
    {
        _context = context;
    }

    // ==========================================
    // SESSION 1 EXPERIMENTS & QUERIES
    // ==========================================

    // 1. Deferred Execution Experiment - Page 117
    [HttpGet("deferred")]
    public IActionResult TestDeferred()
    {
        Console.WriteLine("\n>>> STEP 1: Building the query object (no database contact)...");
        var query = _context.Students.Where(s => s.GPA >= 3.0m);

        Console.WriteLine(">>> STEP 2: Appending a sorting clause...");
        var orderedQuery = query.OrderBy(s => s.Name);

        Console.WriteLine(">>> STEP 3: Materializing query into a C# List...");
        var results = orderedQuery.ToList(); // Database query is triggered here

        Console.WriteLine(">>> STEP 4: Materialization finished. List populated.\n");
        return Ok(results);
    }

    // 2. Translation Failure Experiment - Page 118
    [HttpGet("translation-fail")]
    public IActionResult TestTranslationFail()
    {
        Console.WriteLine("\n>>> STEP 1: Running non-translatable query...");
        try
        {
            var students = _context.Students
                .Where(s => IsHonorRoll(s.GPA)) // Fails because C# method cannot be translated to SQL
                .ToList();
            return Ok(students);
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> EXCEPTION CAUGHT: {ex.Message}\n");
            return BadRequest(new { Message = ex.Message });
        }
    }

    // 3. Query 1: Active students with GPA >= 3.0 - Page 119
    [HttpGet("active-high-gpa-count")]
    public async Task<IActionResult> GetActiveHighGpaCount()
    {
        var count = await _context.Students
            .Where(s => s.IsActive && s.GPA >= 3.0m)
            .CountAsync();

        return Ok(new { Count = count });
    }

    // 4. Query 2: Courses with most enrollments - Page 119
    [HttpGet("courses-by-enrollments")]
    public async Task<IActionResult> GetCoursesByEnrollments()
    {
        var list = await _context.Courses
            .Select(c => new
            {
                c.Title,
                EnrollmentCount = c.Enrollments.Count
            })
            .OrderByDescending(x => x.EnrollmentCount)
            .ToListAsync();

        return Ok(list);
    }

    // 5. Query 3: Average GPA per course - Page 120
    [HttpGet("average-gpa-per-course")]
    public async Task<IActionResult> GetAverageGpaPerCourse()
    {
        var list = await _context.Enrollments
            .GroupBy(e => e.Course.Title)
            .Select(g => new
            {
                Course = g.Key,
                AverageGPA = g.Average(e => e.Student.GPA)
            })
            .ToListAsync();

        return Ok(list);
    }

    // 6. Query 4: Students with zero enrollments - Page 120
    [HttpGet("students-zero-enrollments")]
    public async Task<IActionResult> GetStudentsZeroEnrollments()
    {
        var list = await _context.Students
            .Where(s => !s.Enrollments.Any())
            .Select(s => s.Name)
            .ToListAsync();

        return Ok(list);
    }

    // ==========================================
    // SESSION 2 SCHEMA DESIGNS
    // ==========================================

    // --- Exercise 3: Part 1 - Database-Level Pagination --- - Page 2 (Session 2)
    [HttpGet("students-paged")]
    public async Task<IActionResult> GetStudentsPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        Console.WriteLine("\n>>> RUNNING DATABASE-LEVEL PAGINATION...");

        var pagedStudents = await _context.Students
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize) // Translates to OFFSET in SQL [4]
            .Take(pageSize)              // Translates to LIMIT in SQL [4]
            .ToListAsync();

        return Ok(pagedStudents);
    }

    // --- Exercise 3: Part 2 - Top Courses by Enrollment GroupBy --- - Page 2 (Session 2)
    [HttpGet("top-courses")]
    public async Task<IActionResult> GetTopCourses()
    {
        Console.WriteLine("\n>>> RUNNING GROUPBY COURSE AGGREGATION...");

        var topCourses = await _context.Enrollments
            .GroupBy(e => e.Course.Title)
            .Select(g => new
            {
                CourseTitle = g.Key,
                EnrollmentCount = g.Count()
            })
            .OrderByDescending(x => x.EnrollmentCount)
            .Take(5)
            .ToListAsync();

        return Ok(topCourses);
    }

    // ==========================================
    // SESSION 3 SCHEMA OPERATIONS
    // ==========================================

    // --- Exercise 7: Part A - Intentional N+1 Query --- - Page 2 (Session 3)
    [HttpGet("n-plus-one")]
    public async Task<IActionResult> GetStudentsNPlusOne(CancellationToken cancellationToken)
    {
        Console.WriteLine("\n>>> STARTING INTENTIONAL N+1 PERFORMANCE BUG...");

        var students = await _context.Students.AsNoTracking().ToListAsync(cancellationToken);

        foreach (var s in students)
        {
            var count = await _context.Enrollments
                .AsNoTracking()
                .CountAsync(e => e.StudentId == s.Id, cancellationToken);

            Console.WriteLine($"Student {s.Name}: {count} enrollments");
        }

        return Ok(new { Message = "N+1 Demo Finished. Check console SQL logs." });
    }

    // --- Exercise 7: Part B - Safe Single-Query Projection --- - Page 3 (Session 3)
    [HttpGet("n-plus-one-fixed")]
    public async Task<IActionResult> GetStudentsFixed(CancellationToken cancellationToken)
    {
        Console.WriteLine("\n>>> STARTING SHAPED SINGLE-QUERY SOLUTION...");

        var report = await _context.Students
            .AsNoTracking()
            .Select(s => new
            {
                s.Name,
                EnrollmentCount = s.Enrollments.Count
            })
            .ToListAsync(cancellationToken);

        foreach (var r in report)
        {
            Console.WriteLine($"Student {r.Name}: {r.EnrollmentCount} enrollments");
        }

        return Ok(report);
    }

    // --- Exercise 8: Concurrency Token Test --- - Page 4 (Session 3)
    [HttpPost("update-gpa/{id}")]
    public async Task<IActionResult> UpdateStudentGpa(int id, [FromQuery] decimal gpa, CancellationToken cancellationToken)
    {
        var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (student == null) return NotFound("Student not found.");

        student.GPA = gpa;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            return Ok(new { Message = "GPA updated successfully", student.GPA });
        }
        catch (DbUpdateConcurrencyException)
        {
            Console.WriteLine("⚠️ CONCURRENCY CONFLICT DETECTED!");
            return Conflict("The student record was modified by another user. Please reload.");
        }
    }

    // --- Exercise 9: Part 1 - Bulk Archive --- - Page 5 (Session 3)
    [HttpPost("bulk-archive")]
    public async Task<IActionResult> BulkArchiveEnrollments(CancellationToken cancellationToken)
    {
        Console.WriteLine("\n>>> RUNNING BULK ARCHIVE EXECUTEUPDATE...");

        var updatedCount = await _context.Enrollments
            .Where(e => e.EnrolledAt < DateTime.UtcNow)
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.IsArchived, true), cancellationToken);

        return Ok(new { Message = $"Bulk updated {updatedCount} enrollments directly in DB." });
    }

    // --- Exercise 9: Part 2 - Soft Delete & Global Filters --- - Page 5 (Session 3)
    [HttpPost("soft-delete/{id}")]
    public async Task<IActionResult> SoftDeleteStudent(int id, CancellationToken cancellationToken)
    {
        var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (student == null) return NotFound("Student not found.");

        student.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);

        var visibleStudentsCount = await _context.Students.CountAsync(cancellationToken);
        var totalStudentsCount = await _context.Students.IgnoreQueryFilters().CountAsync(cancellationToken);

        return Ok(new
        {
            Message = "Student soft-deleted.",
            VisibleCount = visibleStudentsCount,
            TotalIncludingDeleted = totalStudentsCount
        });
    }

    // --- Helper Method for Translation Fail --- - Page 5
    private static bool IsHonorRoll(decimal gpa)
    {
        return gpa >= 3.5m;
    }
    [HttpGet("shaping")]
public async Task<IActionResult> TestShaping()
{
    // ዳታቤዙን በአንድ ጊዜ እንዲያመጣልን እናዘዋለን (Projection ይባላል)
    var report = await _context.Students
        .AsNoTracking()
        .Select(s => new {
            StudentName = s.Name,
            EnrollmentCount = s.Enrollments.Count // EF Core ይህንን በSQL COUNT(*) ደረጃ ይጨርሰዋል
        })
        .ToListAsync();

    return Ok(report);
}
[HttpPost("update-student/{id}")]
public async Task<IActionResult> UpdateStudent(int id, string newName)
{
    // 1. ተማሪውን ከዳታቤዝ ፈልገህ አምጣ
    var student = await _context.Students.FindAsync(id);
    if (student == null) return NotFound();

    // 2. ስሙን ቀይር
    student.Name = newName;

    // 3. Shadow Property (LastUpdated) ሴት አድርግ (ገጽ 119 TODO)
    // ይህ ኮለም በC# Student class ውስጥ የለም፣ ዳታቤዝ ውስጥ ግን አለ።
    _context.Entry(student).Property("LastUpdated").CurrentValue = DateTime.UtcNow;

    try 
    {
        await _context.SaveChangesAsync();
        return Ok($"Student {id} updated successfully. Check LastUpdated in pgAdmin!");
    }
    catch (DbUpdateConcurrencyException) 
    {
        // Concurrency የሚሠራው እዚህ ጋር ነው! (ገጽ 120 ላይ የተጠቀሰው)
        return Conflict("ይቅርታ፣ አንተ ዳታውን ከመቀየርህ በፊት ሌላ ሰው ቀይሮታል።");
    }
}
[HttpPost("archive-all")]
public async Task<IActionResult> ArchiveAll()
{
    // 50,000 ዳታ ቢሆን እንኳ በአንድ SQL ትእዛዝ ብቻ "Archive" ያደርጋቸዋል
    // ይህ ExecuteUpdateAsync ይባላል (ከ EF Core 7 ጀምሮ የመጣ ምርጥ ዘዴ ነው)
    int count = await _context.Enrollments
        .ExecuteUpdateAsync(s => s.SetProperty(e => e.IsArchived, true));

    return Ok($"{count} enrollments archived in one shot!");
}
}