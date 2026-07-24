using Microsoft.Extensions.Caching.Hybrid;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Caching;
using Microsoft.Extensions.Logging;
namespace TmsApi.Infrastructure.Services;

public class CachedCourseService(HybridCache cache, ICourseService service, ILogger<CachedCourseService> logger) : ICachedCourseService
{
    public async Task<CourseResponseDto?> GetCourseAsync(string code, CancellationToken ct)
    {
        var key = CacheKeys.Course(code);
        bool dbHit = false;

        var result = await cache.GetOrCreateAsync(
            key,
            async token => {
                dbHit = true; // Cache MISS
                logger.LogInformation("Cache MISS for {Key}. Fetching from DB", key);
                var course = await service.GetByCodeAsync(code, token);
                return course != null ? new CourseResponseDto(course.Id, course.Code, course.Title, course.MaxCapacity, course.Enrollments.Count) : null;
            },
            tags: [CacheKeys.CoursesTag],
            cancellationToken: ct
        );

        if (!dbHit) logger.LogInformation("Cache HIT for {Key}", key);
        return result;
    }

    public async Task InvalidateCourseCacheAsync(CancellationToken ct)
    {
        logger.LogInformation("Invalidating cache tag: {Tag}", CacheKeys.CoursesTag);
        await cache.RemoveByTagAsync(CacheKeys.CoursesTag, ct);
    }
}