using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TmsApi.Controllers
{
    [ApiController]
    [Route("api/enrollments")]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IEnrollmentService _enrollmentService;

        // Constructor Injection - የፈጠርነውን ሰርቪስ እዚህ ውስጥ እናስገባለን [99]
        public EnrollmentsController(IEnrollmentService enrollmentService)
        {
            _enrollmentService = enrollmentService;
        }

        // 1. GET /api/enrollments — ሁሉንም ምዝገባዎች ማምጫ [99]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var enrollments = await _enrollmentService.GetAllAsync();
            return Ok(enrollments);
        }

        // 2. GET /api/enrollments/{id} — አንድን ምዝገባ ብቻ ማምጫ (ወይም 404) [99]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var record = await _enrollmentService.GetByIdAsync(id);
            return record is not null ? Ok(record) : NotFound();
        }

        // 3. POST /api/enrollments — አዲስ ምዝገባ ማከናወኛ (201 Created) [100]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEnrollmentRequest request)
        {
            var record = await _enrollmentService.EnrollAsync(request.StudentId, request.CourseCode);

            // የ 201 Created ምላሽ ከ Location ሄደር ጋር መመለስ [100]
            return CreatedAtAction(nameof(GetById), new { id = record.Id }, record);
        }

        // 4. DELETE /api/enrollments/{id} — ምዝገባ መሰረዣ (204 or 404) [100]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var deleted = await _enrollmentService.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }

    // 5. የምዝገባ መጠየቂያ ዳታ ፎርማት (Request Model) [100]
    public record CreateEnrollmentRequest(string StudentId, string CourseCode);
}