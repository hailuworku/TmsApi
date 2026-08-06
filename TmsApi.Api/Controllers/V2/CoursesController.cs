using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;
using TmsApi.Application.Utilities;

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/courses")]
[ApiVersion("2.0")]
[Tags("Courses")]
public class CoursesController(
    ICachedCourseService cacheService, 
    ICourseService dbService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCourses(
        [FromQuery] string? fields, 
        [FromQuery] PagedRequest request, 
        CancellationToken ct)
    {
        // 1. Fetch paged data from the database service
        var result = await dbService.GetCoursesAsync(request, ct);
        
        // 2. Apply data shaping based on the requested fields
        var shapedData = result.Items.ShapeData(fields, CourseDtoFields.Allowed);

        return Ok(new
        {
            data = shapedData,
            meta = new 
            { 
                result.TotalCount, 
                result.Page, 
                result.PageSize, 
                result.TotalPages 
            },
            links = new 
            { 
                self = $"/api/v2/courses?page={result.Page}&pageSize={result.PageSize}" 
            }
        });
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> GetCourse(string code, CancellationToken ct)
    {
        // Fetch single course using the cache service (Stampede Protection)
        var course = await cacheService.GetCourseAsync(code, ct);
        
        if (course == null) return NotFound();
        
        return Ok(course);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCourse(
        int id, 
        [FromBody] UpdateCourseRequest request, 
        CancellationToken ct)
    {
        var success = await dbService.UpdateAsync(id, request, ct);

        if (success)
        {
            // Invalidate cache so users don't see old data
            await cacheService.InvalidateCourseCacheAsync(ct);
        }

        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCourse(int id, CancellationToken ct)
    {
        var success = await dbService.DeleteAsync(id, ct);

        if (success)
        {
            // Clear cache after deletion
            await cacheService.InvalidateCourseCacheAsync(ct);
        }

        return success ? NoContent() : NotFound();
    }
}