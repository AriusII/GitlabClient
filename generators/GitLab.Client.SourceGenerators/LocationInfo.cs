using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace GitLab.Client.SourceGenerators;

/// <summary>
///     A value-equatable stand-in for <see cref="Location" />. A real <see cref="Location" /> holds the
///     <see cref="SyntaxTree" /> it came from, so carrying one through an incremental pipeline both
///     defeats caching (reference equality) and roots the whole tree in the driver's cache. This keeps
///     the path and spans only and rebuilds a real <see cref="Location" /> when a diagnostic is reported.
/// </summary>
internal readonly record struct LocationInfo(string FilePath, TextSpan TextSpan, LinePositionSpan LineSpan)
{
    public static LocationInfo? CreateFrom(Location? location)
    {
        if (location?.SourceTree is null)
        {
            return null;
        }

        return new LocationInfo(location.SourceTree.FilePath, location.SourceSpan, location.GetLineSpan().Span);
    }

    public static LocationInfo? CreateFrom(SyntaxNode? node)
    {
        return node is null ? null : CreateFrom(node.GetLocation());
    }

    public static LocationInfo? CreateFrom(ISymbol symbol)
    {
        foreach (SyntaxReference reference in symbol.DeclaringSyntaxReferences)
        {
            LocationInfo? candidate = CreateFrom(reference.GetSyntax());
            if (candidate is not null)
            {
                return candidate;
            }
        }

        return null;
    }

    public Location ToLocation()
    {
        return Location.Create(FilePath, TextSpan, LineSpan);
    }
}