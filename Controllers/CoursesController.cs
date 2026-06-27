// using Microsoft.AspNetCore.Mvc;
// using TmsApi.Entities;
// using TmsApi.Services;

// namespace TmsApi.Controllers;

// [ApiController]
// [Route("api/courses")] // የ API አድራሻ (ገጽ 129)
// public class CoursesController(ICourseService courseService) : ControllerBase
// {
//     // GET /api/courses/1 - መረጃ ለማንበብ (ገጽ 128)
//     [HttpGet("{id:int}", Name = nameof(GetCourseById))]
//     public async Task<IActionResult> GetCourseById(int id, CancellationToken ct)
//     {
//         var course = await courseService.GetByIdAsync(id, ct);

//         // ኮርሱ ከሌለ 404 መልስ፣ ካለ ግን ዳታውን በ 200 OK መልስ
//         return course is not null ? Ok(course) : NotFound();
//     }

//     // POST /api/courses - አዲስ ኮርስ ለመመዝገብ (ገጽ 129)
//     [HttpPost]
//     public async Task<IActionResult> CreateCourse(Course course, CancellationToken ct)
//     {
//         var result = await courseService.CreateAsync(course, ct);

//         // 201 Created ይመልሳል + አዲሱ ዳታ የት እንደሚገኝ (Location) ይናገራል
//         return CreatedAtAction(nameof(GetCourseById), new { id = result.Id }, result);
//     }
// }
using Microsoft.AspNetCore.Mvc;
using TmsApi.Dtos;
using TmsApi.Services;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController(ICourseService courseService) : ControllerBase
{
    [HttpGet("{id:int}", Name = nameof(GetCourseById))]
    public async Task<IActionResult> GetCourseById(int id, CancellationToken ct)
    {
        var course = await courseService.GetByIdAsync(id, ct);
        return course is not null ? Ok(course) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> CreateCourse(CreateCourseRequest request, CancellationToken ct)
    {
        // 1. የቢዝነስ ህግ ቼክ (ገጽ 136 - TODO 1)
        // ዳታቤዙ ውስጥ ኮዱ ቀድሞ ካለ 409 Conflict በል
        if (await courseService.CodeExistsAsync(request.Code, ct))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course code already exists",
                Detail = $"A course with code '{request.Code}' is already registered.",
                Status = StatusCodes.Status409Conflict
            });
        }

        // 2. ኮዱ ከሌለ መመዝገቡን ቀጥል
        var result = await courseService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetCourseById), new { id = result.Id }, result);
    }
}
