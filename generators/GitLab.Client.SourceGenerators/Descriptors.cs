using Microsoft.CodeAnalysis;

namespace GitLab.Client.SourceGenerators;

/// <summary>
///     Every diagnostic either generator can report. Without these, misusing
///     <c>[GenerateClientLayers]</c> surfaces as a compiler error inside a generated file the developer
///     cannot edit (or, worse, as silently missing code), which is the wrong failure mode for a
///     generator that ~1800 further API operations will be added through.
///     <para>
///         GLC0001-GLC0008 belong to <see cref="GenerateClientLayersGenerator" /> (the Service/Controller
///         forwarders); GLC0101-GLC0104 belong to <see cref="GitLabClientWiringGenerator" /> (the DI
///         registrations and root aggregate). The two ranges are deliberately disjoint: both generators
///         ship in one assembly, so a shared id would make the same number mean two different things.
///     </para>
/// </summary>
internal static class Descriptors
{
    private const string LayersCategory = "GitLab.Client.Generation";

    private const string WiringCategory = "GitLab.Client.Wiring";

    public static readonly DiagnosticDescriptor ArgumentMustBeInterface = new(
        "GLC0001",
        "GenerateClientLayers argument must be an interface",
        "Type '{0}' passed to [GenerateClientLayers] on '{1}' is not an interface, so the generated layer would inherit from it instead of implementing it",
        LayersCategory,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor DuplicateGeneratedLayer = new(
        "GLC0002",
        "Duplicate generated client layer",
        "More than one [GenerateClientLayers] interface generates '{0}' ({1}); rename one of the repository interfaces or move it to a different namespace",
        LayersCategory,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor ForwardingTargetMissingMember = new(
        "GLC0003",
        "Forwarding target does not declare a matching member",
        "'{0}' declares '{1}', but '{2}' has no member with a matching signature, so the generated forwarder cannot call it",
        LayersCategory,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor UnsupportedMember = new(
        "GLC0004",
        "Interface member cannot be forwarded",
        "'{0}' declares '{1}', which the client-layer generator cannot forward ({2}); move it out of the generated layer or implement it by hand",
        LayersCategory,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor RepositoryNamingConvention = new(
        "GLC0005",
        "Repository interface naming convention",
        "'{0}' should be named 'I<Resource>Repository'; the generated layers will be named '{1}Service' and '{1}Controller'",
        LayersCategory,
        DiagnosticSeverity.Warning,
        true);

    public static readonly DiagnosticDescriptor UnsupportedRepositoryShape = new(
        "GLC0006",
        "Unsupported repository interface shape",
        "'{0}' must be a non-generic, top-level interface in a named namespace to carry [GenerateClientLayers]",
        LayersCategory,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor EmptyGeneratedLayer = new(
        "GLC0007",
        "Generated client layer has no members",
        "'{0}' declares no forwardable members, so '{1}' will be an empty forwarder",
        LayersCategory,
        DiagnosticSeverity.Warning,
        true);

    public static readonly DiagnosticDescriptor UnrepresentableDefaultValue = new(
        "GLC0008",
        "Optional parameter default cannot be reproduced",
        "The default value of parameter '{0}' on '{1}' cannot be reproduced in generated code, so the generated forwarder declares the parameter without a default",
        LayersCategory,
        DiagnosticSeverity.Warning,
        true);

    public static readonly DiagnosticDescriptor DuplicateRootProperty = new(
        "GLC0101",
        "Duplicate root client property",
        "Resources '{0}' and '{2}' both map to root client property '{1}'; set RootPropertyName on one of their [GenerateClientLayers] attributes",
        WiringCategory,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor ClientInterfaceNotPublic = new(
        "GLC0102",
        "Resource client interface is not public",
        "Client interface '{0}' is not public and cannot be exposed on the public IGitLabClient; make it public or set ExposeOnRootClient = false",
        WiringCategory,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor MissingRepositoryImplementation = new(
        "GLC0103",
        "Repository implementation not found",
        "No non-abstract class named '{1}' implementing '{0}' was found in the same namespace, so the DI registration for this resource cannot be generated",
        WiringCategory,
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor InvalidRootPropertyName = new(
        "GLC0104",
        "Root client property name is not an identifier",
        "RootPropertyName '{0}' on resource '{1}' is not a valid C# identifier",
        WiringCategory,
        DiagnosticSeverity.Error,
        true);
}