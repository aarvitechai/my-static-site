using static RedStarVibe.ArchitectureTests.Solution;

namespace RedStarVibe.ArchitectureTests;

/// <summary>ADR-0001 as executable rules. A failure names the offending assembly and the forbidden reference.</summary>
public sealed class DependencyRulesTests
{
    private static readonly string[] FrameworkPrefixes =
    [
        "Microsoft.EntityFrameworkCore",
        "Microsoft.AspNetCore.Http",
        "Microsoft.AspNetCore.Mvc",
        "Microsoft.AspNetCore.Routing",
        "Microsoft.AspNetCore.Builder",
        "Microsoft.AspNetCore.Hosting",
        "Microsoft.Data.SqlClient",
        "StackExchange.Redis",
    ];

    [Fact]
    public void RulesSeeTheExpectedAssemblies()
    {
        // Guards against the rules passing vacuously because nothing was loaded.
        var names = Assemblies.Select(Name).ToList();
        Assert.Contains("RedStarVibe.Api", names);
        Assert.Contains("RedStarVibe.BuildingBlocks", names);
        Assert.Contains("RedStarVibe.Modules.Identity.Domain", names);
        Assert.Contains("RedStarVibe.Modules.Identity.Infrastructure", names);
        Assert.Contains("RedStarVibe.Modules.Identity.Endpoints", names);
    }

    [Fact]
    public void CoreBuildingBlocksHaveNoDependenciesBeyondTheRuntime()
    {
        var core = Assemblies.Single(a => Name(a) == "RedStarVibe.BuildingBlocks");

        AssertNone(
            References(core).Where(r => r != "netstandard" && !r.StartsWith("System.", StringComparison.Ordinal)),
            "RedStarVibe.BuildingBlocks must only use the .NET runtime, so any layer (including Domain) can depend on it.");
    }

    [Fact]
    public void BuildingBlocksNeverDependOnModulesOrHosts()
    {
        AssertNone(
            from assembly in Named(n => n.StartsWith("RedStarVibe.BuildingBlocks", StringComparison.Ordinal))
            from reference in References(assembly)
            where reference.StartsWith("RedStarVibe.Modules.", StringComparison.Ordinal) || reference == "RedStarVibe.Api"
            select $"{Name(assembly)} -> {reference}",
            "Building blocks are shared by everyone and must not know about any module or host.");
    }

    [Fact]
    public void DomainLayersDoNotUseInfrastructureFrameworks()
    {
        AssertNone(
            from assembly in Named(n => ModulePart(n)?.Layer == "Domain")
            from reference in References(assembly)
            where FrameworkPrefixes.Any(p => reference.StartsWith(p, StringComparison.Ordinal))
                  || reference is "RedStarVibe.BuildingBlocks.Persistence" or "RedStarVibe.BuildingBlocks.Web"
            select $"{Name(assembly)} -> {reference}",
            "Domain code must not depend on EF Core, ASP.NET Core HTTP, SQL Server or Redis (only on BuildingBlocks core).");
    }

    [Fact]
    public void LayersOnlyDependInward()
    {
        // Allowed direction: Endpoints -> Application/Infrastructure -> Domain. Never the reverse.
        var forbidden = new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["Domain"] = ["Application", "Infrastructure", "Endpoints"],
            ["Application"] = ["Infrastructure", "Endpoints"],
            ["Infrastructure"] = ["Endpoints"],
        };

        AssertNone(
            from assembly in Assemblies
            let self = ModulePart(Name(assembly))
            where self is not null && forbidden.ContainsKey(self.Value.Layer)
            from reference in References(assembly)
            let target = ModulePart(reference)
            where target is not null && target.Value.Module == self.Value.Module
                  && forbidden[self.Value.Layer].Contains(target.Value.Layer)
            select $"{Name(assembly)} -> {reference}",
            "Module layers must depend inward only (Endpoints -> Application/Infrastructure -> Domain).");
    }

    [Fact]
    public void ModulesDoNotReachIntoOtherModules()
    {
        AssertNone(
            from assembly in Assemblies
            let self = ModulePart(Name(assembly))
            where self is not null
            from reference in References(assembly)
            let target = ModulePart(reference)
            where target is not null && target.Value.Module != self.Value.Module && target.Value.Layer != "Contracts"
            select $"{Name(assembly)} -> {reference}",
            "A module may only use another module through its Contracts project (or integration events).");
    }

    [Fact]
    public void HostOnlyPlugsInModulesThroughTheirEndpoints()
    {
        var host = Assemblies.Single(a => Name(a) == "RedStarVibe.Api");

        AssertNone(
            References(host)
                .Where(r => ModulePart(r) is { Layer: not "Endpoints" }),
            "The API host must reference only a module's Endpoints project (AddXModule / UseXModuleAsync), never its internals.");
    }
}
