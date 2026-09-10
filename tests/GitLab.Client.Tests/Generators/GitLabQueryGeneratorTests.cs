using System.Collections.Immutable;

using GitLab.Client.SourceGenerators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace GitLab.Client.Tests.Generators;

/// <summary>
///     Regression coverage for the generator's source-level invariants. These tests compile miniature
///     consumers so they cover the cases ordinary endpoint tests cannot observe: duplicate short names,
///     nested generic type syntax and malformed query metadata.
/// </summary>
public sealed class GitLabQueryGeneratorTests
{
    private const string RouteBuilderStub = """
                                            namespace GitLab.Client.Infrastructure.Routing
                                            {
                                                public sealed class GitLabRouteBuilder
                                                {
                                                    public GitLabRouteBuilder Query(string name, string? value) => this;

                                                    public GitLabRouteBuilder Query(string name, int? value) => this;

                                                    public GitLabRouteBuilder Query(string name, long? value) => this;

                                                    public GitLabRouteBuilder Query(string name, bool? value) => this;

                                                    public GitLabRouteBuilder Query(string name, System.DateTimeOffset? value) => this;

                                                    public GitLabRouteBuilder Query(string name, System.DateOnly? value) => this;

                                                    public GitLabRouteBuilder Query(string name,
                                                        System.Collections.Generic.IReadOnlyList<string>? value) => this;

                                                    public GitLabRouteBuilder Query(string name,
                                                        System.Collections.Generic.IReadOnlyList<long>? value) => this;

                                                    public GitLabRouteBuilder QueryRepeated(string name,
                                                        System.Collections.Generic.IReadOnlyList<string>? value) => this;

                                                    public GitLabRouteBuilder QueryRepeated(string name,
                                                        System.Collections.Generic.IReadOnlyList<long>? value) => this;
                                                }
                                            }
                                            """;

    [Fact]
    public void QueryFrom_HomonymousOptionsAcrossNamespaces_UsesDistinctGeneratedDeclarations()
    {
        const string source = """
                              namespace Alpha
                              {
                                  [GitLab.Client.SourceGenerators.GitLabQuery]
                                  public sealed class DuplicateOptions
                                  {
                                      public string? Search { get; init; }
                                  }
                              }

                              namespace Beta
                              {
                                  [GitLab.Client.SourceGenerators.GitLabQuery]
                                  public sealed class DuplicateOptions
                                  {
                                      public string? Search { get; init; }
                                  }
                              }
                              """;

        GeneratorHarnessResult result =
            GeneratorTestHarness.Run([new GitLabQueryGenerator()], source, RouteBuilderStub);

        GeneratorAssertions.AssertNoDiagnostics(result);
        GeneratorAssertions.AssertCompiles(result);

        string[] optionsSources = result.GeneratedSources
            .Where(static pair => pair.Key.EndsWith("QueryExtensions.g.cs", StringComparison.Ordinal))
            .Select(static pair => pair.Value)
            .ToArray();

        Assert.Equal(2, optionsSources.Length);
        Assert.Equal(2, result.GeneratedSources.Keys.Count(static key =>
            key.EndsWith("QueryExtensions.g.cs", StringComparison.Ordinal)));
        Assert.Contains(optionsSources, static generated =>
            generated.Contains("global::Alpha.DuplicateOptions? options", StringComparison.Ordinal));
        Assert.Contains(optionsSources, static generated =>
            generated.Contains("global::Beta.DuplicateOptions? options", StringComparison.Ordinal));
    }

    [Fact]
    public void QueryFrom_NestedGenericOptions_PreservesAllTypeParametersAndConstraints()
    {
        const string source = """
                              namespace Sample
                              {
                                  public sealed class Container<T>
                                      where T : class, new()
                                  {
                                      [GitLab.Client.SourceGenerators.GitLabQuery]
                                      public sealed class NestedOptions<TValue>
                                          where TValue : struct
                                      {
                                          public string? Search { get; init; }
                                      }
                                  }
                              }
                              """;

        GeneratorHarnessResult result =
            GeneratorTestHarness.Run([new GitLabQueryGenerator()], source, RouteBuilderStub);

        GeneratorAssertions.AssertNoDiagnostics(result);
        GeneratorAssertions.AssertCompiles(result);

        string generated = result.GeneratedSources.Single(static pair =>
            pair.Key.EndsWith("QueryExtensions.g.cs", StringComparison.Ordinal)).Value;

        Assert.Contains("QueryFrom<T, TValue>", generated, StringComparison.Ordinal);
        Assert.Contains("global::Sample.Container<T>.NestedOptions<TValue>? options", generated,
            StringComparison.Ordinal);
        Assert.Contains("where T : class, new()", generated, StringComparison.Ordinal);
        Assert.Contains("where TValue : struct", generated, StringComparison.Ordinal);
    }

