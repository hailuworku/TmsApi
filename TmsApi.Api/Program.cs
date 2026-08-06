// // using Microsoft.AspNetCore.Authentication;
// // using Microsoft.EntityFrameworkCore;
// // using Microsoft.AspNetCore.OpenApi;
// // using Scalar.AspNetCore;
// // using TmsApi.Infrastructure.Persistence;
// // using TmsApi.Application.Interfaces;
// // using TmsApi.Api.Filters;
// // using TmsApi.Api.Authentications;
// // using TmsApi.Api.Middlewares;
// // using TmsApi.Infrastructure.Services;
// // using Asp.Versioning;

// // var builder = WebApplication.CreateBuilder(args);
// // //add versioning

// // builder.Services.AddApiVersioning(options =>
// // {
// //     options.DefaultApiVersion = new ApiVersion(1, 0);
// //     options.AssumeDefaultVersionWhenUnspecified = true;
// //     options.ReportApiVersions = true;
// //     options.ApiVersionReader = new UrlSegmentApiVersionReader();
// // })
// // .AddApiExplorer(options =>
// // {
// //     options.GroupNameFormat = "'v'VVV";
// //     options.SubstituteApiVersionInUrl = true;
// // });

// // builder.Services.AddAuthentication("Training")
// //     .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);
// // builder.Services.AddAuthorization();
// // builder.Services.AddProblemDetails();
// // builder.Services.AddOpenApi();

// // builder.Services.AddControllers(options =>
// // {
// //     options.Filters.Add<AuditLogFilter>();
// // });

// // builder.Services.AddDbContext<TmsDbContext>(options =>
// //     options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase"))
// //         .LogTo(Console.WriteLine, LogLevel.Information)
// //         .EnableSensitiveDataLogging());

// // builder.Services.AddScoped<ICourseService, CourseService>();
// // builder.Services.AddScoped<IStudentService, StudentService>();
// // builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// // var app = builder.Build();

// // app.UseMiddleware<RequestLoggingMiddleware>();
// // app.UseExceptionHandler();
// // app.UseStatusCodePages();
// // app.UseRouting();
// // app.UseAuthentication();
// // app.UseAuthorization();

// // if (app.Environment.IsDevelopment())
// // {
// //     app.MapOpenApi();
// //     app.MapScalarApiReference();
// //     using var scope = app.Services.CreateScope();
// //     var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
// //     await DataSeeder.SeedAsync(context);
// // }
// // if (app.Environment.IsDevelopment())
// // {
// //     app.MapOpenApi();

// //     // ለ v1
// //     app.MapScalarApiReference(options =>
// //     {
// //         options.WithTitle("TMS API v1");
// //     });
// // }

// // app.MapControllers();
// // app.Run();
// //
// using Microsoft.AspNetCore.Authentication;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.AspNetCore.OpenApi;
// using Scalar.AspNetCore;
// using TmsApi.Infrastructure.Persistence;
// using TmsApi.Application.Interfaces;
// using TmsApi.Api.Filters;
// using TmsApi.Api.Authentications;
// using TmsApi.Api.Middlewares;
// using TmsApi.Infrastructure.Services;
// using Asp.Versioning;
// using TmsApi.Api.Middleware;
// using MediatR;
// using FluentValidation;
// using TmsApi.Api.ExceptionHandlers;
// using TmsApi.Application.Behaviors;
// using TmsApi.Application.Enrollments.Commands;
// using Microsoft.AspNetCore.RateLimiting;
// using System.Threading.RateLimiting;
// using TmsApi.Api.RateLimiting;
// using System.Threading.Channels;
// using TmsApi.Application.Transcripts;
// using TmsApi.Infrastructure.Transcripts;
// using Microsoft.Extensions.Hosting;
// using TmsApi.Application.Hubs;
// using TmsApi.Infrastructure.ExternalServices;
// using Polly;
// using OpenTelemetry.Trace;
// using OpenTelemetry.Metrics;
// using TmsApi.Infrastructure.Workers; // We will create this folder next
// var builder = WebApplication.CreateBuilder(args);
// // 1. Versioning ኮንፊገሬሽን (ልክ ነው)
// builder.Services.AddApiVersioning(options =>
// {
//     options.DefaultApiVersion = new ApiVersion(1, 0);
//     options.AssumeDefaultVersionWhenUnspecified = true;
//     options.ReportApiVersions = true;
//     options.ApiVersionReader = new UrlSegmentApiVersionReader();
// })
// .AddApiExplorer(options =>
// {
//     options.GroupNameFormat = "'v'VVV";
//     options.SubstituteApiVersionInUrl = true;
// });

