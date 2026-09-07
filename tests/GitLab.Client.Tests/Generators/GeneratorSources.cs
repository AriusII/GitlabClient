using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Tests.Generators;

/// <summary>
///     Inline sources and run helpers shared by the generator tests.
///     <para>
///         The split between <see cref="RunLayers" /> and <see cref="RunWiring" /> is load-bearing:
///         <c>GenerateClientLayersGenerator</c> injects <c>GenerateClientLayersAttribute</c> itself
///         through post-initialization, so a source that also declared the attribute would define it
///         twice. When the wiring generator runs alone the attribute has to come from
///         <see cref="AttributeSource" /> instead, or <c>ForAttributeWithMetadataName</c> matches nothing
///         and the test silently passes for the wrong reason.
///     </para>
/// </summary>
internal static class GeneratorSources
{
    /// <summary>
    ///     A hand-written stand-in for the attribute <c>GenerateClientLayersGenerator</c> normally emits,
    ///     for the tests that run the wiring generator on its own.
    /// </summary>
    public const string AttributeSource = """
                                          namespace GitLab.Client.SourceGenerators
                                          {
                                              [System.AttributeUsage(System.AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
                                              internal sealed class GenerateClientLayersAttribute : System.Attribute
                                              {
                                                  public GenerateClientLayersAttribute(System.Type serviceInterface, System.Type clientInterface)
                                                  {
                                                      ServiceInterface = serviceInterface;
                                                      ClientInterface = clientInterface;
                                                  }

                                                  public System.Type ServiceInterface { get; }

                                                  public System.Type ClientInterface { get; }

                                                  public string? ServiceNamespace { get; set; }

                                                  public string? ControllerNamespace { get; set; }

                                                  public bool Register { get; set; } = true;

                                                  public bool ExposeOnRootClient { get; set; } = true;

                                                  public string? RootPropertyName { get; set; }
                                              }
                                          }
                                          """;

    /// <summary>
    ///     The two hand-written anchors the wiring generator emits into: the root aggregate interface it
    ///     implements explicitly, and the partial class carrying the registration method it implements.
    ///     Both live at fixed namespaces, so they are spelled out here exactly as in the real library.
    /// </summary>
    public const string RootScaffoldSource = """
                                             namespace GitLab.Client.Abstractions
                                             {
                                                 public interface IGitLabClient
                                                 {
                                                     global::Sample.Abstractions.IThingsClient Things { get; }
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

    /// <summary>The same scaffold with no resource property, for the opt-out tests.</summary>
    public const string EmptyRootScaffoldSource = """
                                                  namespace GitLab.Client.Abstractions
                                                  {
                                                      public interface IGitLabClient
                                                      {
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

    /// <summary>
    ///     One resource in the shape the real library uses: a public client interface, an internal service
    ///     interface, the attributed repository interface, and its hand-written implementation. The
    ///     namespaces are deliberately NOT the library's own, so the tests also cover deriving
    ///     <c>Sample.Services</c> / <c>Sample.Controllers</c> from <c>Sample.Repositories</c>.
    /// </summary>
    private const string ResourceTemplate = """
                                            namespace Sample.Abstractions
                                            {
                                                public interface IThingsClient
                                                {
                                            $$CLIENTMEMBERS$$
                                                }
                                            }

                                            namespace Sample.Services
                                            {
                                                internal interface IThingsService
                                                {
                                            $$MEMBERS$$
                                                }
                                            }

                                            namespace Sample.Repositories
                                            {
                                                [GitLab.Client.SourceGenerators.GenerateClientLayers(typeof(Sample.Services.IThingsService), typeof(Sample.Abstractions.IThingsClient)$$ATTRIBUTEARGS$$)]
                                                internal interface IThingsRepository
                                                {
                                            $$MEMBERS$$
                                                }

                                                internal sealed class ThingsRepository : IThingsRepository
                                                {
                                            $$IMPLEMENTATION$$
                                                }
                                            }
                                            """;

    public const string ServiceHintName = "Sample.Repositories.IThingsRepository.Service.g.cs";

    public const string ControllerHintName = "Sample.Repositories.IThingsRepository.Controller.g.cs";

    public const string RegistrationsHintName = "ServiceCollectionExtensions.Resources.g.cs";

    public const string RootClientHintName = "GitLabClient.Root.g.cs";

    /// <summary>The default resource: one method, declared identically on all three interfaces.</summary>
    public static string Resource(string attributeArguments = "")
    {
        return ResourceWithMembers("        int Get(int id);", "        public int Get(int id) { return id; }",
            attributeArguments);
    }

    public static string ResourceWithMembers(string members, string implementation,
        string attributeArguments = "")
    {
        return Resource(members, members, implementation, attributeArguments);
    }

    public static string Resource(string members, string clientMembers, string implementation,
        string attributeArguments)
    {
        return ResourceTemplate
            .Replace("$$CLIENTMEMBERS$$", clientMembers, StringComparison.Ordinal)
            .Replace("$$MEMBERS$$", members, StringComparison.Ordinal)
            .Replace("$$IMPLEMENTATION$$", implementation, StringComparison.Ordinal)
            .Replace("$$ATTRIBUTEARGS$$", attributeArguments, StringComparison.Ordinal);
    }

    /// <summary>
    ///     Runs the layer generator alone. It emits the marker attribute itself, so the sources must not
    ///     declare it.
    /// </summary>
    public static GeneratorHarnessResult RunLayers(params string[] sources)
    {
        return GeneratorTestHarness.Run([new GenerateClientLayersGenerator()], sources);
    }

    /// <summary>
    ///     Runs the wiring generator alone, with a hand-written attribute standing in for the one the
    ///     other generator would have injected.
    /// </summary>
    public static GeneratorHarnessResult RunWiring(params string[] sources)
    {
        return GeneratorTestHarness.Run([new GitLabClientWiringGenerator()], [.. sources, AttributeSource]);
    }

    public static GeneratorHarnessResult RunBoth(params string[] sources)
    {
        return GeneratorTestHarness.Run(
            [new GenerateClientLayersGenerator(), new GitLabClientWiringGenerator()], sources);
    }

    public static void AssertDiagnostic(GeneratorHarnessResult result, string id)
    {
        Assert.True(
            result.GeneratorDiagnostics.Any(diagnostic =>
                string.Equals(diagnostic.Id, id, StringComparison.Ordinal)),
            $"Expected {id}; got: {Describe(result)}");
    }

    public static void AssertNoDiagnostics(GeneratorHarnessResult result)
    {
        Assert.True(result.GeneratorDiagnostics.IsEmpty, $"Expected no generator diagnostics; got: {Describe(result)}");
    }

    public static void AssertCompiles(GeneratorHarnessResult result)
    {
        Assert.True(result.CompilationErrors.IsEmpty,
            "Generated code did not compile: " +
            string.Join(" | ", result.CompilationErrors.Select(error => error.ToString())));
    }

    private static string Describe(GeneratorHarnessResult result)
    {
        return string.Join(", ",
            result.GeneratorDiagnostics.Select(diagnostic => diagnostic.Id + ": " + diagnostic.GetMessage()));
    }
}