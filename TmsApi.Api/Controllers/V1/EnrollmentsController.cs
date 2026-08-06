// // using Microsoft.AspNetCore.Mvc;
// // using TmsApi.Application.Dtos;
// // using TmsApi.Application.Interfaces;
// // using TmsApi.Domain.Entities;

// // namespace TmsApi.Application.Application.Controllers;

// // [ApiController]
// // [Route("api/courses/{courseId:int}/enrollments")]
// // public class EnrollmentsController(
// //     ICourseService courseService,
// //     IEnrollmentService enrollmentService) : ControllerBase
// // {
// //     [HttpGet(Name = "ListCourseEnrollments")] // 
// //     [EndpointSummary("List all enrollments for a specific course")]
// //     public async Task<IActionResult> GetEnrollments(int courseId, CancellationToken ct)
// //     {
// //         var course = await courseService.GetByIdAsync(courseId, ct);
// //         if (course is null) return NotFound();

// //         var enrollments = await enrollmentService.GetByCourseAsync(courseId, ct);
// //         return Ok(enrollments);
// //     }
// //     [HttpGet("{id:int}", Name = nameof(GetEnrollment))]
// //     public async Task<IActionResult> GetEnrollment(int courseId, int id, CancellationToken ct)
// //     {
// //         // እዚህ ጋር 3 arguments መኖራቸውን አረጋግጥ
// //         var enrollment = await enrollmentService.GetByIdAsync(courseId, id, ct);
// //         return enrollment is not null ? Ok(enrollment) : NotFound();
// //     }

// //     [HttpPost]
// //     public async Task<IActionResult> EnrollStudent(int courseId, EnrollStudentRequest request, CancellationToken ct)
// //     {
// //         var course = await courseService.GetByIdAsync(courseId, ct);
// //         if (course is null) return NotFound("Course not found.");

// //         if (course.EnrollmentCount >= course.MaxCapacity)
// //         {
// //             return Conflict(new ProblemDetails
// //             {
// //                 Title = "Course is full",
// //                 Status = StatusCodes.Status409Conflict,
// //                 Detail = $"Course '{course.Title}' has reached its maximum capacity."
// //             });
// //         }

// //         // እዚህ ጋር CreateAsync የሚለው ስም መኖሩን አረጋግጥ
// //         var result = await enrollmentService.CreateAsync(courseId, request, ct);

// //         return CreatedAtAction(nameof(GetEnrollment),
// //             new { courseId = result.CourseId, id = result.Id }, result);
// //     }
// // }
// using Microsoft.EntityFrameworkCore;
// using TmsApi.Application.Dtos;
// using TmsApi.Application.Interfaces;
// using TmsApi.Domain.Entities;
// using TmsApi.Infrastructure.Persistence;

// namespace TmsApi.Infrastructure.Services;

// public class EnrollmentService(TmsDbContext context) : IEnrollmentService
// {
//     // Fetch all enrollments for a specific course
//     public async Task<IEnumerable<EnrollmentResponseDto>> GetByCourseIdAsync(int courseId, CancellationToken ct)
//     {
//         return await context.Enrollments
//             .AsNoTracking()
//             .Where(e => e.CourseId == courseId)
//             .Select(e => new EnrollmentResponseDto(e.Id, e.CourseId, e.StudentId, e.EnrolledAt, e.Grade))
//             .ToListAsync(ct);
//     }

//     // Register a student with strict Business Rules
//     public async Task<EnrollmentResponseDto> EnrollAsync(int courseId, EnrollStudentRequest request, CancellationToken ct)
//     {
//         // 1. Check if course exists and include its current enrollments for capacity check
//         var course = await context.Courses
//             .Include(c => c.Enrollments)
//             .FirstOrDefaultAsync(c => c.Id == courseId, ct);

//         if (course == null) throw new KeyNotFoundException("Course not found");

//         // 2. Business Rule: Is the course full?
//         if (course.Enrollments.Count >= course.MaxCapacity)
//             throw new InvalidOperationException("Course has reached maximum capacity");

//         // 3. Business Rule: Is student already enrolled?
//         var exists = await context.Enrollments.AnyAsync(e => e.CourseId == courseId && e.StudentId == request.StudentId, ct);
//         if (exists) throw new InvalidOperationException("Student is already enrolled in this course");

//         // 4. Create the new record
//         var enrollment = new Enrollment {
//             CourseId = courseId,
//             StudentId = request.StudentId,
//             EnrolledAt = DateTime.UtcNow
//         };

//         context.Enrollments.Add(enrollment);
//         await context.SaveChangesAsync(ct);

//         return new EnrollmentResponseDto(enrollment.Id, enrollment.CourseId, enrollment.StudentId, enrollment.EnrolledAt, enrollment.Grade);
//     }

//     // Update grade (PUT logic)
//     public async Task<bool> UpdateGradeAsync(int courseId, int id, UpdateGradeRequest request, CancellationToken ct)
//     {
//         var enrollment = await context.Enrollments.FirstOrDefaultAsync(e => e.Id == id && e.CourseId == courseId, ct);
//         if (enrollment == null) return false;

//         enrollment.Grade = request.Grade;
//         await context.SaveChangesAsync(ct);
//         return true;
//     }

//     // Delete/Drop enrollment (DELETE logic)
//     public async Task<bool> DeleteAsync(int courseId, int id, CancellationToken ct)
//     {
//         var enrollment = await context.Enrollments.FirstOrDefaultAsync(e => e.Id == id && e.CourseId == courseId, ct);
//         if (enrollment == null) return false;

//         context.Enrollments.Remove(enrollment);
//         await context.SaveChangesAsync(ct);
//         return true;
//     }
// }

using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;
using Asp.Versioning;
namespace TmsApi.Api.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/Enrollments")] 
[ApiVersion("1.0")]
[Tags("Enrollments")]
public class EnrollmentsController(IEnrollmentService enrollmentService) : ControllerBase
{
    [HttpGet] // GET /api/courses/1/enrollments
    public async Task<IActionResult> Get(int courseId, CancellationToken ct)
        => Ok(await enrollmentService.GetByCourseIdAsync(courseId, ct));

    [HttpPost] // POST /api/courses/1/enrollments
    public async Task<IActionResult> Enroll(int courseId, EnrollStudentRequest request, CancellationToken ct)
    {
        try {
            var result = await enrollmentService.EnrollAsync(courseId, request, ct);
            return Created("", result);
        } catch (Exception ex) {
            return Conflict(new ProblemDetails { Title = "Registration Failed", Detail = ex.Message });
        }
    }

    [HttpPut("{id:int}")] // PUT /api/courses/1/enrollments/5 (Update Grade)
    public async Task<IActionResult> UpdateGrade(int courseId, int id, UpdateGradeRequest request, CancellationToken ct)
    {
        var success = await enrollmentService.UpdateGradeAsync(courseId, id, request, ct);
        return success ? Ok() : NotFound();
    }

    [HttpDelete("{id:int}")] // DELETE /api/courses/1/enrollments/5 (Drop)
    public async Task<IActionResult> Delete(int courseId, int id, CancellationToken ct)
    {
        var success = await enrollmentService.DeleteAsync(courseId, id, ct);
        return success ? NoContent() : NotFound();
    }
}