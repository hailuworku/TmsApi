using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore; // 👈 የ Scalar ጥቅል ማስፈንጠሪያ [102]
using TmsApi;            // 👈 የሰርቪስ ክላሶችን ማገናኛ [92]

var builder = WebApplication.CreateBuilder(args);

// 1. የሙከራ Auth ሰርቪሶችን መመዝገብ
builder.Services.AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);
builder.Services.AddAuthorization();

// 2. የሰርቪስ lifetimes መመዝገብ
builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddSingleton<IEnrollmentService, EnrollmentService>();

// 3. ኮንትሮለሮችን (Controllers) መመዝገብ - Page 98
builder.Services.AddControllers();

// 4. የ OpenAPI ሰርቪስ መመዝገብ (ለ Scalar የግድ ያስፈልጋል!) - Page 103
builder.Services.AddOpenApi();

// 5. የ ProblemDetails ሰርቪስ መመዝገብ - Page 102
builder.Services.AddProblemDetails();

// 6. የኮንፊገሬሽን options መመዝገብ
builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

var app = builder.Build();

// 7. Custom Request Logging Middleware መመዝገብ
app.UseMiddleware<RequestLoggingMiddleware>();

// 8. ⚠️ የአካባቢ መቆጣጠሪያ ማስተካከያ (Environment-Aware Configuration) - Page 103
if (app.Environment.IsDevelopment())
{
    // በDevelopment ደረጃ ብቻ የ OpenAPI ሰነድ እና የ Scalar መሞከሪያ ገጽ ይከፈታሉ [103]
    app.MapOpenApi();
    app.MapScalarApiReference();

    // በስራ ወቅት ስህተቶችን በይፋ ማሳያ ገጽ
    app.UseDeveloperExceptionPage();
}
else
{
    // በProduction ደረጃ ብቻ stack trace እንዲደበቅ ExceptionHandler ማጣሪያ እንጠቀማለን [103]
    app.UseExceptionHandler();
}

app.UseStatusCodePages();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// 9. የኮንትሮለር በሮቹን ማገናኘት - Page 98
app.MapControllers();

// 10. ሆን ብሎ ስህተት የሚወረውር የሙከራ ዌብ በር - Page 102
app.MapGet("/api/error", () =>
{
    throw new TmsDatabaseException("Simulated database failure for ProblemDetails testing");
});

app.Run();

// ⚠️ የክላስ መግለጫዎች በሙሉ ሁልጊዜ ከሁሉም በታች መጨረሻ ላይ መሆን አለባቸው! (CS8803 Fix) - Page 102
public class TmsDatabaseException(string message) : Exception(message);