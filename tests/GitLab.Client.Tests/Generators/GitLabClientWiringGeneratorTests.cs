using GitLab.Client.SourceGenerators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace GitLab.Client.Tests.Generators;

/// <summary>Tests the direct endpoint composition generator independently of the compiled client package.</summary>
public sealed class GitLabClientWiringGeneratorTests
{
    private const string RegistrationsHintName = "GitLabClient.Registrations.g.cs";
    private const string RootClientHintName = "GitLabClient.Root.g.cs";

    private const string DirectEndpoints = """
                                           namespace Microsoft.Extensions.DependencyInjection
                                           {
                                               public interface IServiceCollection { }

                                               public static partial class GitLabClientServiceCollectionExtensions
                                               {
                                                   private static partial void AddResourceClients(IServiceCollection services);
                                               }
                                           }

                                           namespace Microsoft.Extensions.DependencyInjection.Extensions
                                           {
                                               public static class ServiceCollectionDescriptorExtensions
                                               {
                                                   public static void TryAddSingleton<TService, TImplementation>(
                                                       this global::Microsoft.Extensions.DependencyInjection.IServiceCollection services)
                                                   {
                                                   }
                                               }
                                           }

                                           namespace GitLab.Client.Abstractions
                                           {
                                               public interface IThingsClient { }
                                               public interface IGadgetsClient { }

                                               public interface IGitLabClient
                                               {
                                                   IGadgetsClient Gadgets { get; }
                                                   IThingsClient Things { get; }
                                               }
                                           }

                                           namespace GitLab.Client.Endpoints
                                           {
                                               internal sealed class ThingsClient : global::GitLab.Client.Abstractions.IThingsClient { }
                                               internal sealed class GadgetsClient : global::GitLab.Client.Abstractions.IGadgetsClient { }
                                           }
                                           """;

    [Fact]
    public void EmitsClosedDirectEndpointRegistrationsAndExplicitRootPropertiesInOrdinalOrder()
    {
        GeneratorHarnessResult result = GeneratorTestHarness.Run([new GitLabClientWiringGenerator()], DirectEndpoints);

        GeneratorAssertions.AssertNoDiagnostics(result);
        GeneratorAssertions.AssertCompiles(result);

        string registrations = result.Source(RegistrationsHintName);
        string root = result.Source(RootClientHintName);

        Assert.Contains(
            "TryAddSingleton<global::GitLab.Client.Abstractions.IGadgetsClient, global::GitLab.Client.Endpoints.GadgetsClient>()",
            registrations, StringComparison.Ordinal);
        Assert.Contains("global::GitLab.Client.Abstractions.IGitLabClient.Gadgets => _gadgets",
            root, StringComparison.Ordinal);
        Assert.True(registrations.IndexOf("IGadgetsClient", StringComparison.Ordinal) <
                    registrations.IndexOf("IThingsClient", StringComparison.Ordinal));
        Assert.DoesNotContain("GitLab.Client.Repositories.", registrations, StringComparison.Ordinal);
        Assert.DoesNotContain("GitLab.Client.Services.", registrations, StringComparison.Ordinal);
        Assert.DoesNotContain("GitLab.Client.Controllers.", registrations, StringComparison.Ordinal);
    }

    [Fact]
    public void ReportsAMissingOrIncompatibleDirectEndpointAtGenerationTime()
    {
        string missingEndpoint = DirectEndpoints.Replace(
            "internal sealed class GadgetsClient : global::GitLab.Client.Abstractions.IGadgetsClient { }",
            "internal sealed class GadgetsStore { }", StringComparison.Ordinal);

        GeneratorHarnessResult result = GeneratorTestHarness.Run([new GitLabClientWiringGenerator()], missingEndpoint);

        GeneratorAssertions.AssertDiagnostic(result, "GLC0202");
    }

    [Fact]
    public void ReportsWhyAResourcePropertyWithASetterIsInvalid()
    {
        string mutableProperty = DirectEndpoints.Replace(
            "IThingsClient Things { get; }",
            "IThingsClient Things { get; set; }",
            StringComparison.Ordinal);

        GeneratorHarnessResult result = GeneratorTestHarness.Run([new GitLabClientWiringGenerator()], mutableProperty);

        GeneratorAssertions.AssertDiagnostic(result, "GLC0201");
        AssertInvalidRootPropertyDiagnostic(result, "a setter is not allowed");
    }

