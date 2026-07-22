using Microsoft.AspNetCore.Mvc;
using TmsApi.Dtos;
using TmsApi.Services;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses/{courseId:int}/enrollments")]
public class EnrollmentsController(
    ICourseService courseService,
    IEnrollmentService enrollmentService) : ControllerBase
{
    [HttpGet(Name = "ListCourseEnrollments")] // 
[EndpointSummary("List all enrollments for a specific course")]
public async Task<IActionResult> GetEnrollments(int courseId, CancellationToken ct)
{
    var course = await courseService.GetByIdAsync(courseId, ct);
    if (course is null) return NotFound();

    var enrollments = await enrollmentService.GetByCourseAsync(courseId, ct);
    return Ok(enrollments);
}
    [HttpGet("{id:int}", Name = nameof(GetEnrollment))]
    public async Task<IActionResult> GetEnrollment(int courseId, int id, CancellationToken ct)
    {
        // እዚህ ጋር 3 arguments መኖራቸውን አረጋግጥ
        var enrollment = await enrollmentService.GetByIdAsync(courseId, id, ct);
        return enrollment is not null ? Ok(enrollment) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> EnrollStudent(int courseId, EnrollStudentRequest request, CancellationToken ct)
    {
        var course = await courseService.GetByIdAsync(courseId, ct);
        if (course is null) return NotFound("Course not found.");

        if (course.EnrollmentCount >= course.MaxCapacity)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course is full",
                Status = StatusCodes.Status409Conflict,
                Detail = $"Course '{course.Title}' has reached its maximum capacity."
            });
        }

        // እዚህ ጋር CreateAsync የሚለው ስም መኖሩን አረጋግጥ
        var result = await enrollmentService.CreateAsync(courseId, request, ct);

        return CreatedAtAction(nameof(GetEnrollment),
            new { courseId = result.CourseId, id = result.Id }, result);
    }
}