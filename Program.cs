using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using TmsApi;
var builder = WebApplication.CreateBuilder(args);

// Add authentication and authorization services
builder.Services.AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);
builder.Services.AddAuthorization();
//lifetimes registration
builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
// payment options configuration
builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments") // appsettings WITHOUT .json
    .ValidateDataAnnotations()     // required እና range CHECKS 
    .ValidateOnStart();            // Validate on start

// 
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

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