// // 2. OpenAPI ለ v1 እና v2 መመዝገብ (ይህ መስተካከል ነበረበት)
// builder.Services.AddOpenApi("v1");
// builder.Services.AddOpenApi("v2");

// builder.Services.AddAuthentication("Training")
//     .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);
// builder.Services.AddAuthorization();
// builder.Services.AddProblemDetails();
// builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(TmsApi.Application.Common.Result<,>).Assembly));
// builder.Services.AddValidatorsFromAssembly(typeof(TmsApi.Application.Enrollments.Commands.EnrollStudentValidator).Assembly);
// builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TmsApi.Application.Behaviors.LoggingBehavior<,>));
// builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TmsApi.Application.Behaviors.ValidationBehavior<,>));
// builder.Services.AddExceptionHandler<TmsApi.Api.ExceptionHandlers.GlobalExceptionHandler>();

// builder.Services.AddRateLimiter(options =>
// {
//     options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

//     // 1. ተጠቃሚው ከገደብ በላይ ሲጠቀም የሚደርሰው መልእክት
//     options.OnRejected = async (context, token) =>
//     {
//         context.HttpContext.Response.ContentType = "application/problem+json";
//         await context.HttpContext.Response.WriteAsJsonAsync(new Microsoft.AspNetCore.Mvc.ProblemDetails
//         {
//             Status = StatusCodes.Status429TooManyRequests,
//             Title = "Rate limit exceeded",
//             Detail = "Too many requests. Please try again later."
//         }, token);
//     };

//     // 2. የገደብ ህጎቹ (Tiers)
//     options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
//     {
//         var (partitionKey, tier) = ApiKeyResolver.Resolve(httpContext);

//         return tier switch
//         {
//             ApiKeyTier.Paid => RateLimitPartition.GetTokenBucketLimiter(partitionKey, _ => new TokenBucketRateLimiterOptions
//             {
//                 TokenLimit = 200,
//                 TokensPerPeriod = 100,
//                 ReplenishmentPeriod = TimeSpan.FromSeconds(10)
//             }),
//             ApiKeyTier.Free => RateLimitPartition.GetTokenBucketLimiter(partitionKey, _ => new TokenBucketRateLimiterOptions
//             {
//                 TokenLimit = 30,
//                 TokensPerPeriod = 10,
//                 ReplenishmentPeriod = TimeSpan.FromSeconds(10)
//             }),
//             _ => RateLimitPartition.GetTokenBucketLimiter(partitionKey, _ => new TokenBucketRateLimiterOptions
//             {
//                 TokenLimit = 4,
//                 TokensPerPeriod = 2,
//                 ReplenishmentPeriod = TimeSpan.FromSeconds(10)
//             })
//         };
//     });
// });
// builder.Services.AddControllers(options =>
// {
//     options.Filters.Add<AuditLogFilter>();
// });

// builder.Services.AddDbContext<TmsDbContext>(options =>
//     options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase"))
//         .LogTo(Console.WriteLine, LogLevel.Information)
//         .EnableSensitiveDataLogging());

// builder.Services.AddScoped<ICourseService, CourseService>();
// builder.Services.AddScoped<IStudentService, StudentService>();
// builder.Services.AddScoped<ICachedCourseService, CachedCourseService>();
// builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
// builder.Services.AddSignalR();
// builder.Services.AddHybridCache(options =>
// {
//     options.DefaultEntryOptions = new Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryOptions
//     {
//         Expiration = TimeSpan.FromMinutes(10),
//         LocalCacheExpiration = TimeSpan.FromMinutes(2)
//     };
// });

// // ... (other registrations)

