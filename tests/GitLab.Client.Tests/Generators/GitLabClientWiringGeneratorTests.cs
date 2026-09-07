namespace GitLab.Client.Tests.Generators;

/// <summary>
///     Drives <c>GitLabClientWiringGenerator</c> over inline compilations. The first two tests pin the
///     cross-generator claim the whole design rests on, in both directions.
/// </summary>
public sealed class GitLabClientWiringGeneratorTests
{
    /// <summary>
    ///     Generated code MAY reference other generated code: both generators' output lands in one
    ///     compilation and is bound together, so the registrations resolve the Service and Controller
    ///     classes the other generator emitted.
    /// </summary>
    [Fact]
    public void BothGenerators_Together_CompileCleanly()
    {
        GeneratorHarnessResult result =
            GeneratorSources.RunBoth(GeneratorSources.Resource(), GeneratorSources.RootScaffoldSource);

        GeneratorSources.AssertNoDiagnostics(result);
        GeneratorSources.AssertCompiles(result);
        Assert.Contains("global::Sample.Services.ThingsService",
            result.Source(GeneratorSources.RegistrationsHintName), StringComparison.Ordinal);
    }

    /// <summary>
    ///     Neither generator can ANALYSE the other's output. With the layer generator absent the emission
    ///     is unchanged and the compiler - not the generator - is the one that complains, which proves the
    ///     wiring generator names those classes rather than resolving them.
    /// </summary>
    [Fact]
    public void WiringGeneratorAlone_LeavesUnresolvedServiceAndControllerTypes()
    {
        GeneratorHarnessResult result =
            GeneratorSources.RunWiring(GeneratorSources.Resource(), GeneratorSources.RootScaffoldSource);

        GeneratorSources.AssertNoDiagnostics(result);
        Assert.Contains("global::Sample.Services.ThingsService",
            result.Source(GeneratorSources.RegistrationsHintName), StringComparison.Ordinal);
        Assert.Contains(result.CompilationErrors,
            diagnostic => diagnostic.GetMessage().Contains("ThingsService", StringComparison.Ordinal));
    }

    [Fact]
    public void RootClientMembersAreExplicitInterfaceImplementations()
    {
        GeneratorHarnessResult result =
            GeneratorSources.RunBoth(GeneratorSources.Resource(), GeneratorSources.RootScaffoldSource);

        // Explicit implementation is what makes the compiler enforce the pairing in both directions:
        // an orphaned attributed repository becomes CS0539 naming the exact member.
        Assert.Contains("global::GitLab.Client.Abstractions.IGitLabClient.Things =>",
            result.Source(GeneratorSources.RootClientHintName), StringComparison.Ordinal);
    }

    [Fact]
    public void AttributedResourceWithNoRootProperty_IsACompileError()
    {
        GeneratorHarnessResult result =
            GeneratorSources.RunBoth(GeneratorSources.Resource(), GeneratorSources.EmptyRootScaffoldSource);

        GeneratorSources.AssertNoDiagnostics(result);
        Assert.Contains(result.CompilationErrors,
            diagnostic => string.Equals(diagnostic.Id, "CS0539", StringComparison.Ordinal));
    }

    [Fact]
    public void Registrations_AreEmittedInOrdinalOrderRegardlessOfSourceOrder()
    {
        string things = GeneratorSources.Resource();
        string gadgets = things.Replace("Things", "Gadgets", StringComparison.Ordinal);
        string scaffold = """
                          namespace GitLab.Client.Abstractions
                          {
                              public interface IGitLabClient
                              {
                                  global::Sample.Abstractions.IThingsClient Things { get; }

                                  global::Sample.Abstractions.IGadgetsClient Gadgets { get; }
                              }
                          }

                          namespace Microsoft.Extensions.DependencyInjection
                          {
                              public static partial class GitLabClientServiceCollectionExtensions
                              {
                                  private static partial void AddResourceClients(
                                      Microsoft.Extensions.DependencyInjection.IServiceCollection services);
                              }
                          }
                          """;

        string forward = GeneratorSources.RunBoth(things, gadgets, scaffold)
            .Source(GeneratorSources.RegistrationsHintName);
        string reversed = GeneratorSources.RunBoth(gadgets, things, scaffold)
            .Source(GeneratorSources.RegistrationsHintName);

        Assert.Equal(forward, reversed, StringComparer.Ordinal);
        Assert.True(forward.IndexOf("GadgetsRepository", StringComparison.Ordinal)
                    < forward.IndexOf("ThingsRepository", StringComparison.Ordinal));
    }

    [Fact]
    public void RegisterFalse_SuppressesTheRegistrationButNotTheRootProperty()
    {
        GeneratorHarnessResult result = GeneratorSources.RunBoth(
            GeneratorSources.Resource(", Register = false"), GeneratorSources.RootScaffoldSource);

        GeneratorSources.AssertNoDiagnostics(result);
        GeneratorSources.AssertCompiles(result);
        Assert.DoesNotContain("ThingsRepository", result.Source(GeneratorSources.RegistrationsHintName),
            StringComparison.Ordinal);
        Assert.Contains("IGitLabClient.Things", result.Source(GeneratorSources.RootClientHintName),
            StringComparison.Ordinal);
    }

