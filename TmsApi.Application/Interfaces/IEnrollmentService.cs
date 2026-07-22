using TmsApi.Application.Dtos;

namespace TmsApi.Application.Interfaces;

public interface IEnrollmentService
{
    // 1. Get all students registered for a specific course
    Task<IEnumerable<EnrollmentResponseDto>> GetByCourseIdAsync(int courseId, CancellationToken ct);

    // 2. Register a student to a course (POST)
    Task<EnrollmentResponseDto> EnrollAsync(int courseId, EnrollStudentRequest request, CancellationToken ct);

    // 3. Update a student's grade (PUT)
    Task<bool> UpdateGradeAsync(int courseId, int id, UpdateGradeRequest request, CancellationToken ct);

    // 4. Delete/Drop an enrollment (DELETE)
    Task<bool> DeleteAsync(int courseId, int id, CancellationToken ct);
}