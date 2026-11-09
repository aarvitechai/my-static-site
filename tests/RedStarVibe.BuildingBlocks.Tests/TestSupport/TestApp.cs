using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RedStarVibe.BuildingBlocks.Web;
using RedStarVibe.BuildingBlocks.Web.Idempotency;

namespace RedStarVibe.BuildingBlocks.Tests.TestSupport;

internal sealed record CreateThing(string Name, int Quantity);

internal sealed class CreateThingValidator : AbstractValidator<CreateThing>
{
    public CreateThingValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Quantity).InclusiveBetween(1, 10);
    }
}

/// <summary>
/// A real ASP.NET Core pipeline hosted in memory (TestServer): requests go through routing, rate limiting,
/// idempotency and endpoint filters exactly as in the API, without opening a network port.
/// </summary>
internal sealed class TestApp : IAsyncDisposable
{
    private readonly WebApplication _app;

    private TestApp(WebApplication app, InMemoryIdempotencyStore store)
    {
        _app = app;
        Store = store;
        Client = app.GetTestClient();
    }

    public HttpClient Client { get; }

    public InMemoryIdempotencyStore Store { get; }

    public static async Task<TestApp> StartAsync(Action<WebApplication> mapEndpoints, int permitLimit = 1000)
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["RateLimiting:PermitLimit"] = permitLimit.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["RateLimiting:WindowSeconds"] = "60",
        });

        var store = new InMemoryIdempotencyStore();
        builder.Services.AddProblemDetails();
        builder.Services.AddSingleton<IIdempotencyStore>(store);   // registered first, so the Redis default is skipped
        builder.Services.AddBuildingBlocksWeb(builder.Configuration);
        builder.Services.AddScoped<IValidator<CreateThing>, CreateThingValidator>();

        var app = builder.Build();
        app.UseBuildingBlocksWeb();
        mapEndpoints(app);
        await app.StartAsync(TestContext.Current.CancellationToken);
        return new TestApp(app, store);
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await _app.DisposeAsync();
    }
}
