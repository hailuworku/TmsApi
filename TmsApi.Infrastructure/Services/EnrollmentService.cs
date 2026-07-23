using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Infrastructure.Services; // ሴሚኮለን እዚህ ጋር ተጨምሯል

public class EnrollmentService(TmsDbContext context) : IEnrollmentService
{
    // 1. GET BY ID: አንድን ምዝገባ ለማየት
    public async Task<EnrollmentResponseDto?> GetByIdAsync(int courseId, int id, CancellationToken ct) =>
        await context.Enrollments
            .AsNoTracking()
            .Where(e => e.Id == id && e.CourseId == courseId)
            .Select(e => new EnrollmentResponseDto(e.Id, e.CourseId, e.StudentId, e.EnrolledAt, e.Grade)) // Grade እዚህ ተጨምሯል
            .FirstOrDefaultAsync(ct);

    // 2. ENROLL: ተማሪ ለመመዝገብ (ይህ በ Interface ላይ ያለው ስም ነው)
    public async Task<EnrollmentResponseDto> EnrollAsync(int courseId, EnrollStudentRequest request, CancellationToken ct)
    {
        var course = await context.Courses.Include(c => c.Enrollments).FirstOrDefaultAsync(c => c.Id == courseId, ct);
        if (course == null) throw new Exception("Course not found");
        if (course.Enrollments.Count >= course.MaxCapacity) throw new InvalidOperationException("Course is full");

        var enrollment = new Enrollment
        {
            CourseId = courseId,
            StudentId = request.StudentId,
            EnrolledAt = DateTime.UtcNow
        };

        context.Enrollments.Add(enrollment);
        await context.SaveChangesAsync(ct);

        return (await GetByIdAsync(courseId, enrollment.Id, ct))!;
    }

    // 3. GET BY COURSE ID: የኮርስ ተማሪዎችን ዝርዝር ለማየት
    public async Task<IEnumerable<EnrollmentResponseDto>> GetByCourseIdAsync(int courseId, CancellationToken ct)
    {
        return await context.Enrollments
            .AsNoTracking()
            .Where(e => e.CourseId == courseId)
            .Select(e => new EnrollmentResponseDto(e.Id, e.CourseId, e.StudentId, e.EnrolledAt, e.Grade))
            .ToListAsync(ct);
    }

    // 4. UPDATE GRADE: ውጤት ለመሙላት (PUT)
    public async Task<bool> UpdateGradeAsync(int courseId, int id, UpdateGradeRequest request, CancellationToken ct)
    {
        var enrollment = await context.Enrollments.FirstOrDefaultAsync(e => e.Id == id && e.CourseId == courseId, ct);
        if (enrollment == null) return false;

        enrollment.Grade = request.Grade;
        await context.SaveChangesAsync(ct);
        return true;
    }

    // 5. DELETE: ምዝገባ ለመሰረዝ
    public async Task<bool> DeleteAsync(int courseId, int id, CancellationToken ct)
    {
        var enrollment = await context.Enrollments.FirstOrDefaultAsync(e => e.Id == id && e.CourseId == courseId, ct);
        if (enrollment == null) return false;

        context.Enrollments.Remove(enrollment);
        await context.SaveChangesAsync(ct);
        return true;
    }
    public async Task<bool> ExistsAsync(int studentId, string courseCode, CancellationToken ct)
{
    // ተማሪው በዚህ ኮድ ባለው ኮርስ ላይ አስቀድሞ መኖሩን ያረጋግጣል
    return await context.Enrollments
        .AnyAsync(e => e.StudentId == studentId && e.Course.Code == courseCode, ct);
}

public async Task AddAsync(Enrollment enrollment, CancellationToken ct)
{
    context.Enrollments.Add(enrollment);
    await context.SaveChangesAsync(ct);
}
} 