// // 1. Register the Status Store as a Singleton
// builder.Services.AddSingleton<ITranscriptStatusStore, InMemoryTranscriptStatusStore>();

// // 2. Create a "Bounded Channel" (This is our thread-safe Queue)
// // It can hold 100 transcript requests at a time.
// builder.Services.AddSingleton(Channel.CreateBounded<TranscriptRequest>(
//     new BoundedChannelOptions(100)
//     {
//         FullMode = BoundedChannelFullMode.Wait
//     }));
// //
// builder.Services.AddResiliencePipeline("certificate-api", pipeline =>
// {
//     pipeline.AddTimeout(TimeSpan.FromSeconds(5))
//             .AddRetry(new Polly.Retry.RetryStrategyOptions { MaxRetryAttempts = 3 });
// });

// builder.Services.AddHttpClient<ICertificateService, CertificateService>(client =>
// {
//     client.BaseAddress = new Uri("http://localhost:5158"); // የራስህ ፖርት
// });
// builder.Services.AddHealthChecks()
//     .AddNpgSql(builder.Configuration.GetConnectionString("TmsDatabase")!);
// // 3. Register the Background Worker (We will create this file in the next step)
// builder.Services.AddHostedService<TranscriptWorker>();
// builder.Services.AddOpenTelemetry()
//     .WithTracing(tracing => tracing
//         .AddAspNetCoreInstrumentation()
//         .AddHttpClientInstrumentation()
//         .AddOtlpExporter())
//     .WithMetrics(metrics => metrics
//         .AddMeter("tms-api")
//         .AddAspNetCoreInstrumentation()
//         .AddRuntimeInstrumentation()
//         .AddOtlpExporter());
// var app = builder.Build();
// builder.Logging.AddJsonConsole(options =>
// {
//     options.IncludeScopes = true;
//     options.JsonWriterOptions = new System.Text.Json.JsonWriterOptions { Indented = true };
// });

// // Middleware ቅደም ተከተል
// app.UseExceptionHandler(); // ExceptionHandler መጀመሪያ መሆን አለበት
// app.UseMiddleware<RequestLoggingMiddleware>();
// app.UseStatusCodePages();
// app.UseHttpsRedirection();
// app.UseMiddleware<V1DeprecationMiddleware>();
// app.UseRouting();
// app.UseRateLimiter();
// app.UseAuthentication();
// app.UseAuthorization();

// // 3. Scalar ማሳያዎችን ማስተካከል

// if (app.Environment.IsDevelopment())
// {
//     app.MapOpenApi();
//     app.MapScalarApiReference(); // Default ማሳያ

//     // Seeder
//     using var scope = app.Services.CreateScope();
//     var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
//     await DataSeeder.SeedAsync(context);
// }
// app.MapHub<TmsHub>("/hubs/tms");
// app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
// {
//     Predicate = check => check.Tags.Contains("live")
// });

// // 2. ዳታቤዙ ጭምር ዝግጁ መሆኑን የሚያሳይ (Readiness)
// app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
// {
//     Predicate = check => check.Tags.Contains("ready")
// });
// app.MapControllers();
// var attempts = 0;
// app.MapPost("/fake/certificates", async () =>
// {
//     var n = Interlocked.Increment(ref attempts);
//     if (n % 3 != 0) return Results.StatusCode(503); // ሆን ብሎ እንዲበላሽ (Transient)
//     return Results.Ok(new { Status = "Issued", Attempt = n });
// });
// app.Run();
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OpenApi;
using Scalar.AspNetCore;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Application.Interfaces;
using TmsApi.Api.Filters;
using TmsApi.Api.Authentications;
using TmsApi.Api.Middlewares;
using TmsApi.Infrastructure.Services;
using Asp.Versioning;
using TmsApi.Api.Middleware;
using MediatR;
using FluentValidation;
using TmsApi.Api.ExceptionHandlers;
using TmsApi.Application.Behaviors;
using TmsApi.Application.Enrollments.Commands;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using TmsApi.Api.RateLimiting;
using System.Threading.Channels;
using TmsApi.Application.Transcripts;
using TmsApi.Infrastructure.Transcripts;
using Microsoft.Extensions.Hosting;
using TmsApi.Application.Hubs;
using TmsApi.Infrastructure.ExternalServices;
using Polly;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using TmsApi.Infrastructure.Workers;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// --- 1. LOGGING (ሁልጊዜ ከላይ ቢሆን ይመረጣል) ---
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
    options.JsonWriterOptions = new System.Text.Json.JsonWriterOptions { Indented = true };
});

