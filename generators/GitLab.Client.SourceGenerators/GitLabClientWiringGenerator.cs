using System.Collections.Immutable;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace GitLab.Client.SourceGenerators;

/// <summary>
///     Emits the compile-time composition root for the direct endpoint architecture. The public
///     <c>IGitLabClient</c> interface is the single, reviewable inventory of resource clients; every
///     <c>I&lt;Resource&gt;Client</c> property maps to the hand-written
///     <c>GitLab.Client.Endpoints.&lt;Resource&gt;Client</c> implementation.
///     <para>
///         The generator deliberately analyses only types already present in the facade compilation. It does
///         not depend on output of another source generator, perform assembly scanning, or introduce runtime
///         reflection. A missing or incompatible endpoint is an actionable compiler diagnostic rather than a
///         service-provider failure at runtime.
///     </para>
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class GitLabClientWiringGenerator : IIncrementalGenerator
{
    private const string EndpointNamespace = "GitLab.Client.Endpoints";
    private const string GeneratorName = "GitLab.Client.SourceGenerators";
    private const string GeneratorVersion = "2.0.0";

    private static readonly SymbolDisplayFormat TypeFormat = SymbolDisplayFormat.FullyQualifiedFormat;

    private static readonly DiagnosticDescriptor InvalidResourceClientProperty = new(
        "GLC0201",
        "Root client property must expose a public resource-client interface",
        "Property '{0}' on IGitLabClient is invalid: {1}. It must be a public, non-generic I<Resource>Client " +
        "interface instance property with a getter and no setter.",
        "GitLab.Client.Wiring",
        DiagnosticSeverity.Error,
        true);

    private static readonly DiagnosticDescriptor MissingEndpointImplementation = new(
        "GLC0202",
        "Direct endpoint implementation is missing or incompatible",
        "Property '{0}' requires endpoint '{1}', a non-abstract class implementing '{2}'",
        "GitLab.Client.Wiring",
        DiagnosticSeverity.Error,
        true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Do not use CompilationProvider here. It invalidates on every edit in the consumer project,
        // including DTO and endpoint-method changes that cannot affect the composition root. The two
        // syntax providers below each track precisely the declarations that participate in wiring.
        IncrementalValuesProvider<bool> rootInterfaces = context.SyntaxProvider.CreateSyntaxProvider(
                static (node, _) => node is InterfaceDeclarationSyntax
                {
                    Identifier.ValueText: "IGitLabClient"
                },
                static (syntaxContext, _) => IsRootClientInterface(syntaxContext))
            .Where(static isRootClient => isRootClient);

        IncrementalValuesProvider<RootPropertyModel> rootProperties = context.SyntaxProvider.CreateSyntaxProvider(
                static (node, _) => node is PropertyDeclarationSyntax or IndexerDeclarationSyntax &&
                                    node.Parent is InterfaceDeclarationSyntax
                                    {
                                        Identifier.ValueText: "IGitLabClient"
                                    },
                static (syntaxContext, _) => TryCreateRootProperty(syntaxContext))
            .Where(static property => property is not null)
            .Select(static (property, _) => property!.Value)
            // Roslyn recreates semantic symbols when any compilation tree is added. The models are
            // immutable value snapshots, so compare their observable content rather than the symbol
            // graph (or an ImmutableArray backing store) and keep unrelated edits downstream-cached.
            .WithComparer(RootPropertyModelComparer.Instance);

        IncrementalValuesProvider<EndpointClientModel> endpointClients = context.SyntaxProvider.CreateSyntaxProvider(
                static (node, _) => node is ClassDeclarationSyntax { Identifier.ValueText: var identifier } &&
                                    identifier.EndsWith("Client", StringComparison.Ordinal),
                static (syntaxContext, _) => TryCreateEndpointClient(syntaxContext))
            .Where(static endpoint => endpoint is not null)
            .Select(static (endpoint, _) => endpoint!.Value)
            .WithComparer(EndpointClientModelComparer.Instance);

        IncrementalValueProvider<WiringInput> wiringInput = rootInterfaces.Collect()
            .Combine(rootProperties.Collect())
            .Combine(endpointClients.Collect())
            .Select(static (inputs, _) => new WiringInput(
                inputs.Left.Left,
                inputs.Left.Right,
                inputs.Right))
            .WithComparer(WiringInputComparer.Instance)
            .WithTrackingName("GitLabClientWiring.Input");

        context.RegisterSourceOutput(wiringInput, static (productionContext, input) =>
        {
            if (input.RootInterfaces.IsDefaultOrEmpty)
            {
                return;
            }

            List<ResourceClient> resources = CollectResources(input.RootProperties, input.EndpointClients,
                productionContext);
            productionContext.AddSource("GitLabClient.Registrations.g.cs",
                SourceText.From(RenderRegistrations(resources), Encoding.UTF8));
            productionContext.AddSource("GitLabClient.Root.g.cs",
                SourceText.From(RenderRootClient(resources), Encoding.UTF8));
        });
    }

    private static bool IsRootClientInterface(GeneratorSyntaxContext syntaxContext)
    {
        return syntaxContext.Node is InterfaceDeclarationSyntax declaration &&
               syntaxContext.SemanticModel.GetDeclaredSymbol(declaration) is INamedTypeSymbol type &&
               IsRootClientInterface(type);
    }

    private static RootPropertyModel? TryCreateRootProperty(GeneratorSyntaxContext syntaxContext)
    {
        IPropertySymbol? property = syntaxContext.Node switch
        {
            PropertyDeclarationSyntax declaration => syntaxContext.SemanticModel.GetDeclaredSymbol(declaration),
            IndexerDeclarationSyntax declaration => syntaxContext.SemanticModel.GetDeclaredSymbol(declaration),
            _ => null
        };

        if (property is null || !IsRootClientInterface(property.ContainingType))
        {
            return null;
        }

        RootPropertyIssue issue = GetRootPropertyIssue(property, out INamedTypeSymbol? resourceInterface);
        return new RootPropertyModel(
            property.IsIndexer ? "this[]" : property.Name,
            resourceInterface?.ToDisplayString(TypeFormat),
            resourceInterface is null ? null : resourceInterface.Name.Substring(1),
            issue,
            property.Locations.FirstOrDefault() ?? Location.None);
    }

    private static EndpointClientModel? TryCreateEndpointClient(GeneratorSyntaxContext syntaxContext)
    {
        if (syntaxContext.Node is not ClassDeclarationSyntax declaration ||
            syntaxContext.SemanticModel.GetDeclaredSymbol(declaration) is not INamedTypeSymbol endpoint ||
            !string.Equals(endpoint.ContainingNamespace.ToDisplayString(), EndpointNamespace,
                StringComparison.Ordinal))
        {
            return null;
        }

        ImmutableArray<string> implementedInterfaces = endpoint.AllInterfaces
            .Select(static candidate => candidate.ToDisplayString(TypeFormat))
            .OrderBy(static candidate => candidate, StringComparer.Ordinal)
            .ToImmutableArray();

        return new EndpointClientModel(
            endpoint.Name,
            endpoint.ToDisplayString(TypeFormat),
            endpoint.IsAbstract,
            endpoint.IsGenericType,
            implementedInterfaces);
    }

    private static List<ResourceClient> CollectResources(ImmutableArray<RootPropertyModel> properties,
        ImmutableArray<EndpointClientModel> endpointClients, SourceProductionContext context)
    {
        Dictionary<string, EndpointClientModel> endpoints = new(StringComparer.Ordinal);

        foreach (EndpointClientModel endpoint in endpointClients.OrderBy(static candidate => candidate.Name,
                     StringComparer.Ordinal))
        {
            // A partial endpoint can be observed once for each declaration. Its complete symbol has the
            // same shape at every declaration, so retaining the first is deterministic and avoids a
            // duplicate-candidate allocation in the common single-file case.
            if (!endpoints.TryGetValue(endpoint.Name, out _))
            {
                endpoints.Add(endpoint.Name, endpoint);
            }
        }

        List<ResourceClient> resources = new(properties.Length);

        foreach (RootPropertyModel property in properties.OrderBy(static candidate => candidate.Name,
                     StringComparer.Ordinal))
        {
            if (property.Issue is not RootPropertyIssue.None)
            {
                context.ReportDiagnostic(Diagnostic.Create(InvalidResourceClientProperty,
                    property.Location,
                    property.Name,
                    DescribeRootPropertyIssue(property.Issue)));
                continue;
            }

            string interfaceType = property.InterfaceType!;
            string endpointName = property.EndpointName!;
            string endpointMetadataName = EndpointNamespace + "." + endpointName;

            if (!endpoints.TryGetValue(endpointName, out EndpointClientModel endpoint) || endpoint.IsAbstract ||
                endpoint.IsGeneric || !endpoint.Implements(interfaceType))
            {
                context.ReportDiagnostic(Diagnostic.Create(MissingEndpointImplementation,
                    property.Location, property.Name, endpointMetadataName, interfaceType));
                continue;
            }

            string memberName = ToCamelCase(property.Name);
            resources.Add(new ResourceClient(
                property.Name,
                memberName,
                EscapeIdentifier(memberName),
                interfaceType,
                endpoint.Type));
        }

        return resources;
    }

    private static RootPropertyIssue GetRootPropertyIssue(IPropertySymbol property,
        out INamedTypeSymbol? resourceInterface)
    {
        resourceInterface = null;

        if (property.IsStatic)
        {
            return RootPropertyIssue.Static;
        }

        if (property.IsIndexer)
        {
            return RootPropertyIssue.Indexer;
        }

        if (property.GetMethod is null)
        {
            return RootPropertyIssue.MissingGetter;
        }

        if (property.SetMethod is not null)
        {
            return RootPropertyIssue.HasSetter;
        }

        if (property.NullableAnnotation is NullableAnnotation.Annotated || property.Type is not INamedTypeSymbol
            {
                TypeKind: TypeKind.Interface,
                IsGenericType: false,
                DeclaredAccessibility: Accessibility.Public
            } candidate || !IsResourceClientInterface(candidate.Name))
        {
            return RootPropertyIssue.InvalidType;
        }

        resourceInterface = candidate;
        return RootPropertyIssue.None;
    }

    private static bool IsRootClientInterface(INamedTypeSymbol type)
    {
        return !type.IsGenericType && string.Equals(type.MetadataName, "IGitLabClient", StringComparison.Ordinal) &&
               string.Equals(type.ContainingNamespace.ToDisplayString(), "GitLab.Client.Abstractions",
                   StringComparison.Ordinal);
    }

    private static string DescribeRootPropertyIssue(RootPropertyIssue issue)
    {
        return issue switch
        {
            RootPropertyIssue.Static => "static properties are not supported",
            RootPropertyIssue.Indexer => "indexers are not supported",
            RootPropertyIssue.MissingGetter => "a getter is required",
            RootPropertyIssue.HasSetter => "a setter is not allowed",
            RootPropertyIssue.InvalidType =>
                "the declared type is not a non-nullable public non-generic I<Resource>Client interface",
            _ => throw new ArgumentOutOfRangeException(nameof(issue), issue, null)
        };
    }

    private static bool IsResourceClientInterface(string name)
    {
        const string suffix = "Client";
        return name.Length > 1 + suffix.Length && name[0] == 'I' && char.IsUpper(name[1]) &&
               name.EndsWith(suffix, StringComparison.Ordinal);
    }

    private static string RenderRegistrations(List<ResourceClient> resources)
    {
        StringBuilder builder = new();
        AppendHeader(builder);
        builder.AppendLine("using Microsoft.Extensions.DependencyInjection.Extensions;");
        builder.AppendLine();
        builder.AppendLine("namespace Microsoft.Extensions.DependencyInjection;");
        builder.AppendLine();
        builder.AppendLine("public static partial class GitLabClientServiceCollectionExtensions");
        builder.AppendLine("{");
        AppendGeneratedCodeAttributes(builder, "    ");
        builder.AppendLine(
            "    private static partial void AddResourceClients(global::Microsoft.Extensions.DependencyInjection.IServiceCollection services)");
        builder.AppendLine("    {");

        foreach (ResourceClient resource in resources)
        {
            builder.Append("        services.TryAddSingleton<").Append(resource.InterfaceType).Append(", ")
                .Append(resource.EndpointType).AppendLine(">();");
        }

        builder.AppendLine("    }");
        builder.AppendLine("}");
        return builder.ToString();
    }

    private static string RenderRootClient(List<ResourceClient> resources)
    {
        StringBuilder builder = new();
        AppendHeader(builder);
        builder.AppendLine("namespace GitLab.Client;");
        builder.AppendLine();
        AppendGeneratedCodeAttributes(builder, string.Empty);
        builder.AppendLine("[global::System.Diagnostics.DebuggerNonUserCode]");
        builder.AppendLine("internal sealed class GitLabClient : global::GitLab.Client.Abstractions.IGitLabClient");
        builder.AppendLine("{");

        foreach (ResourceClient resource in resources)
        {
            builder.Append("    private readonly ").Append(resource.InterfaceType).Append(" _")
                .Append(resource.MemberName).AppendLine(";");
        }

        builder.AppendLine();
        builder.AppendLine("    public GitLabClient(");

        for (int index = 0; index < resources.Count; index++)
        {
            ResourceClient resource = resources[index];
            builder.Append("        ").Append(resource.InterfaceType).Append(' ').Append(resource.ParameterName);
            builder.AppendLine(index == resources.Count - 1 ? ")" : ",");
        }

        if (resources.Count == 0)
        {
            builder.AppendLine("    )");
        }

        builder.AppendLine("    {");

        foreach (ResourceClient resource in resources)
        {
            builder.Append("        _").Append(resource.MemberName).Append(" = ")
                .Append(resource.ParameterName).AppendLine(";");
        }

        builder.AppendLine("    }");

        foreach (ResourceClient resource in resources)
        {
            builder.AppendLine();
            builder.Append("    ").Append(resource.InterfaceType)
                .Append(" global::GitLab.Client.Abstractions.IGitLabClient.")
                .Append(EscapeIdentifier(resource.PropertyName)).Append(" => _")
                .Append(resource.MemberName)
                .AppendLine(";");
        }

        builder.AppendLine("}");
        return builder.ToString();
    }

    private static void AppendHeader(StringBuilder builder)
    {
        builder.AppendLine("// <auto-generated/>");
        builder.AppendLine("#nullable enable");
        builder.AppendLine();
    }

    private static void AppendGeneratedCodeAttributes(StringBuilder builder, string indent)
    {
        builder.Append(indent).Append("[global::System.CodeDom.Compiler.GeneratedCode(\"").Append(GeneratorName)
            .Append("\", \"").Append(GeneratorVersion).AppendLine("\")]");
        builder.Append(indent).AppendLine("[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]");
    }

    private static string ToCamelCase(string name)
    {
        return char.ToLowerInvariant(name[0]) + name.Substring(1);
    }

    private static string EscapeIdentifier(string name)
    {
        return SyntaxFacts.GetKeywordKind(name) == SyntaxKind.None &&
               SyntaxFacts.GetContextualKeywordKind(name) == SyntaxKind.None
            ? name
            : "@" + name;
    }

    private static bool HaveEquivalentLocations(Location left, Location right)
    {
        return left.Kind == right.Kind && left.SourceSpan.Equals(right.SourceSpan) &&
               string.Equals(left.SourceTree?.FilePath, right.SourceTree?.FilePath, StringComparison.Ordinal);
    }

    private static int AppendHash(int hash, string? value)
    {
        return unchecked((hash * 31) + (value is null ? 0 : StringComparer.Ordinal.GetHashCode(value)));
    }

    private sealed class RootPropertyModelComparer : IEqualityComparer<RootPropertyModel>
    {
        public static RootPropertyModelComparer Instance { get; } = new();

        public bool Equals(RootPropertyModel x, RootPropertyModel y)
        {
            return string.Equals(x.Name, y.Name, StringComparison.Ordinal) &&
                   string.Equals(x.InterfaceType, y.InterfaceType, StringComparison.Ordinal) &&
                   string.Equals(x.EndpointName, y.EndpointName, StringComparison.Ordinal) && x.Issue == y.Issue &&
                   HaveEquivalentLocations(x.Location, y.Location);
        }

        public int GetHashCode(RootPropertyModel model)
        {
            int hash = AppendHash(17, model.Name);
            hash = AppendHash(hash, model.InterfaceType);
            hash = AppendHash(hash, model.EndpointName);
            hash = unchecked((hash * 31) + (int)model.Issue);
            hash = unchecked((hash * 31) + (int)model.Location.Kind);
            hash = unchecked((hash * 31) + model.Location.SourceSpan.Start);
            hash = unchecked((hash * 31) + model.Location.SourceSpan.Length);
            return AppendHash(hash, model.Location.SourceTree?.FilePath);
        }
    }

    private sealed class EndpointClientModelComparer : IEqualityComparer<EndpointClientModel>
    {
        public static EndpointClientModelComparer Instance { get; } = new();

        public bool Equals(EndpointClientModel x, EndpointClientModel y)
        {
            return string.Equals(x.Name, y.Name, StringComparison.Ordinal) &&
                   string.Equals(x.Type, y.Type, StringComparison.Ordinal) && x.IsAbstract == y.IsAbstract &&
                   x.IsGeneric == y.IsGeneric && x.ImplementedInterfaces.SequenceEqual(y.ImplementedInterfaces,
                       StringComparer.Ordinal);
        }

        public int GetHashCode(EndpointClientModel model)
        {
            int hash = AppendHash(17, model.Name);
            hash = AppendHash(hash, model.Type);
            hash = unchecked((hash * 31) + (model.IsAbstract ? 1 : 0));
            hash = unchecked((hash * 31) + (model.IsGeneric ? 1 : 0));

            foreach (string implementedInterface in model.ImplementedInterfaces)
            {
                hash = AppendHash(hash, implementedInterface);
            }

            return hash;
        }
    }

    private sealed class WiringInputComparer : IEqualityComparer<WiringInput>
    {
        public static WiringInputComparer Instance { get; } = new();

        public bool Equals(WiringInput x, WiringInput y)
        {
            if (!x.RootInterfaces.SequenceEqual(y.RootInterfaces) ||
                x.RootProperties.Length != y.RootProperties.Length ||
                x.EndpointClients.Length != y.EndpointClients.Length)
            {
                return false;
            }

            for (int index = 0; index < x.RootProperties.Length; index++)
            {
                if (!RootPropertyModelComparer.Instance.Equals(x.RootProperties[index], y.RootProperties[index]))
                {
                    return false;
                }
            }

            for (int index = 0; index < x.EndpointClients.Length; index++)
            {
                if (!EndpointClientModelComparer.Instance.Equals(x.EndpointClients[index], y.EndpointClients[index]))
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(WiringInput input)
        {
            int hash = 17;

            foreach (bool rootInterface in input.RootInterfaces)
            {
                hash = unchecked((hash * 31) + (rootInterface ? 1 : 0));
            }

            foreach (RootPropertyModel property in input.RootProperties)
            {
                hash = unchecked((hash * 31) + RootPropertyModelComparer.Instance.GetHashCode(property));
            }

            foreach (EndpointClientModel endpoint in input.EndpointClients)
            {
                hash = unchecked((hash * 31) + EndpointClientModelComparer.Instance.GetHashCode(endpoint));
            }

            return hash;
        }
    }

    private readonly record struct WiringInput(
        ImmutableArray<bool> RootInterfaces,
        ImmutableArray<RootPropertyModel> RootProperties,
        ImmutableArray<EndpointClientModel> EndpointClients);

    private readonly record struct RootPropertyModel(
        string Name,
        string? InterfaceType,
        string? EndpointName,
        RootPropertyIssue Issue,
        Location Location);

    private readonly record struct EndpointClientModel(
        string Name,
        string Type,
        bool IsAbstract,
        bool IsGeneric,
        ImmutableArray<string> ImplementedInterfaces)
    {
        public bool Implements(string interfaceType)
        {
            foreach (string implementedInterface in ImplementedInterfaces)
            {
                if (string.Equals(implementedInterface, interfaceType, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    ///     Rendering uses each member name in several places. Resolve its camel-cased and escaped forms
    ///     once while building the resource model rather than repeatedly allocating them during source
    ///     rendering for every endpoint client.
    /// </summary>
    private readonly record struct ResourceClient(
        string PropertyName,
        string MemberName,
        string ParameterName,
        string InterfaceType,
        string EndpointType);

    private enum RootPropertyIssue
    {
        None,
        Static,
        Indexer,
        MissingGetter,
        HasSetter,
        InvalidType
    }
}