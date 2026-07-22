using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Infrastructure.Services;

public class StudentService(TmsDbContext context) : IStudentService
{
    public async Task<StudentResponseDto?> GetByIdAsync(int id, CancellationToken ct) =>
        await context.Students.AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new StudentResponseDto(s.Id, s.RegistrationNumber, s.Name, s.GPA, s.IsActive))
            .FirstOrDefaultAsync(ct);

    public async Task<PagedResponse<StudentResponseDto>> GetStudentsAsync(PagedRequest request, CancellationToken ct)
    {
        var query = context.Students.AsNoTracking();

        // Search logic
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(s => EF.Functions.ILike(s.Name, $"%{request.Search}%") 
                                  || EF.Functions.ILike(s.RegistrationNumber, $"%{request.Search}%"));
        }

        var totalCount = await query.CountAsync(ct);

        // Sort and Page
        var items = await query
            .OrderBy(s => s.Name)
            .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .Select(s => new StudentResponseDto(s.Id, s.RegistrationNumber, s.Name, s.GPA, s.IsActive))
            .ToListAsync(ct);

        return new PagedResponse<StudentResponseDto> { Items = items, TotalCount = totalCount, Page = request.Page, PageSize = request.PageSize };
    }

    public async Task<StudentResponseDto> CreateAsync(CreateStudentRequest request, CancellationToken ct)
    {
        var student = new Student { RegistrationNumber = request.RegistrationNumber, Name = request.Name, GPA = request.GPA };
        context.Students.Add(student);
        await context.SaveChangesAsync(ct);
        return (await GetByIdAsync(student.Id, ct))!;
    }

    public async Task<bool> UpdateAsync(int id, CreateStudentRequest request, CancellationToken ct)
    {
        var student = await context.Students.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (student == null) return false;

        student.Name = request.Name;
        student.GPA = request.GPA;
        student.RegistrationNumber = request.RegistrationNumber;

        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var student = await context.Students.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (student == null) return false;

        context.Students.Remove(student);
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ExistsAsync(string regNum, CancellationToken ct) =>
        await context.Students.AnyAsync(s => s.RegistrationNumber == regNum, ct);
}