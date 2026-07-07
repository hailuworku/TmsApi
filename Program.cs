using Microsoft.AspNetCore.OpenApi;
using Scalar.AspNetCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TmsApi.Data;
using TmsApi.Entities;
using TmsApi.Services;
using TmsApi.Authentications;
using TmsApi.Middlewares;

var builder = WebApplication.CreateBuilder(args);



// Add authentication and authorization services
builder.Services.AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);
builder.Services.AddAuthorization();

// 1. ADD THIS: Register MVC Controllers - Page 98
builder.Services.AddProblemDetails(); // tell problem hapen i front end
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register TmsDbContext with Npgsql and Console Logging - Page 115
builder.Services.AddDbContext<TmsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase"))
        .LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging());

//to comunicate each others
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>(); 
var app = builder.Build();

// Use the request logging middleware - Page 84
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseExceptionHandler();    // 
app.UseStatusCodePages();     // 

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// 2. ADD THIS: Map MVC Controllers - Page 98
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();           // <--- እዚህ ጋር (ደረጃ 4)
    app.MapScalarApiReference(); // <--- እዚህ ጋር (ደረጃ 4)
    
    // ... Seeder ኮድ ...
}

app.MapControllers();

// Protected endpoint - Page 88
app.MapGet("/api/assessments/results", () => Results.Ok(new
{
    courseCode = "CS-101",
    studentId = "S-001",
    letterGrade = "A"
})).RequireAuthorization();

// Auto-Seeder: Seed test data at startup - Page 115
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
    ctx.Database.Migrate(); // Apply pending migrations on start [115]

    if (!ctx.Students.Any())
    {
        var students = new List<Student> {
            new() { RegistrationNumber="TMS-2026-0001", Name="Alice Smith",   GPA=3.8m, IsActive=true },
            new() { RegistrationNumber="TMS-2026-0002", Name="Bob Jones",     GPA=2.9m, IsActive=true },
            new() { RegistrationNumber="TMS-2026-0003", Name="Charlie Brown", GPA=3.4m, IsActive=false },
            new() { RegistrationNumber="TMS-2026-0004", Name="Diana Prince",  GPA=3.9m, IsActive=true },
            new() { RegistrationNumber="TMS-2026-0005", Name="Evan Wright",   GPA=2.5m, IsActive=true }
        };
        ctx.Students.AddRange(students);

        var courses = new List<Course> {
            new() { Code="CS-101", Title="Introduction to Computer Science", MaxCapacity=30 },
            new() { Code="CS-201", Title="Data Structures and Algorithms",   MaxCapacity=25 },
            new() { Code="MAT-101", Title="Calculus I",                      MaxCapacity=40 }
        };
        ctx.Courses.AddRange(courses);
        ctx.SaveChanges(); // Students and Courses get IDs here [116]

        var enrollments = new List<Enrollment> {
            new() { StudentId=students[0].Id, CourseId=courses[0].Id, Grade=4.0m },
            new() { StudentId=students[0].Id, CourseId=courses[1].Id, Grade=3.6m },
            new() { StudentId=students[1].Id, CourseId=courses[0].Id, Grade=2.8m },
            new() { StudentId=students[3].Id, CourseId=courses[1].Id, Grade=3.9m }
        };
        ctx.Enrollments.AddRange(enrollments);
        ctx.SaveChanges();
    }
}
app.Run();