    [Fact]
    public void QueryFrom_NestedEnumWithEscapedWireValue_CompilesAndEscapesBothIdentifiersAndLiterals()
    {
        const string source = """
                              using System.Text.Json.Serialization;

                              namespace Sample
                              {
                                  public static class Outer
                                  {
                                      [JsonConverter(typeof(JsonStringEnumConverter<InnerState>))]
                                      public enum InnerState
                                      {
                                          [JsonStringEnumMemberName("quoted\"value\\path")]
                                          @class
                                      }
                                  }

                                  [GitLab.Client.SourceGenerators.GitLabQuery]
                                  public sealed class Options
                                  {
                                      public Outer.InnerState? State { get; init; }
                                  }
                              }
                              """;

        GeneratorHarnessResult result =
            GeneratorTestHarness.Run([new GitLabQueryGenerator()], source, RouteBuilderStub);

        GeneratorAssertions.AssertNoDiagnostics(result);
        GeneratorAssertions.AssertCompiles(result);

        string enumHelper = result.GeneratedSources.Single(static pair =>
            pair.Key.EndsWith("QueryValues.g.cs", StringComparison.Ordinal)).Value;

        Assert.Contains("global::Sample.Outer.InnerState.@class", enumHelper, StringComparison.Ordinal);
        Assert.Contains("return \"quoted\\\"value\\\\path\";", enumHelper, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("GLQ0001", """
                           [GitLab.Client.SourceGenerators.GitLabQuery]
                           public sealed class Options
                           {
                               public decimal? Unsupported { get; init; }
                           }
                           """)]
    [InlineData("GLQ0002", """
                           [GitLab.Client.SourceGenerators.GitLabQuery]
                           public sealed class Options
                           {
                               public int AlwaysSent { get; init; }
                           }
                           """)]
    [InlineData("GLQ0003", """
                           [GitLab.Client.SourceGenerators.GitLabQuery]
                           public sealed class Options
                           {
                               [GitLab.Client.SourceGenerators.QueryParameter("contains a space")]
                               public string? InvalidName { get; init; }
                           }
                           """)]
    [InlineData("GLQ0005", """
                           [GitLab.Client.SourceGenerators.GitLabQuery]
                           public sealed class Options
                           {
                               [GitLab.Client.SourceGenerators.QueryParameter("same")]
                               public string? First { get; init; }

                               [GitLab.Client.SourceGenerators.QueryParameter("same")]
                               public string? Second { get; init; }
                           }
                           """)]
    [InlineData("GLQ0006", """
                           [GitLab.Client.SourceGenerators.GitLabQuery]
                           public sealed class Options
                           {
                           }
                           """)]
    [InlineData("GLQ0007", """
                           [GitLab.Client.SourceGenerators.GitLabQuery]
                           public sealed class Options
                           {
                               [GitLab.Client.SourceGenerators.QueryParameter(
                                   MultiValue = GitLab.Client.SourceGenerators.QueryMultiValueStyle.Repeated)]
                               public string? NotACollection { get; init; }
                           }
                           """)]
    public void QueryMetadata_InvalidOptions_ReportsOnlyTheExpectedDiagnostic(string diagnosticId, string source)
    {
        GeneratorHarnessResult result =
            GeneratorTestHarness.Run([new GitLabQueryGenerator()], source, RouteBuilderStub);

        AssertOnlyQueryDiagnostic(result, diagnosticId);
        GeneratorAssertions.AssertCompiles(result);
    }

    [Fact]
    public void StringEnumMemberWithoutExplicitWireValue_ReportsGlq0004AndKeepsGeneratedSourceCompilable()
    {
        const string source = """
                              using System.Text.Json.Serialization;

                              [JsonConverter(typeof(JsonStringEnumConverter<UnmappedState>))]
                              public enum UnmappedState
                              {
                                  Missing
                              }

                              [GitLab.Client.SourceGenerators.GitLabQuery]
                              public sealed class Options
                              {
                                  public UnmappedState? State { get; init; }
                              }
                              """;

        GeneratorHarnessResult result =
            GeneratorTestHarness.Run([new GitLabQueryGenerator()], source, RouteBuilderStub);

        AssertOnlyQueryDiagnostic(result, "GLQ0004");
        GeneratorAssertions.AssertCompiles(result);
    }

    [Fact]
    public void QueryGenerator_UnrelatedSyntaxTreeLeavesModelUnchangedAndSourceInputCached()
    {
        const string querySource = """
                                   [GitLab.Client.SourceGenerators.GitLabQuery]
                                   public sealed class Options
                                   {
                                       public string? Search { get; init; }
                                   }
                                   """;
        const string unrelatedSource = "namespace Unrelated { public sealed class Added { } }";

        CSharpParseOptions parseOptions = new(LanguageVersion.CSharp14);
        CSharpCompilation compilation =
            GeneratorTestHarness.CreateCompilation(parseOptions, querySource, RouteBuilderStub);
        GeneratorDriver driver = GeneratorTestHarness.CreateDriver([new GitLabQueryGenerator()], parseOptions);
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _, cancellationToken);

        SyntaxTree unrelatedTree = CSharpSyntaxTree.ParseText(
            unrelatedSource,
            parseOptions,
            "Unrelated.cs",
            cancellationToken: cancellationToken);
        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation.AddSyntaxTrees(unrelatedTree),
            out _,
            out _,
            cancellationToken);

