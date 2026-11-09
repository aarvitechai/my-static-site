using Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------- 1. Register services (Dependency Injection) ----------
builder.Services.AddControllers();
builder.Services.AddOpenApi();   // OpenAPI doc at /openapi/v1.json (Development only)

// Singleton so the in-memory list survives between requests.
builder.Services.AddSingleton<IProductService, InMemoryProductService>();

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
}

app.UseHttpsRedirection();
app.UseCors(FrontendCors);
app.UseAuthorization();

app.MapControllers();

app.Run();
