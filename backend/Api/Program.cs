using Api.Data;
using Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---------- 1. Register services (Dependency Injection) ----------
builder.Services.AddControllers();
builder.Services.AddOpenApi();   // OpenAPI doc at /openapi/v1.json (Development only)

// Database: provider chosen by "Database:Provider" in appsettings ("SqlServer" or "Sqlite").
builder.Services.AddAppDatabase(builder.Configuration);
// Scoped = one instance per HTTP request, same lifetime as the DbContext.
builder.Services.AddScoped<IProductService, EfProductService>();

// Allow the React dev server to call this API directly (origins come from appsettings.json).
const string FrontendCors = "Frontend";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddPolicy(FrontendCors, policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// ---------- 2. Configure the HTTP pipeline (middleware order matters) ----------
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Learning convenience: apply pending migrations on startup (creates the DB if missing).
    // In production you would run migrations as a separate deployment step instead.
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

app.UseHttpsRedirection();
app.UseCors(FrontendCors);
app.UseAuthorization();

app.MapControllers();

app.Run();
