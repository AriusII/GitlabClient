using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace GitLab.Client.Tests.Generators;

/// <summary>
///     Compiles inline sources against the test host's own reference set and runs any combination of the
///     two generators over them. This is the only way to observe what a generator does when its input is
///     wrong: a broken generator makes the whole test project fail to build, so nothing that fails at
///     generation time can be covered by tests that merely instantiate the generated types.
/// </summary>
internal static class GeneratorTestHarness
{
    private static readonly ImmutableArray<MetadataReference> References = CreateReferences();

    public static GeneratorHarnessResult Run(IEnumerable<IIncrementalGenerator> generators, params string[] sources)
    {
        CSharpParseOptions parseOptions = new(LanguageVersion.Latest);
        CSharpCompilation compilation = CreateCompilation(parseOptions, sources);

        GeneratorDriver driver = CreateDriver(generators, parseOptions);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation output,
            out ImmutableArray<Diagnostic> diagnostics);

        Dictionary<string, string> generated = new(StringComparer.Ordinal);

        foreach (GeneratorRunResult result in driver.GetRunResult().Results)
        {
            foreach (GeneratedSourceResult source in result.GeneratedSources)
            {
                generated[source.HintName] = source.SourceText.ToString();
            }
        }

        return new GeneratorHarnessResult(output, diagnostics, generated);
    }

    public static CSharpCompilation CreateCompilation(CSharpParseOptions parseOptions, params string[] sources)
    {
        return CSharpCompilation.Create(
            "GeneratorHarness",
            sources.Select((source, index) => CSharpSyntaxTree.ParseText(source, parseOptions, $"Source{index}.cs")),
            References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable));
    }

    /// <summary>
    ///     Creates a driver with step tracking on, which is what makes the incrementality assertions
    ///     possible: nothing in a normal build reports that a generator stopped caching.
    /// </summary>
    public static GeneratorDriver CreateDriver(IEnumerable<IIncrementalGenerator> generators,
        CSharpParseOptions parseOptions)
    {
        return CSharpGeneratorDriver.Create(
            generators.Select(generator => generator.AsSourceGenerator()),
            parseOptions: parseOptions,
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, true));
    }

    private static ImmutableArray<MetadataReference> CreateReferences()
    {
        string trusted = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string ?? string.Empty;

        return trusted.Split(Path.PathSeparator)
            .Where(path => path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            // The real GitLab.Client types would collide with the inline test sources, which redeclare
            // IGitLabClient and friends, and every such collision surfaces as CS0436. Anything that needs
            // the real types must reference them instead of redeclaring them - do not relax this filter.
            .Where(path => !Path.GetFileName(path).StartsWith("GitLab.Client", StringComparison.Ordinal))
            .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path))
            .ToImmutableArray();
    }
}