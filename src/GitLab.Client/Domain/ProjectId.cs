using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace GitLab.Client.Domain;

/// <summary>
///     Identifies a GitLab project either by its numeric ID or by its URL-encoded
///     "namespace/project" path, as accepted by the <c>:id</c> route parameter across the GitLab REST API.
/// </summary>
public readonly record struct ProjectId
{
    private readonly long _numericId;
    private readonly string? _path;

    private ProjectId(long numericId, string? path)
    {
        _numericId = numericId;
        _path = path;
    }

    public bool IsNumeric => _path is null;

    public static ProjectId FromId(long id)
    {
        return new ProjectId(id, null);
    }

    public static ProjectId FromPath(string namespacedPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(namespacedPath);
        return new ProjectId(0, namespacedPath);
    }

    [SuppressMessage("Design", "CA2225",
        Justification = $"{nameof(FromId)} is the named equivalent, and reads better than FromInt64 at call sites.")]
    public static implicit operator ProjectId(long id)
    {
        return FromId(id);
    }

    [SuppressMessage("Design", "CA2225",
        Justification = $"{nameof(FromPath)} is the named equivalent, and reads better than FromString at call sites.")]
    public static implicit operator ProjectId(string namespacedPath)
    {
        return FromPath(namespacedPath);
    }

    /// <summary>The value to substitute into the route, already percent-encoded when it is a namespaced path.</summary>
    public string ToRouteValue()
    {
        return _path is null
            ? _numericId.ToString(CultureInfo.InvariantCulture)
            : Uri.EscapeDataString(_path);
    }

    public override string ToString()
    {
        return _path ?? _numericId.ToString(CultureInfo.InvariantCulture);
    }
}