using Microsoft.CodeAnalysis;

namespace GitLab.Client.SourceGenerators;

/// <summary>
///     A diagnostic reduced to value-equatable parts so it can travel through the incremental pipeline
///     alongside the model it belongs to. <see cref="DiagnosticDescriptor" /> is itself value-equatable
///     and holds no compilation state, and <see cref="LocationInfo" /> replaces the tree-rooted
///     <see cref="Location" />, so a model carrying these still caches on value equality.
/// </summary>
internal readonly record struct DiagnosticInfo(
    DiagnosticDescriptor Descriptor,
    LocationInfo? Location,
    EquatableArray<string> MessageArguments)
{
    public static DiagnosticInfo Create(DiagnosticDescriptor descriptor, LocationInfo? location,
        params string[] messageArguments)
    {
        return new DiagnosticInfo(descriptor, location, new EquatableArray<string>(messageArguments));
    }

    public Diagnostic ToDiagnostic()
    {
        object?[] arguments = new object?[MessageArguments.Length];

        for (int index = 0; index < arguments.Length; index++)
        {
            arguments[index] = MessageArguments[index];
        }

        return Diagnostic.Create(Descriptor, Location?.ToLocation(), arguments);
    }
}