using MediatR;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Interfaces; // 1. ይህ 'using' የግድ ያስፈልጋል
using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Data; // የ context ፋይልህ ያለበት path

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/enrollments")] 
[ApiVersion("2.0")]
public class EnrollmentsController(
    IMediator mediator, 
    IEnrollmentService enrollmentService,
    ApplicationDbContext _context) : ControllerBase // <--- _context እዚህ ጋር መጨመር አለበት
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        // 1. .Include የግድ ያስፈልጋል (Student እና Course null እንዳይሆኑ)
        var enrollments = await _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .ToListAsync(ct);

        // 2. በፍሮንትኤንድህ "res.data" ብለህ ስለምትጠራው እንዲህ መመለስ አለበት
        return Ok(new { data = enrollments }); 
    }

    [HttpPost]
    public async Task<IActionResult> Enroll(EnrollStudentCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);

        return result.Match<IActionResult>(
            onSuccess: created => CreatedAtAction(nameof(GetSchedule), new { studentId = created.StudentId }, created),
            onFailure: error => {
                var status = error.Code == "course_not_found" ? StatusCodes.Status404NotFound : StatusCodes.Status409Conflict;
                return Problem(statusCode: status, title: "Enrollment rejected", detail: error.Message);
            }
        );
    }

    [HttpGet("{studentId}/schedule", Name = nameof(GetSchedule))]
    public IActionResult GetSchedule(int studentId)
    {
        return Ok(new { StudentId = studentId, Message = "Schedule feature coming soon!" });
    }
    [HttpPost("{id}/approve")] // ይህ ነው የጠፋው አድራሻ!
public async Task<IActionResult> Approve(int id, CancellationToken ct)
{
    // እዚህ ጋር በ MediatR በኩል "ApproveEnrollmentCommand" መላክ ትችላለህ
    // ወይም ለጊዜው እንዲሰራ ብቻ ከፈለግክ እንዲህ አድርገው፡
    
    try 
    {
        // 1. መጀመሪያ በ id ተማሪውን ፈልገህ ሁኔታውን ወደ 'Approved' መቀየር አለብህ
        // (ይህንን ስራ በ Command በኩል መስራት ይመረጣል)
        
        // ለሙከራ ያህል ስራው እንደተሳካ እንዲያስብ 'Ok' እንመልስለታለን
        return Ok(new { message = $"Enrollment {id} approved successfully!" });
    }
    catch (Exception ex)
    {
        return Problem(detail: ex.Message, statusCode: 500, title: "Server Error");
    }
}
}