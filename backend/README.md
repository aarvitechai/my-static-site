# Backend: ASP.NET Core Web API (.NET 10) + EF Core

## Run

```bash
dotnet run --project Api --launch-profile http     # http://localhost:5121
```

Open `LearningApp.slnx` in Visual Studio / Rider, or use `Api/Api.http` to send test requests.

## Structure

```
LearningApp.slnx         solution file
Api/
├── Controllers/         HTTP endpoints (ProductsController, HealthController)
├── Dtos/                request/response shapes
├── Models/              domain entities
├── Data/                EF Core DbContext + provider switch + design-time factories
├── Migrations/          SqlServer/ and Sqlite/ (one set per provider)
├── Services/            business logic (IProductService + EfProductService)
├── Program.cs           DI registration + middleware pipeline
└── Api.http             ready-made requests
```

## Endpoints

| Method | URL                  | Description          |
|--------|----------------------|----------------------|
| GET    | /api/health          | health check         |
| GET    | /api/products        | list products        |
| GET    | /api/products/{id}   | get one              |
| POST   | /api/products        | create `{name, price}` |
| PUT    | /api/products/{id}   | update               |
| DELETE | /api/products/{id}   | delete               |

OpenAPI document (Development): http://localhost:5121/openapi/v1.json

## Database (EF Core: SQL Server or SQLite)

Pick the provider in `Api/appsettings.json`:

```json
"Database": { "Provider": "SqlServer" },      // or "Sqlite"
"ConnectionStrings": {
  "SqlServer": "Server=DESKTOP-2ERCDGB\\MSSQLSERVER01;Database=LearningAppDb;Trusted_Connection=True;TrustServerCertificate=True",
  "Sqlite": "Data Source=app.db"
}
```

In Development the API applies pending migrations on startup, creating and seeding the database if it doesn't exist.
Override the provider for a single run without editing the file:

```powershell
$env:Database__Provider = "Sqlite"; dotnet run --project Api --launch-profile http
```

**After changing a model**, add a migration for *each* provider (run inside `Api/`):

```bash
dotnet ef migrations add <Name> --context SqlServerDbContext --output-dir Migrations/SqlServer
dotnet ef migrations add <Name> --context SqliteDbContext   --output-dir Migrations/Sqlite
```

Other useful commands: `dotnet ef database update --context SqlServerDbContext`, `dotnet ef migrations remove --context SqliteDbContext`.
