using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TmsDbContext>(options
 => options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase")));
// Add authentication and authorization services
builder.Services.AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);
builder.Services.AddAuthorization();
var app = builder.Build();
// Use the request logging middleware
app.UseMiddleware<RequestLoggingMiddleware>();

//Use authentication and authorization middleware

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Protected endpoint
app.MapGet("/api/assessments/results", () => Results.Ok(new
{
    courseCode = "CS-101",
    studentId = "S-001",
    letterGrade = "A"
})).RequireAuthorization();


app.Run();