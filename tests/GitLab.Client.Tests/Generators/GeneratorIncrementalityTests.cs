using GitLab.Client.SourceGenerators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace GitLab.Client.Tests.Generators;

/// <summary>
///     The acceptance tests for the value-equatable pipeline models. Nothing in a normal build reports
///     that a generator stopped caching, so without these the models could silently regress to holding
///     symbols - which both defeats the cache (every keystroke re-runs every resource) and roots whole
///     compilations in the driver for the life of an IDE session.
/// </summary>
public sealed class GeneratorIncrementalityTests
{
    [Fact]
    public void LayerGenerator_CachesEveryTrackedStep_WhenAnUnrelatedTreeIsAdded()
    {
        // The layer generator injects the marker attribute itself, so the sources must not declare it.
        GeneratorRunResult result = RunTwice(new GenerateClientLayersGenerator(), false);

        AssertCached(result, TrackingNames.LayerModels);
        AssertCached(result, TrackingNames.LayerCollisions);
    }

    [Fact]
    public void WiringGenerator_CachesEveryTrackedStep_WhenAnUnrelatedTreeIsAdded()
    {
        GeneratorRunResult result = RunTwice(new GitLabClientWiringGenerator(), true);

        AssertCached(result, TrackingNames.WiringModels);
        AssertCached(result, TrackingNames.SortedWiringModels);
    }

    private static GeneratorRunResult RunTwice(IIncrementalGenerator generator, bool declareAttribute)
    {
        CSharpParseOptions parseOptions = new(LanguageVersion.Latest);
        string[] sources = declareAttribute
            ? [GeneratorSources.Resource(), GeneratorSources.RootScaffoldSource, GeneratorSources.AttributeSource]
            : [GeneratorSources.Resource(), GeneratorSources.RootScaffoldSource];
        CSharpCompilation compilation = GeneratorTestHarness.CreateCompilation(parseOptions, sources);

        GeneratorDriver driver = GeneratorTestHarness.CreateDriver([generator], parseOptions);
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        // An edit that cannot possibly affect any [GenerateClientLayers] interface.
        Compilation edited = compilation.AddSyntaxTrees(
            CSharpSyntaxTree.ParseText("internal sealed class Unrelated { }", parseOptions));
        driver = driver.RunGenerators(edited, TestContext.Current.CancellationToken);

        return driver.GetRunResult().Results[0];
    }

    private static void AssertCached(GeneratorRunResult result, string stepName)
    {
        Assert.True(result.TrackedSteps.ContainsKey(stepName),
            $"No tracked step named '{stepName}'. Tracked: {string.Join(", ", result.TrackedSteps.Keys)}");

        foreach (IncrementalGeneratorRunStep step in result.TrackedSteps[stepName])
        {
            foreach ((object Value, IncrementalStepRunReason Reason) output in step.Outputs)
            {
                Assert.True(
                    output.Reason is IncrementalStepRunReason.Cached or IncrementalStepRunReason.Unchanged,
                    $"Step '{stepName}' re-ran with reason {output.Reason}; the pipeline model is not comparing by value.");
            }
        }
    }
}