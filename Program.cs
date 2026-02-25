using BugPredictionBackend.Background;
using BugPredictionBackend.Configurations;
using BugPredictionBackend.Repositories.Read;
using BugPredictionBackend.Repositories.Sync;
using BugPredictionBackend.Services.Read;
using BugPredictionBackend.Services.Sonar;
using BugPredictionBackend.Services.Sync;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Load optional developer secrets file (secrets.json) before binding configuration.
// secrets.json should contain only development secrets and is ignored by git.
builder.Configuration.AddJsonFile("secrets.json", optional: true, reloadOnChange: true);
// ── Settings ──────────────────────────────────────────────────────────────────
builder.Services.Configure<SonarSettings>(builder.Configuration.GetSection("SonarSettings"));

// ── HTTP Client ───────────────────────────────────────────────────────────────
builder.Services.AddHttpClient("SonarCloud");

// ── Sync Repositories ─────────────────────────────────────────────────────────
builder.Services.AddScoped<ProjectRepository>();
builder.Services.AddScoped<BranchRepository>();
builder.Services.AddScoped<SnapshotRepository>();
builder.Services.AddScoped<ModuleRepository>();
builder.Services.AddScoped<SeverityRepository>();

// ── Read Repositories ─────────────────────────────────────────────────────────
builder.Services.AddScoped<ProjectReadRepository>();
builder.Services.AddScoped<DashboardReadRepository>();
builder.Services.AddScoped<MetricsReadRepository>();
builder.Services.AddScoped<QualityGateReadRepository>();
builder.Services.AddScoped<ScanHistoryReadRepository>();

// ── Sonar API Client ──────────────────────────────────────────────────────────
builder.Services.AddScoped<SonarApiClient>();

// ── Sync Services ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<SyncService>();

// ── Read Services ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<MetricsService>();
builder.Services.AddScoped<QualityGateService>();
builder.Services.AddScoped<ScanHistoryService>();

// ── Background Sync ───────────────────────────────────────────────────────────
builder.Services.AddHostedService<SonarSyncHostedService>();

 //── CORS ──────────────────────────────────────────────────────────────────────
//string[] allowedOrigins = builder.Configuration
//    .GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:4200"];

// ── CORS ──────────────────────────────────────────────────────────────────────
// Allow all origins for development/testing. WARNING: not recommended for production.
builder.Services.AddCors(options =>
    options.AddPolicy("AngularPolicy", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod()
    ));


// ── Controllers & Swagger ─────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "BugPrediction API",
        Version     = "v1",
        Description = "SonarCloud Analytics Platform – Backend REST API"
    });

    // Read [Tags] attribute from the controller class to group endpoints
    c.TagActionsBy(api =>
    {
        if (api.ActionDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor descriptor)
        {
            TagsAttribute? tags = descriptor.ControllerTypeInfo
                .GetCustomAttributes(typeof(TagsAttribute), inherit: true)
                .FirstOrDefault() as TagsAttribute;

            if (tags?.Tags is { Count: > 0 })
                return [tags.Tags[0]];
        }
        return [api.ActionDescriptor.RouteValues["controller"]!];
    });

    // "Sonar to Database" section appears first, "Database to Frontend" second
    c.OrderActionsBy(api =>
    {
        if (api.ActionDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor descriptor)
        {
            TagsAttribute? tags = descriptor.ControllerTypeInfo
                .GetCustomAttributes(typeof(TagsAttribute), inherit: true)
                .FirstOrDefault() as TagsAttribute;

            return tags?.Tags?.FirstOrDefault() == "Sonar to Database" ? "1" : "2";
        }
        return "2";
    });
});

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BugPrediction API v1");
        c.DocumentTitle = "BugPrediction API";
        c.DefaultModelsExpandDepth(-1);
    });
}

app.UseHttpsRedirection();
app.UseCors("AngularPolicy");
app.UseAuthorization();
app.MapControllers();
app.Run();
