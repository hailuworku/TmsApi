using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Domain.Entities;
using TmsApi.Application.Dtos;
using System.ComponentModel.DataAnnotations; // 1. ይህ የግድ ያስፈልጋል

namespace TmsApi.Api.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/courses")]
[ApiVersion("1.0")]
public class CoursesController(TmsDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCourses([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var baseQuery = context.Courses.AsNoTracking();
        var totalCount = await baseQuery.CountAsync(ct);

        var items = await baseQuery
            .OrderBy(c => c.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CourseResponseDto(c.Id, c.Code, c.Title, c.MaxCapacity, c.Enrollments.Count))
            .ToListAsync(ct);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return Ok(new { items, totalCount, page, pageSize, totalPages });
    }

    [HttpGet("{id:int}", Name = nameof(GetCourse))]
    public async Task<IActionResult> GetCourse(int id, CancellationToken ct)
    {
        var course = await context.Courses
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CourseResponseDto(c.Id, c.Code, c.Title, c.MaxCapacity, c.Enrollments.Count))
            .FirstOrDefaultAsync(ct);

        if (course == null) return NotFound();
        return Ok(course);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseRequest request)
    {
        var exists = await context.Courses.AnyAsync(c => c.Code == request.Code);
        if (exists)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course code already exists",
                Detail = $"A course with code '{request.Code}' is already registered.",
                Status = StatusCodes.Status409Conflict
            });
        }

        var course = new Course
        {
            Code = request.Code,
            Title = request.Title,
            MaxCapacity = request.MaxCapacity
        };

        context.Courses.Add(course);
        await context.SaveChangesAsync();

        var responseDto = new CourseResponseDto(course.Id, course.Code, course.Title, course.MaxCapacity, 0);
        return CreatedAtAction(nameof(GetCourse), new { id = course.Id }, responseDto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCourse(int id, [FromBody] UpdateCourseRequest request)
    {
        var course = await context.Courses.FindAsync(id);
        if (course == null) return NotFound();

        course.Code = request.Code;
        course.Title = request.Title;
        course.MaxCapacity = request.MaxCapacity;

        await context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        var course = await context.Courses.FindAsync(id);
        if (course == null) return NotFound();

        context.Courses.Remove(course);
        await context.SaveChangesAsync();
        return NoContent();
    }
} // የክላሱ መዝጊያ እዚህ ጋር ነው

// 2. ሪከርዶችን ከክላሱ ውጭ ማስቀመጥ ይሻላል
public record CreateCourseRequest
{
    [Required]
    [RegularExpression(@"^[A-Z]{3}-\d{3}$", ErrorMessage = "Code must follow the pattern XXX-000 (e.g., CSE-101).")]
    public required string Code { get; init; }

    [Required]
    [MaxLength(200)]
    public required string Title { get; init; }

    [Range(1, 200)]
    public int MaxCapacity { get; init; }
}

public record UpdateCourseRequest(string Code, string Title, int MaxCapacity);