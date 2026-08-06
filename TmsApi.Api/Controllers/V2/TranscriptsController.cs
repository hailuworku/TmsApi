using System.Threading.Channels;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Transcripts;
using TmsApi.Infrastructure.Transcripts;

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/transcripts")]
[ApiVersion("2.0")]
public class TranscriptsController(
    Channel<TranscriptRequest> channel,
    ITranscriptStatusStore statusStore) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> RequestTranscript(
        [FromBody] TranscriptRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken ct)
    {
        // 1. Check if we've already done this work using the Idempotency Key
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var existingReportId = await statusStore.GetReportIdForIdempotencyKeyAsync(idempotencyKey, ct);
            if (existingReportId != null)
            {
                var existingStatus = await statusStore.GetAsync(existingReportId, ct);
                // If found, return 202 with the previous result's location
                return Accepted(Url.Action(nameof(GetStatus), new { id = existingReportId }), existingStatus);
            }
        }

        // 2. Start a NEW request
        var reportId = Guid.NewGuid().ToString("N")[..12]; // Create a unique ID
        var status = await statusStore.CreateAsync(reportId, request.StudentId, ct);

        // 3. Link the key to this new ID for next time
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            await statusStore.LinkIdempotencyKeyAsync(idempotencyKey, reportId, ct);
        }

        // 4. Drop the request into the "Pipe" (Channel) for the worker to find
        await channel.Writer.WriteAsync(request.WithReportId(reportId), ct);

        // 5. Return 202 Accepted (Wait for polling)
        Response.Headers.RetryAfter = "5"; // Tell the client to wait 5 seconds before checking
        return Accepted(Url.Action(nameof(GetStatus), new { id = reportId }), status);
    }

    [HttpGet("{id}/status")]
    public async Task<IActionResult> GetStatus(string id, CancellationToken ct)
    {
        var status = await statusStore.GetAsync(id, ct);
        if (status == null) return NotFound();
        
        return Ok(status);
    }
}