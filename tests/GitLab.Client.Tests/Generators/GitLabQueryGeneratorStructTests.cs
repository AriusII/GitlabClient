using System.Reflection;

using GitLab.Client.SourceGenerators;

using Microsoft.CodeAnalysis.Emit;

namespace GitLab.Client.Tests.Generators;

/// <summary>
///     <c>[GitLabQuery]</c> can annotate a <c>readonly record struct</c> as well as a <c>sealed record</c>
///     class - preparatory support for a later change that converts small <c>*ListOptions</c> types to
///     structs for allocation reasons. Once the annotated type is a struct, the generated method's
///     parameter type <c>T?</c> means <see cref="Nullable{T}" /> rather than a nullable reference, and
///     <see cref="Nullable{T}" /> does not forward member access - so the emitted body must read
///     <c>options.Value.PropertyName</c> there instead of the <c>options.PropertyName</c> a reference-type
///     <c>T</c> still gets (pinned unchanged by <c>GeneratedQueryProjectionTests</c>).
///     <para>
///         A hand-written stand-in for <c>GitLabRouteBuilder</c> is declared alongside the struct here
///         because <see cref="GeneratorTestHarness" /> deliberately excludes the real
///         <c>GitLab.Client</c> assembly from its reference set (see its own remarks) - anything that
///         needs the real routing types has to reference the compiled library instead, which an
///         isolated generator test does not do.
///     </para>
/// </summary>
public sealed class GitLabQueryGeneratorStructTests
{
    private const string RouteBuilderStub = """
                                            namespace GitLab.Client.Infrastructure.Routing
                                            {
                                                public sealed class GitLabRouteBuilder
                                                {
                                                    private readonly System.Text.StringBuilder _text;
                                                    private bool _hasQuery;

                                                    private GitLabRouteBuilder(string root)
                                                    {
                                                        _text = new System.Text.StringBuilder(root);
                                                    }

                                                    public static GitLabRouteBuilder Create(string root)
                                                    {
                                                        return new GitLabRouteBuilder(root);
                                                    }

                                                    public GitLabRouteBuilder Query(string name, int? value)
                                                    {
                                                        if (value is int notNull)
                                                        {
                                                            Append(name, notNull.ToString(System.Globalization.CultureInfo.InvariantCulture));
                                                        }

                                                        return this;
                                                    }

                                                    public GitLabRouteBuilder Query(string name, string? value)
                                                    {
                                                        if (value is not null)
                                                        {
                                                            Append(name, value);
                                                        }

                                                        return this;
                                                    }

                                                    private void Append(string name, string value)
                                                    {
                                                        _text.Append(_hasQuery ? '&' : '?').Append(name).Append('=').Append(value);
                                                        _hasQuery = true;
                                                    }

                                                    public override string ToString()
                                                    {
                                                        return _text.ToString();
                                                    }
                                                }
                                            }
                                            """;

    private const string StructOptionsSource = """
                                               using GitLab.Client.Infrastructure.Routing;

                                               namespace Sample
                                               {
                                                   [GitLab.Client.SourceGenerators.GitLabQuery]
                                                   public readonly record struct WidgetListOptions
                                                   {
                                                       public int? Count { get; init; }

                                                       public string? Name { get; init; }
                                                   }

                                                   public static class Harness
                                                   {
                                                       public static string Invoke()
                                                       {
                                                           WidgetListOptions options = new() { Count = 5, Name = "widget" };
                                                           GitLab.Client.Infrastructure.Routing.GitLabRouteBuilder builder =
                                                               GitLab.Client.Infrastructure.Routing.GitLabRouteBuilder.Create("widgets").QueryFrom(options);
                                                           return builder.ToString();
                                                       }
                                                   }
                                               }
                                               """;

    private const string GeneratedHintName = "WidgetListOptionsQueryExtensions.g.cs";

    [Fact]
    public void QueryFrom_ForAStructOptionsType_AccessesPropertiesThroughValue()
    {
        GeneratorHarnessResult result =
            GeneratorTestHarness.Run([new GitLabQueryGenerator()], StructOptionsSource, RouteBuilderStub);

        GeneratorAssertions.AssertNoDiagnostics(result);
        string generated = result.Source(GeneratedHintName);

        Assert.Contains("options.Value.Count", generated, StringComparison.Ordinal);
        Assert.Contains("options.Value.Name", generated, StringComparison.Ordinal);

        // The reference-type shape ("options.Count", with nothing before it) must not also be emitted -
        // that would not compile once T is Nullable<T>.
        Assert.DoesNotContain("builder.Query(\"count\", options.Count)", generated, StringComparison.Ordinal);
        Assert.DoesNotContain("builder.Query(\"name\", options.Name)", generated, StringComparison.Ordinal);
    }

    [Fact]
    public void QueryFrom_ForAStructOptionsType_CompilesAndProjectsTheSuppliedValues()
    {
        GeneratorHarnessResult result =
            GeneratorTestHarness.Run([new GitLabQueryGenerator()], StructOptionsSource, RouteBuilderStub);

        GeneratorAssertions.AssertNoDiagnostics(result);
        GeneratorAssertions.AssertCompiles(result);

        using MemoryStream assemblyBytes = new();
        EmitResult emitResult = result.Compilation.Emit(
            assemblyBytes, cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(emitResult.Success,
            "Emit failed: " + string.Join(" | ", emitResult.Diagnostics.Select(diagnostic => diagnostic.ToString())));

        Assembly assembly = Assembly.Load(assemblyBytes.ToArray());
        Type harnessType = assembly.GetType("Sample.Harness", true)!;
        MethodInfo invoke = harnessType.GetMethod("Invoke", BindingFlags.Public | BindingFlags.Static)!;

        object? queryString = invoke.Invoke(null, null);

        Assert.Equal("widgets?count=5&name=widget", queryString);
    }
}