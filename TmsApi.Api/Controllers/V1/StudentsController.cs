using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;
using Asp.Versioning;
namespace TmsApi.Api.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/students")] 
[ApiVersion("1.0")]
[Tags("Students")]
public class StudentsController(IStudentService studentService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PagedRequest request, CancellationToken ct) 
        => Ok(await studentService.GetStudentsAsync(request, ct));

    [HttpPost]
    public async Task<IActionResult> Create(CreateStudentRequest request, CancellationToken ct)
    {
        if (await studentService.ExistsAsync(request.RegistrationNumber, ct))
            return Conflict(new ProblemDetails { Title = "Registration number exists" });

        var result = await studentService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetAll), result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CreateStudentRequest request, CancellationToken ct)
    {
        var success = await studentService.UpdateAsync(id, request, ct);
        return success ? Ok() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var success = await studentService.DeleteAsync(id, ct);
        return success ? NoContent() : NotFound();
    }
}