    [Fact]
    public void ExposeOnRootClientFalse_SuppressesTheRootPropertyButNotTheRegistration()
    {
        GeneratorHarnessResult result = GeneratorSources.RunBoth(
            GeneratorSources.Resource(", ExposeOnRootClient = false"), GeneratorSources.EmptyRootScaffoldSource);

        GeneratorSources.AssertNoDiagnostics(result);
        GeneratorSources.AssertCompiles(result);
        Assert.Contains("ThingsRepository", result.Source(GeneratorSources.RegistrationsHintName),
            StringComparison.Ordinal);
        Assert.DoesNotContain("IGitLabClient.Things", result.Source(GeneratorSources.RootClientHintName),
            StringComparison.Ordinal);
    }

    [Fact]
    public void RootPropertyName_OverridesTheDerivedName()
    {
        const string Scaffold = """
                                namespace GitLab.Client.Abstractions
                                {
                                    public interface IGitLabClient
                                    {
                                        global::Sample.Abstractions.IThingsClient Widgets { get; }
                                    }
                                }

                                namespace Microsoft.Extensions.DependencyInjection
                                {
                                    public static partial class GitLabClientServiceCollectionExtensions
                                    {
                                        private static partial void AddResourceClients(
                                            Microsoft.Extensions.DependencyInjection.IServiceCollection services);
                                    }
                                }
                                """;

        GeneratorHarnessResult result =
            GeneratorSources.RunBoth(GeneratorSources.Resource(", RootPropertyName = \"Widgets\""), Scaffold);

        GeneratorSources.AssertNoDiagnostics(result);
        GeneratorSources.AssertCompiles(result);
        Assert.Contains("IGitLabClient.Widgets", result.Source(GeneratorSources.RootClientHintName),
            StringComparison.Ordinal);
    }

    [Fact]
    public void Reports_GLC0101_WhenTwoResourcesClaimTheSameRootProperty()
    {
        string things = GeneratorSources.Resource();
        string gadgets = GeneratorSources.Resource(", RootPropertyName = \"Things\"")
            .Replace("Things", "Gadgets", StringComparison.Ordinal)
            .Replace("RootPropertyName = \"Gadgets\"", "RootPropertyName = \"Things\"", StringComparison.Ordinal);

        GeneratorHarnessResult result =
            GeneratorSources.RunWiring(things, gadgets, GeneratorSources.RootScaffoldSource);

        GeneratorSources.AssertDiagnostic(result, "GLC0101");
    }

    [Fact]
    public void Reports_GLC0102_WhenTheClientInterfaceIsNotPublic()
    {
        string source = GeneratorSources.Resource()
            .Replace("public interface IThingsClient", "internal interface IThingsClient",
                StringComparison.Ordinal);

        GeneratorHarnessResult result = GeneratorSources.RunWiring(source, GeneratorSources.EmptyRootScaffoldSource);

        GeneratorSources.AssertDiagnostic(result, "GLC0102");
    }

    [Fact]
    public void Reports_GLC0103_WhenTheRepositoryImplementationIsMissing()
    {
        string source = GeneratorSources.Resource()
            .Replace("internal sealed class ThingsRepository : IThingsRepository",
                "internal sealed class ThingsStore : IThingsRepository", StringComparison.Ordinal);

        GeneratorHarnessResult result = GeneratorSources.RunWiring(source, GeneratorSources.RootScaffoldSource);

        GeneratorSources.AssertDiagnostic(result, "GLC0103");
        Assert.DoesNotContain("ThingsRepository>", result.Source(GeneratorSources.RegistrationsHintName),
            StringComparison.Ordinal);
    }

    [Fact]
    public void Reports_GLC0104_WhenRootPropertyNameIsNotAnIdentifier()
    {
        GeneratorHarnessResult result = GeneratorSources.RunWiring(
            GeneratorSources.Resource(", RootPropertyName = \"not an identifier\""),
            GeneratorSources.EmptyRootScaffoldSource);

        GeneratorSources.AssertDiagnostic(result, "GLC0104");
    }

    /// <summary>
    ///     Declaring the public contract before the repository behind it is a legitimate intermediate
    ///     state, so an orphaned client interface must not be a diagnostic.
    /// </summary>
    [Fact]
    public void OrphanClientInterface_IsNotAnError()
    {
        const string Orphan = """
                              namespace Sample.Abstractions
                              {
                                  public interface IGizmosClient { int Get(int id); }
                              }
                              """;

        GeneratorHarnessResult result = GeneratorSources.RunBoth(GeneratorSources.Resource(), Orphan,
            GeneratorSources.RootScaffoldSource);

        GeneratorSources.AssertNoDiagnostics(result);
        GeneratorSources.AssertCompiles(result);
    }

    [Fact]
    public void RegistrationsUseClosedGenericTryAddSingletonOnly()
    {
        GeneratorHarnessResult result =
            GeneratorSources.RunBoth(GeneratorSources.Resource(), GeneratorSources.RootScaffoldSource);

        // Only the code, never the explanatory comment header - which mentions the very things the
        // generated code must not do.
        string code = string.Join(
            " ",
            result.Source(GeneratorSources.RegistrationsHintName)
                .Split('\n')
                .Where(line => !line.TrimStart().StartsWith("//", StringComparison.Ordinal)));

        // CLAUDE.md's "DI: explicit registration only" rule is about what runs at runtime. Generated or
        // not, every registration must stay a closed-generic call with no reflection behind it.
        Assert.Contains("services.TryAddSingleton<", code, StringComparison.Ordinal);
        Assert.DoesNotContain("typeof(", code, StringComparison.Ordinal);
        Assert.DoesNotContain("Activator", code, StringComparison.Ordinal);
        Assert.DoesNotContain("GetTypes", code, StringComparison.Ordinal);
        Assert.DoesNotContain("Assembly", code, StringComparison.Ordinal);
    }
}