        GeneratorRunResult run = Assert.Single(driver.GetRunResult().Results);
        AssertTrackedOutputs(run, "GitLabQuery.OptionsModel", IncrementalStepRunReason.Unchanged);
        AssertTrackedOutputs(run, "GitLabQuery.OptionsSourceInput", IncrementalStepRunReason.Cached);
    }

    [Fact]
    public void QueryGenerator_AddingDistinctOptionsType_MarksExistingSourceInputUnchanged()
    {
        const string existingSource = """
                                      namespace Alpha
                                      {
                                          [GitLab.Client.SourceGenerators.GitLabQuery]
                                          public sealed class ExistingOptions
                                          {
                                              public string? Search { get; init; }
                                          }
                                      }
                                      """;
        const string addedSource = """
                                   namespace Beta
                                   {
                                       [GitLab.Client.SourceGenerators.GitLabQuery]
                                       public sealed class AddedOptions
                                       {
                                           public int? Page { get; init; }
                                       }
                                   }
                                   """;

        CSharpParseOptions parseOptions = new(LanguageVersion.CSharp14);
        CSharpCompilation compilation =
            GeneratorTestHarness.CreateCompilation(parseOptions, existingSource, RouteBuilderStub);
        GeneratorDriver driver = GeneratorTestHarness.CreateDriver([new GitLabQueryGenerator()], parseOptions);
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _, cancellationToken);

        SyntaxTree addedTree = CSharpSyntaxTree.ParseText(
            addedSource,
            parseOptions,
            "AddedOptions.cs",
            cancellationToken: cancellationToken);
        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation.AddSyntaxTrees(addedTree),
            out _,
            out _,
            cancellationToken);

        GeneratorRunResult run = Assert.Single(driver.GetRunResult().Results);
        ImmutableArray<(object Value, IncrementalStepRunReason Reason)> outputs = run
            .TrackedSteps["GitLabQuery.OptionsSourceInput"]
            .SelectMany(static step => step.Outputs)
            .ToImmutableArray();

        // The new type has to produce one new output. The collection upstream necessarily changes, so
        // Roslyn labels the equal projection for the existing type Unchanged rather than Cached; either
        // way it is not re-rendered. This is the hot incremental-build invariant: changing one
        // endpoint's options cannot re-render all of the SDK's query projections.
        Assert.Contains(outputs, static output => output.Reason == IncrementalStepRunReason.Unchanged);
        Assert.Contains(outputs, static output => output.Reason == IncrementalStepRunReason.New);
    }

    private static void AssertTrackedOutputs(
        GeneratorRunResult run,
        string trackingName,
        IncrementalStepRunReason expectedReason)
    {
        Assert.True(run.TrackedSteps.TryGetValue(trackingName, out ImmutableArray<IncrementalGeneratorRunStep> steps),
            $"The '{trackingName}' pipeline stage must remain tracked so its incremental invariant is observable.");

        (object Value, IncrementalStepRunReason Reason)[] outputs =
            steps.SelectMany(static step => step.Outputs).ToArray();
        Assert.NotEmpty(outputs);
        Assert.All(outputs, output => Assert.Equal(expectedReason, output.Reason));
    }

    private static void AssertOnlyQueryDiagnostic(GeneratorHarnessResult result, string expectedId)
    {
        Diagnostic[] queryDiagnostics = result.GeneratorDiagnostics
            .Where(static diagnostic => diagnostic.Id.StartsWith("GLQ", StringComparison.Ordinal))
            .ToArray();

        Diagnostic diagnostic = Assert.Single(queryDiagnostics);
        Assert.Equal(expectedId, diagnostic.Id);
    }
}