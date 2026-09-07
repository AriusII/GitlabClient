using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace GitLab.Client.Domain;

/// <summary>
///     Identifies a GitLab group either by its numeric ID or by its URL-encoded "full path", exactly
///     as <see cref="ProjectId" /> does for projects — the two ID shapes are used identically across
///     the GitLab REST API.
/// </summary>
public readonly record struct GroupId
{
    private readonly long _numericId;
    private readonly string? _path;

    private GroupId(long numericId, string? path)
    {
        _numericId = numericId;
        _path = path;
    }

    public bool IsNumeric => _path is null;

    public static GroupId FromId(long id)
    {
        return new GroupId(id, null);
    }

    public static GroupId FromPath(string fullPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullPath);
        return new GroupId(0, fullPath);
    }

    [SuppressMessage("Design", "CA2225",
        Justification = $"{nameof(FromId)} is the named equivalent, and reads better than FromInt64 at call sites.")]
    public static implicit operator GroupId(long id)
    {
        return FromId(id);
    }

    [SuppressMessage("Design", "CA2225",
        Justification = $"{nameof(FromPath)} is the named equivalent, and reads better than FromString at call sites.")]
    public static implicit operator GroupId(string fullPath)
    {
        return FromPath(fullPath);
    }

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