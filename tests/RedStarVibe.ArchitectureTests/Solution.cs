using System.Reflection;

namespace RedStarVibe.ArchitectureTests;

/// <summary>All RedStarVibe production assemblies, loaded from the test output folder.</summary>
internal static class Solution
{
    private const string Prefix = "RedStarVibe.";

    public static IReadOnlyList<Assembly> Assemblies { get; } = Directory
        .GetFiles(AppContext.BaseDirectory, Prefix + "*.dll")
        .Where(path => !Path.GetFileName(path).Contains(".Tests", StringComparison.Ordinal)
                       && !Path.GetFileName(path).Contains("ArchitectureTests", StringComparison.Ordinal))
        .Select(Assembly.LoadFrom)
        .OrderBy(a => a.GetName().Name, StringComparer.Ordinal)
        .ToList();

    public static IEnumerable<Assembly> Named(Func<string, bool> predicate) =>
        Assemblies.Where(a => predicate(Name(a)));

    public static string Name(Assembly assembly) => assembly.GetName().Name!;

    /// <summary>Names of assemblies this assembly actually uses (from its compiled metadata).</summary>
    public static IReadOnlyList<string> References(Assembly assembly) =>
        assembly.GetReferencedAssemblies().Select(r => r.Name!).ToList();

    /// <summary>"RedStarVibe.Modules.Identity.Domain" -> ("Identity", "Domain").</summary>
    public static (string Module, string Layer)? ModulePart(string assemblyName)
    {
        const string modules = Prefix + "Modules.";
        if (!assemblyName.StartsWith(modules, StringComparison.Ordinal))
        {
            return null;
        }

        var parts = assemblyName[modules.Length..].Split('.');
        return parts.Length == 2 ? (parts[0], parts[1]) : null;
    }

    /// <summary>Fails with every violation listed, so one run shows all problems at once.</summary>
    public static void AssertNone(IEnumerable<string> violations, string rule)
    {
        var list = violations.ToList();
        Assert.True(list.Count == 0, $"{rule}\nViolations:\n  " + string.Join("\n  ", list));
    }
}