// --- 2. CORS (Angular ዳታ እንዲያገኝ የግድ ያስፈልጋል) ---
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:4200") // የAngular አድራሻ
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// --- 3. SERVICES REGISTRATION ---
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddOpenApi("v1");
builder.Services.AddOpenApi("v2");

builder.Services.AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);
builder.Services.AddAuthorization();
builder.Services.AddProblemDetails();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(TmsApi.Application.Interfaces.ICourseService).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(TmsApi.Application.Enrollments.Commands.EnrollStudentValidator).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TmsApi.Application.Behaviors.LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TmsApi.Application.Behaviors.ValidationBehavior<,>));
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "application/problem+json";
        await context.HttpContext.Response.WriteAsJsonAsync(new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = StatusCodes.Status429TooManyRequests,
            Title = "Rate limit exceeded",
            Detail = "Too many requests. Please try again later."
        }, token);
    };

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var (partitionKey, tier) = ApiKeyResolver.Resolve(httpContext);
        return tier switch
        {
            ApiKeyTier.Paid => RateLimitPartition.GetTokenBucketLimiter(partitionKey, _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 200, TokensPerPeriod = 100, ReplenishmentPeriod = TimeSpan.FromSeconds(10)
            }),
            ApiKeyTier.Free => RateLimitPartition.GetTokenBucketLimiter(partitionKey, _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 30, TokensPerPeriod = 10, ReplenishmentPeriod = TimeSpan.FromSeconds(10)
            }),
            _ => RateLimitPartition.GetTokenBucketLimiter(partitionKey, _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 10, TokensPerPeriod = 5, ReplenishmentPeriod = TimeSpan.FromSeconds(10)
            })
        };
    });
});

builder.Services.AddControllers(options => { options.Filters.Add<AuditLogFilter>(); });

builder.Services.AddDbContext<TmsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase"))
        .LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging());

builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ICachedCourseService, CachedCourseService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddSignalR();
builder.Services.AddHybridCache();

builder.Services.AddSingleton<ITranscriptStatusStore, InMemoryTranscriptStatusStore>();
builder.Services.AddSingleton(Channel.CreateBounded<TranscriptRequest>(new BoundedChannelOptions(100) { FullMode = BoundedChannelFullMode.Wait }));

builder.Services.AddResiliencePipeline("certificate-api", pipeline =>
{
    pipeline.AddTimeout(TimeSpan.FromSeconds(5))
            .AddRetry(new Polly.Retry.RetryStrategyOptions { MaxRetryAttempts = 3 });
});

builder.Services.AddHttpClient<ICertificateService, CertificateService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:5158"); // የእራስህን API Port እዚህ ጋር አረጋግጥ
});

builder.Services.AddHealthChecks().AddNpgSql(builder.Configuration.GetConnectionString("TmsDatabase")!);
builder.Services.AddHostedService<TranscriptWorker>();

builder.Services.AddOpenTelemetry()
    .WithTracing(t => t.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddOtlpExporter())
    .WithMetrics(metrics => metrics.AddMeter("tms-api").AddAspNetCoreInstrumentation().AddRuntimeInstrumentation().AddOtlpExporter());

// --- 4. BUILD THE APP ---
var app = builder.Build();

// --- 5. MIDDLEWARE PIPELINE (ቅደም ተከተሉ አስፈላጊ ነው) ---
app.UseExceptionHandler(); 
app.UseCors(); // <--- ይሄ መኖሩን አረጋግጥ!
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseStatusCodePages();
app.UseHttpsRedirection();
app.UseMiddleware<V1DeprecationMiddleware>();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
    await DataSeeder.SeedAsync(context);
}

app.MapHub<TmsHub>("/hubs/tms");
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = check => check.Tags.Contains("live") });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });
app.MapControllers();

app.Run();