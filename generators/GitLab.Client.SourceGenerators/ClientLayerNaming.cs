using Microsoft.CodeAnalysis.CSharp;

namespace GitLab.Client.SourceGenerators;

/// <summary>
///     The naming and namespace conventions shared by <see cref="GenerateClientLayersGenerator" />
///     (which emits the <c>&lt;Resource&gt;Service</c> / <c>&lt;Resource&gt;Controller</c> classes) and
///     <see cref="GitLabClientWiringGenerator" /> (which emits DI registrations naming those very
///     classes). Roslyn hands every generator the compilation as it stood <em>before</em> generation, so
///     the wiring generator provably cannot look the layer classes up - it can only compose their names.
///     These helpers are therefore the only thing keeping the two generators in agreement: change one
///     convention here and both generators move together.
/// </summary>
internal static class ClientLayerNaming
{
    public const string AttributeFullName = "GitLab.Client.SourceGenerators.GenerateClientLayersAttribute";

    public const string ServicesLayerSegment = "Services";

    public const string ControllersLayerSegment = "Controllers";

    public const string ServiceSuffix = "Service";

    public const string ControllerSuffix = "Controller";

    public const string RepositorySuffix = "Repository";

    public const string RootClientInterface = "global::GitLab.Client.Abstractions.IGitLabClient";

    public const string RootClientClassName = "GitLabClient";

    public const string RootClientNamespace = "GitLab.Client.Controllers";

    public const string DependencyInjectionNamespace = "Microsoft.Extensions.DependencyInjection";

    public const string RegistrationClassName = "GitLabClientServiceCollectionExtensions";

    public const string RegistrationMethodName = "AddResourceClients";

    private const string RepositoriesLayerSegment = "Repositories";

    /// <summary>Maps <c>IProjectsRepository</c> to <c>Projects</c>.</summary>
    public static string GetResourceName(string repositoryInterfaceName)
    {
        string name = repositoryInterfaceName;

        if (name.Length > 1 && name[0] == 'I' && char.IsUpper(name[1]))
        {
            name = name.Substring(1);
        }

        if (name.Length > RepositorySuffix.Length && name.EndsWith(RepositorySuffix, StringComparison.Ordinal))
        {
            name = name.Substring(0, name.Length - RepositorySuffix.Length);
        }

        return name;
    }

    public static string GetRepositoryImplementationName(string resourceName)
    {
        return resourceName + RepositorySuffix;
    }

    /// <summary>
    ///     Maps the repository interface's namespace onto the equivalent layer namespace:
    ///     <c>GitLab.Client.Repositories</c> becomes <c>GitLab.Client.Services</c>, and a future
    ///     <c>GitLab.Client.Repositories.Ci</c> becomes <c>GitLab.Client.Services.Ci</c>. A namespace with
    ///     no <c>Repositories</c> segment simply gains the layer segment, so the convention degrades
    ///     predictably rather than dumping every resource into one flat namespace.
    /// </summary>
    public static string DeriveLayerNamespace(string repositoryNamespace, string layerSegment)
    {
        string[] segments = repositoryNamespace.Split('.');

        for (int index = segments.Length - 1; index >= 0; index--)
        {
            if (string.Equals(segments[index], RepositoriesLayerSegment, StringComparison.Ordinal))
            {
                segments[index] = layerSegment;
                return string.Join(".", segments);
            }
        }

        return repositoryNamespace + "." + layerSegment;
    }

    public static string ComposeGlobalTypeName(string namespaceName, string typeName)
    {
        return "global::" + namespaceName + "." + typeName;
    }

    public static string ToCamelCase(string name)
    {
        if (name.Length == 0)
        {
            return name;
        }

        return char.ToLowerInvariant(name[0]) + name.Substring(1);
    }

    /// <summary>
    ///     Prefixes <c>@</c> only where C# actually needs it. GitLab tag names drive resource names, and a
    ///     future <c>Lock</c>, <c>Base</c>, <c>Event</c> or <c>Namespace</c> resource would otherwise
    ///     camel-case straight into a keyword and produce a syntax error inside generated code.
    /// </summary>
    public static string EscapeIdentifier(string name)
    {
        return SyntaxFacts.GetKeywordKind(name) == SyntaxKind.None ? name : "@" + name;
    }
}