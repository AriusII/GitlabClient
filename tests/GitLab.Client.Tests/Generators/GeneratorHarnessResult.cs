using System.Collections.Immutable;

using Microsoft.CodeAnalysis;

namespace GitLab.Client.Tests.Generators;

/// <summary>
///     What one generator run produced: the merged compilation, the generator's own
///     diagnostics, and every generated file keyed by hint name.
/// </summary>
internal sealed record GeneratorHarnessResult(
    Compilation Compilation,
    ImmutableArray<Diagnostic> GeneratorDiagnostics,
    IReadOnlyDictionary<string, string> GeneratedSources)
{
    public ImmutableArray<Diagnostic> CompilationErrors =>
        Compilation.GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToImmutableArray();

    public string Source(string hintName)
    {
        Assert.True(GeneratedSources.ContainsKey(hintName),
            $"No generated source named '{hintName}'. Produced: {string.Join(", ", GeneratedSources.Keys)}");
        return GeneratedSources[hintName];
    }
}