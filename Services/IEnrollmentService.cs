using TmsApi.Dtos;

namespace TmsApi.Services;


public interface IEnrollmentService
{
    // ስህተት CS1501ን የሚፈታው ይህ መስመር ነው (3 arguments)
    Task<EnrollmentResponseDto?> GetByIdAsync(int courseId, int id, CancellationToken ct);

    // ስህተት CS1061ን የሚፈታው ይህ መስመር ነው (CreateAsync መኖሩን ያረጋግጣል)
    Task<EnrollmentResponseDto> CreateAsync(int courseId, EnrollStudentRequest request, CancellationToken ct);
    Task<IEnumerable<EnrollmentResponseDto>> GetByCourseAsync(int courseId, CancellationToken ct);
}