    [Fact]
    public void ReportsWhyAStaticResourcePropertyIsInvalid()
    {
        string staticProperty = DirectEndpoints.Replace(
            "IThingsClient Things { get; }",
            "static abstract IThingsClient Things { get; }",
            StringComparison.Ordinal);

        GeneratorHarnessResult result = GeneratorTestHarness.Run([new GitLabClientWiringGenerator()], staticProperty);

        GeneratorAssertions.AssertDiagnostic(result, "GLC0201");
        AssertInvalidRootPropertyDiagnostic(result, "static properties are not supported");
    }

    [Fact]
    public void ReportsWhyAnIndexedResourcePropertyIsInvalid()
    {
        string indexedProperty = DirectEndpoints.Replace(
            "IThingsClient Things { get; }",
            "IThingsClient this[int index] { get; }",
            StringComparison.Ordinal);

        GeneratorHarnessResult result = GeneratorTestHarness.Run([new GitLabClientWiringGenerator()], indexedProperty);

        GeneratorAssertions.AssertDiagnostic(result, "GLC0201");
        AssertInvalidRootPropertyDiagnostic(result, "indexers are not supported");
    }

    [Fact]
    public void ReportsWhyAResourcePropertyWithoutAGetterIsInvalid()
    {
        string writeOnlyProperty = DirectEndpoints.Replace(
            "IThingsClient Things { get; }",
            "IThingsClient Things { set; }",
            StringComparison.Ordinal);

        GeneratorHarnessResult result =
            GeneratorTestHarness.Run([new GitLabClientWiringGenerator()], writeOnlyProperty);

        GeneratorAssertions.AssertDiagnostic(result, "GLC0201");
        AssertInvalidRootPropertyDiagnostic(result, "a getter is required");
    }

    [Fact]
    public void EscapesAKeywordNamedResourcePropertyInTheExplicitRootImplementation()
    {
        string keywordProperty = DirectEndpoints.Replace(
            "IThingsClient Things { get; }",
            "IThingsClient @class { get; }",
            StringComparison.Ordinal);

        GeneratorHarnessResult result = GeneratorTestHarness.Run([new GitLabClientWiringGenerator()], keywordProperty);

        GeneratorAssertions.AssertNoDiagnostics(result);
        GeneratorAssertions.AssertCompiles(result);
        Assert.Contains("IGitLabClient.@class => _class", result.Source(RootClientHintName), StringComparison.Ordinal);
    }

    [Fact]
    public void EscapesAContextualKeywordNamedResourcePropertyInTheExplicitRootImplementation()
    {
        string contextualKeywordProperty = DirectEndpoints.Replace(
            "IThingsClient Things { get; }",
            "IThingsClient required { get; }",
            StringComparison.Ordinal);

        GeneratorHarnessResult result =
            GeneratorTestHarness.Run([new GitLabClientWiringGenerator()], contextualKeywordProperty);

        GeneratorAssertions.AssertNoDiagnostics(result);
        GeneratorAssertions.AssertCompiles(result);
        Assert.Contains("IGitLabClient.@required => _required", result.Source(RootClientHintName),
            StringComparison.Ordinal);
    }

    [Fact]
    public void CachesTheWiringInputWhenAnUnrelatedSyntaxTreeIsAdded()
    {
        CSharpParseOptions parseOptions = new(LanguageVersion.CSharp14);
        CSharpCompilation compilation = GeneratorTestHarness.CreateCompilation(parseOptions, DirectEndpoints);
        GeneratorDriver driver = GeneratorTestHarness.CreateDriver([new GitLabClientWiringGenerator()], parseOptions);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _,
            TestContext.Current.CancellationToken);

        SyntaxTree unrelatedTree = CSharpSyntaxTree.ParseText(
            "namespace Unrelated; internal sealed class UnrelatedType { }",
            parseOptions,
            "Unrelated.cs", null, TestContext.Current.CancellationToken);
        CSharpCompilation updatedCompilation = compilation.AddSyntaxTrees(unrelatedTree);
        driver = driver.RunGeneratorsAndUpdateCompilation(updatedCompilation, out _, out _,
            TestContext.Current.CancellationToken);

        GeneratorRunResult result = Assert.Single(driver.GetRunResult().Results);
        IncrementalGeneratorRunStep input = Assert.Single(result.TrackedSteps["GitLabClientWiring.Input"]);
        Assert.All(input.Outputs,
            static output => Assert.Equal(IncrementalStepRunReason.Cached, output.Reason));
    }

    private static void AssertInvalidRootPropertyDiagnostic(GeneratorHarnessResult result, string reason)
    {
        Diagnostic diagnostic = Assert.Single(result.GeneratorDiagnostics.Where(static candidate =>
            string.Equals(candidate.Id, "GLC0201", StringComparison.Ordinal)));
        Assert.Contains(reason, diagnostic.GetMessage(), StringComparison.Ordinal);
    }
}