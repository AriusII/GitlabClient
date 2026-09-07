using System.Collections.Immutable;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace GitLab.Client.SourceGenerators;

/// <summary>
///     Emits the two pieces of per-resource wiring that used to be hand-edited every time a resource was
///     added: the dependency-injection registration block, and the root aggregate implementation behind
///     <c>IGitLabClient</c>. Both are derived from the same <c>[GenerateClientLayers]</c> attribute
///     <see cref="GenerateClientLayersGenerator" /> reads, so adding a resource costs one line on
///     <c>IGitLabClient</c> instead of edits to three shared files.
///     <para>
///         The emitted registrations name <c>&lt;Resource&gt;Service</c> and
///         <c>&lt;Resource&gt;Controller</c>, which the other generator produces. That works because every
///         generator's output is added to the SAME compilation and the compiler binds the merged result:
///         generated code may freely REFERENCE generated code. What is impossible is ANALYSING it - each
///         generator reads the compilation as it stood before generation - so nothing here looks those
///         types up. It composes their names from <see cref="ClientLayerNaming" /> and prints them. The
///         marker attribute is the one thing that does cross generators, and it travels over the
///         post-initialization channel, which is documented as visible to later phases.
///     </para>
///     <para>
///         This generator must never call <c>AddEmbeddedAttributeDefinition</c> or emit
///         <c>GenerateClientLayersAttribute</c>: both generators live in one assembly and their post-init
///         sources land in one compilation, so a second copy would define the type twice (CS0101).
///     </para>
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class GitLabClientWiringGenerator : IIncrementalGenerator
{
    private const string GeneratorName = "GitLab.Client.SourceGenerators";

    private const string GeneratorVersion = "1.0.0";

    private static readonly SymbolDisplayFormat TypeFormat = SymbolDisplayFormat.FullyQualifiedFormat;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<ResourceWiringModel> resources = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                ClientLayerNaming.AttributeFullName,
                static (node, _) => node is InterfaceDeclarationSyntax,
                static (ctx, _) => TryCreateModel(ctx))
            .Where(static model => model.HasValue)
            .Select(static (model, _) => model!.Value)
            .WithTrackingName(TrackingNames.WiringModels);

        // Collect() hands back an ImmutableArray, whose equality is by REFERENCE - so the projection
        // into a value-equatable EquatableArray is not cosmetic: without it the output node below would
        // fire on every single run and re-render both files on every keystroke.
        IncrementalValueProvider<EquatableArray<ResourceWiringModel>> sorted = resources
            .Collect()
            .Select(static (models, _) => Sort(models))
            .WithTrackingName(TrackingNames.SortedWiringModels);

        context.RegisterSourceOutput(sorted, static (spc, models) => Emit(spc, models));
    }

    /// <summary>
    ///     Ordinal, never culture-aware, with a total-order tiebreak, so the generated files are
    ///     byte-identical on every machine under <c>Deterministic=true</c> and their <c>obj/Generated</c>
    ///     diff stays reviewable.
    /// </summary>
    private static EquatableArray<ResourceWiringModel> Sort(ImmutableArray<ResourceWiringModel> models)
    {
        if (models.IsDefaultOrEmpty)
        {
            return EquatableArray<ResourceWiringModel>.Empty;
        }

        ResourceWiringModel[] ordered = models.ToArray();

        Array.Sort(ordered, static (left, right) =>
        {
            int byResource = string.CompareOrdinal(left.ResourceName, right.ResourceName);
            return byResource != 0
                ? byResource
                : string.CompareOrdinal(left.RepositoryInterface, right.RepositoryInterface);
        });

        return new EquatableArray<ResourceWiringModel>(ordered);
    }

    private static ResourceWiringModel? TryCreateModel(GeneratorAttributeSyntaxContext ctx)
    {
        if (ctx.TargetSymbol is not INamedTypeSymbol repositoryInterface ||
            repositoryInterface.ContainingNamespace.IsGlobalNamespace)
        {
            return null;
        }

        AttributeData? attribute = ctx.Attributes.FirstOrDefault();

        if (attribute is null || attribute.ConstructorArguments.Length != 2)
        {
            return null;
        }

        INamedTypeSymbol? serviceInterface = AsInterface(attribute.ConstructorArguments[0]);
        INamedTypeSymbol? clientInterface = AsInterface(attribute.ConstructorArguments[1]);

        if (serviceInterface is null || clientInterface is null)
        {
            // GenerateClientLayersGenerator already reports GLC0001 (or the compiler reported CS0246);
            // a second diagnostic for the same mistake would be noise.
            return null;
        }

        string resourceName = ClientLayerNaming.GetResourceName(repositoryInterface.Name);

        if (resourceName.Length == 0)
        {
            return null;
        }

        string repositoryNamespace = repositoryInterface.ContainingNamespace.ToDisplayString();
        string serviceNamespace = NamedArgument(attribute, "ServiceNamespace") as string ??
                                  ClientLayerNaming.DeriveLayerNamespace(repositoryNamespace,
                                      ClientLayerNaming.ServicesLayerSegment);
        string controllerNamespace = NamedArgument(attribute, "ControllerNamespace") as string ??
                                     ClientLayerNaming.DeriveLayerNamespace(repositoryNamespace,
                                         ClientLayerNaming.ControllersLayerSegment);

        return new ResourceWiringModel(
            resourceName,
            NamedArgument(attribute, "RootPropertyName") as string ?? resourceName,
            repositoryInterface.ToDisplayString(TypeFormat),
            FindRepositoryImplementation(ctx.SemanticModel.Compilation, repositoryInterface, resourceName),
            serviceInterface.ToDisplayString(TypeFormat),
            ClientLayerNaming.ComposeGlobalTypeName(serviceNamespace,
                resourceName + ClientLayerNaming.ServiceSuffix),
            clientInterface.ToDisplayString(TypeFormat),
            ClientLayerNaming.ComposeGlobalTypeName(controllerNamespace,
                resourceName + ClientLayerNaming.ControllerSuffix),
            NamedArgument(attribute, "Register") is not false,
            NamedArgument(attribute, "ExposeOnRootClient") is not false,
            clientInterface.DeclaredAccessibility == Accessibility.Public,
            LocationInfo.CreateFrom(GetTargetLocation(ctx.TargetNode)));
    }

    /// <summary>
    ///     Resolves the hand-written <c>&lt;Resource&gt;Repository</c> class. This lookup is legal
    ///     precisely because the repository implementation is hand-written and therefore present in the
    ///     pre-generation compilation - unlike the Service and Controller classes, which are not and are
    ///     only ever named.
    /// </summary>
    private static string FindRepositoryImplementation(Compilation compilation,
        INamedTypeSymbol repositoryInterface, string resourceName)
    {
        string metadataName = repositoryInterface.ContainingNamespace.ToDisplayString() + "." +
                              ClientLayerNaming.GetRepositoryImplementationName(resourceName);

        INamedTypeSymbol? implementation = compilation.Assembly.GetTypeByMetadataName(metadataName);

        if (implementation is null ||
            implementation.TypeKind != TypeKind.Class ||
            implementation.IsAbstract ||
            implementation.IsGenericType)
        {
            return string.Empty;
        }

        foreach (INamedTypeSymbol candidate in implementation.AllInterfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(candidate, repositoryInterface))
            {
                return implementation.ToDisplayString(TypeFormat);
            }
        }

        return string.Empty;
    }

    private static INamedTypeSymbol? AsInterface(TypedConstant argument)
    {
        return argument.Value is INamedTypeSymbol { TypeKind: TypeKind.Interface } candidate ? candidate : null;
    }

    private static object? NamedArgument(AttributeData attribute, string name)
    {
        foreach (KeyValuePair<string, TypedConstant> argument in attribute.NamedArguments)
        {
            if (string.Equals(argument.Key, name, StringComparison.Ordinal))
            {
                return argument.Value.Value;
            }
        }

        return null;
    }

    private static Location GetTargetLocation(SyntaxNode node)
    {
        return node is InterfaceDeclarationSyntax declaration
            ? declaration.Identifier.GetLocation()
            : node.GetLocation();
    }

    private static void Emit(SourceProductionContext context, EquatableArray<ResourceWiringModel> models)
    {
        Dictionary<string, ResourceWiringModel> claimed = new(StringComparer.Ordinal);
        List<ResourceWiringModel> registrable = new();
        List<ResourceWiringModel> exposed = new();

        foreach (ResourceWiringModel model in models)
        {
            // Every failure below reports one targeted error AND drops only the affected emission, so the
            // one actionable message is not buried under a cascade of compiler errors inside generated
            // files nobody owns.
            if (model.RepositoryImplementation.Length == 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.MissingRepositoryImplementation,
                    model.Location?.ToLocation(),
                    model.RepositoryInterface,
                    ClientLayerNaming.GetRepositoryImplementationName(model.ResourceName)));
            }
            else if (model.Register)
            {
                registrable.Add(model);
            }

            if (!model.ExposeOnRootClient)
            {
                continue;
            }

            if (!model.ClientInterfaceIsPublic)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.ClientInterfaceNotPublic,
                    model.Location?.ToLocation(), model.ClientInterface));
                continue;
            }

            if (!SyntaxFacts.IsValidIdentifier(model.RootPropertyName))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.InvalidRootPropertyName,
                    model.Location?.ToLocation(), model.RootPropertyName, model.ResourceName));
                continue;
            }

            if (claimed.TryGetValue(model.RootPropertyName, out ResourceWiringModel owner))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.DuplicateRootProperty,
                    model.Location?.ToLocation(),
                    model.ResourceName, model.RootPropertyName, owner.ResourceName));
                continue;
            }

            claimed.Add(model.RootPropertyName, model);
            exposed.Add(model);
        }

        context.AddSource("ServiceCollectionExtensions.Resources.g.cs",
            SourceText.From(RenderRegistrations(registrable), Encoding.UTF8));
        context.AddSource("GitLabClient.Root.g.cs",
            SourceText.From(RenderRootClient(exposed), Encoding.UTF8));
    }

    private static void AppendFileHeader(StringBuilder builder)
    {
        builder.AppendLine("// <auto-generated/>");
        builder.AppendLine("#nullable enable");
        builder.AppendLine();
        // The auto-generated header suppresses analyzers, not core compiler warnings. A resource client
        // interface may legitimately be [Obsolete] under the deprecation policy in CLAUDE.md, and that
        // must not become an unfixable build break inside a file nobody can edit.
        builder.AppendLine("#pragma warning disable CS0612, CS0618");
        builder.AppendLine();
    }

    private static void AppendGeneratedCodeAttributes(StringBuilder builder, string indent)
    {
        builder.Append(indent).Append("[global::System.CodeDom.Compiler.GeneratedCode(\"").Append(GeneratorName)
            .Append("\", \"").Append(GeneratorVersion).AppendLine("\")]");
        builder.Append(indent).AppendLine("[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]");
    }

    private static string RenderRegistrations(List<ResourceWiringModel> resources)
    {
        StringBuilder builder = new();
        AppendFileHeader(builder);
        builder.AppendLine("using Microsoft.Extensions.DependencyInjection.Extensions;");
        builder.AppendLine();
        builder.Append("namespace ").AppendLine(ClientLayerNaming.DependencyInjectionNamespace);
        builder.AppendLine("{");
        builder.Append("    public static partial class ").AppendLine(ClientLayerNaming.RegistrationClassName);
        builder.AppendLine("    {");
        builder.AppendLine(
            "        // One block per resource whose Repository interface carries [GenerateClientLayers].");
        builder.AppendLine(
            "        // Both type arguments of every TryAddSingleton below are closed at compile time: no");
        builder.AppendLine(
            "        // assembly scanning, no Activator.CreateInstance, no Type-keyed lookup, no convention-");
        builder.AppendLine(
            "        // based reflection at runtime. Only the typing is generated - the registration is still");
        builder.AppendLine(
            "        // explicit, which is what the \"DI: explicit registration only\" rule in CLAUDE.md is about.");
        AppendGeneratedCodeAttributes(builder, "        ");
        builder.Append("        private static partial void ").Append(ClientLayerNaming.RegistrationMethodName)
            .AppendLine("(global::Microsoft.Extensions.DependencyInjection.IServiceCollection services)");
        builder.AppendLine("        {");

        for (int index = 0; index < resources.Count; index++)
        {
            ResourceWiringModel resource = resources[index];

            if (index > 0)
            {
                builder.AppendLine();
            }

            builder.Append("            // ").AppendLine(resource.ResourceName);
            AppendRegistration(builder, resource.RepositoryInterface, resource.RepositoryImplementation);
            AppendRegistration(builder, resource.ServiceInterface, resource.ServiceImplementation);
            AppendRegistration(builder, resource.ClientInterface, resource.ControllerImplementation);
        }

        builder.AppendLine("        }");
        builder.AppendLine("    }");
        builder.AppendLine("}");
        builder.AppendLine();
        builder.AppendLine("#pragma warning restore CS0612, CS0618");
        return builder.ToString();
    }

    private static void AppendRegistration(StringBuilder builder, string serviceType, string implementationType)
    {
        builder.Append("            services.TryAddSingleton<").Append(serviceType).Append(", ")
            .Append(implementationType).AppendLine(">();");
    }

    private static string RenderRootClient(List<ResourceWiringModel> resources)
    {
        StringBuilder builder = new();
        AppendFileHeader(builder);
        builder.Append("namespace ").AppendLine(ClientLayerNaming.RootClientNamespace);
        builder.AppendLine("{");
        builder.AppendLine(
            "    // Root aggregate implementation behind IGitLabClient. Every member is an EXPLICIT interface");
        builder.AppendLine(
            "    // implementation on purpose: that makes the C# compiler enforce the pairing in both");
        builder.AppendLine(
            "    // directions - a property on IGitLabClient with no attributed resource is CS0535, and an");
        builder.AppendLine(
            "    // attributed resource with no property on IGitLabClient is CS0539.");
        AppendGeneratedCodeAttributes(builder, "    ");
        builder.AppendLine("    [global::System.Diagnostics.DebuggerNonUserCode]");
        builder.Append("    internal sealed class ").Append(ClientLayerNaming.RootClientClassName)
            .Append(" : ").AppendLine(ClientLayerNaming.RootClientInterface);
        builder.AppendLine("    {");

        foreach (ResourceWiringModel resource in resources)
        {
            builder.Append("        private readonly ").Append(resource.ClientInterface).Append(" _")
                .Append(ClientLayerNaming.ToCamelCase(resource.RootPropertyName)).AppendLine(";");
        }

        if (resources.Count > 0)
        {
            builder.AppendLine();
        }

        builder.Append("        public ").Append(ClientLayerNaming.RootClientClassName).Append('(');

        for (int index = 0; index < resources.Count; index++)
        {
            if (index > 0)
            {
                builder.Append(',');
            }

            builder.AppendLine();
            builder.Append("            ").Append(resources[index].ClientInterface).Append(' ')
                .Append(ClientLayerNaming.EscapeIdentifier(
                    ClientLayerNaming.ToCamelCase(resources[index].RootPropertyName)));
        }

        builder.AppendLine(")");
        builder.AppendLine("        {");

        foreach (ResourceWiringModel resource in resources)
        {
            string parameter = ClientLayerNaming.ToCamelCase(resource.RootPropertyName);
            builder.Append("            _").Append(parameter).Append(" = ")
                .Append(ClientLayerNaming.EscapeIdentifier(parameter)).AppendLine(";");
        }

        builder.AppendLine("        }");

        foreach (ResourceWiringModel resource in resources)
        {
            builder.AppendLine();
            builder.Append("        ").Append(resource.ClientInterface).Append(' ')
                .Append(ClientLayerNaming.RootClientInterface).Append('.').Append(resource.RootPropertyName)
                .Append(" => _").Append(ClientLayerNaming.ToCamelCase(resource.RootPropertyName)).AppendLine(";");
        }

        builder.AppendLine("    }");
        builder.AppendLine("}");
        builder.AppendLine();
        builder.AppendLine("#pragma warning restore CS0612, CS0618");
        return builder.ToString();
    }
}