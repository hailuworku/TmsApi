// using Microsoft.AspNetCore.Mvc;
// using TmsApi.Dtos;
// using TmsApi.Services;

// namespace TmsApi.Controllers;

// [ApiController]
// [Route("api/courses")]
// [Tags("Courses")]
// public class CoursesController(ICourseService courseService) : ControllerBase
// {
//     [HttpGet]
//     public async Task<IActionResult> GetCourses([FromQuery] PagedRequest request, CancellationToken ct)
//     {
//         var result = await courseService.GetCoursesAsync(request, ct);
//         return Ok(result);
//     }

//     [HttpGet("{id:int}", Name = nameof(GetCourseById))]
//     public async Task<IActionResult> GetCourseById(int id, CancellationToken ct)
//     {
//         var course = await courseService.GetByIdAsync(id, ct);
//         return course is not null ? Ok(course) : NotFound();
//     }

//     [HttpPost]
//     public async Task<IActionResult> CreateCourse(CreateCourseRequest request, CancellationToken ct)
//     {
//         if (await courseService.CodeExistsAsync(request.Code, ct))
//         {
//             return Conflict(new ProblemDetails { Title = "Course code already exists" });
//         }
//         var result = await courseService.CreateAsync(request, ct);
//         return CreatedAtAction(nameof(GetCourseById), new { id = result.Id }, result);
//     }
// }
using Microsoft.AspNetCore.Mvc;
using TmsApi.Dtos;
using TmsApi.Services;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses")]
[Tags("Courses")] // Scalar ላይ እንዲመደብ
public class CoursesController(ICourseService courseService, LinkGenerator linkGenerator) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("List all courses")]
    public async Task<IActionResult> GetCourses([FromQuery] PagedRequest request, CancellationToken ct)
    {
        var result = await courseService.GetCoursesAsync(request, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}", Name = nameof(GetCourseById))]
    [EndpointSummary("Get a course by ID with links")]
    public async Task<IActionResult> GetCourseById(int id, CancellationToken ct)
    {
        var course = await courseService.GetByIdAsync(id, ct);
        if (course is null) return NotFound();

        // ሊንኮችን መፍጠር
        var links = new List<LinkDto>
        {
            new (linkGenerator.GetPathByName(HttpContext, nameof(GetCourseById), new { id })!, "self", "GET"),
            new (linkGenerator.GetPathByName(HttpContext, nameof(GetCourseById), new { id })!, "update", "PUT"),
            new (linkGenerator.GetPathByName(HttpContext, nameof(GetCourseById), new { id })!, "delete", "DELETE"),
            new (linkGenerator.GetPathByAction(HttpContext, "GetEnrollments", "Enrollments", new { courseId = id })!, "enrollments", "GET")
        };

        // ኮርሱ ካልሞላ ብቻ የመመዝገቢያ ሊንክ ጨምር
        if (course.EnrollmentCount < course.MaxCapacity)
        {
            links.Add(new (linkGenerator.GetPathByAction(HttpContext, "EnrollStudent", "Enrollments", new { courseId = id })!, "enroll", "POST"));
        }

        return Ok(new CourseDetailDto {
            Id = course.Id, Code = course.Code, Title = course.Title,
            MaxCapacity = course.MaxCapacity, EnrollmentCount = course.EnrollmentCount,
            Links = links
        });
    }

    [HttpPost]
    [EndpointSummary("Create a new course")]
    public async Task<IActionResult> CreateCourse(CreateCourseRequest request, CancellationToken ct)
    {
        if (await courseService.CodeExistsAsync(request.Code, ct))
            return Conflict(new ProblemDetails { Title = "Course code already exists" });

        var result = await courseService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetCourseById), new { id = result.Id }, result);
    }
}