using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using TmsApi.Application.Interfaces;

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/certificates")]
[ApiVersion("2.0")]
public class CertificatesController(ICertificateService certificates) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Issue([FromBody] IssueRequest req, CancellationToken ct)
    {
        try {
            var result = await certificates.IssueCertificateAsync(req.StudentId, req.CourseCode, ct);
            return Ok(result);
        }
        catch (Exception ex) {
            return Problem(title: "Certificate failed", detail: ex.Message, statusCode: 400);
        }
    }
    public record IssueRequest(int StudentId, string CourseCode);
}