namespace GitLab.Client.Models;

/// <summary>
///     A Debian distribution - the top-level unit of GitLab's Debian package registry
///     (<c>/projects/:id/debian_distributions</c>, <c>/groups/:id/-/debian_distributions</c>). One
///     distribution owns the components and architectures its APT repository serves, and the metadata
///     GitLab renders into the <c>Release</c>/<c>InRelease</c> files under
///     <c>/packages/debian/dists/:codename</c>.
/// </summary>
public sealed record GitLabDebianDistribution
{
    public required long Id { get; init; }

    /// <summary>
    ///     The Debian codename (<c>sid</c>, <c>bullseye</c>) - the value every other Debian route
    ///     addresses this distribution by, since GitLab does not expose a numeric-id route for it.
    /// </summary>
    public required string Codename { get; init; }

    /// <summary>The Debian suite (<c>unstable</c>, <c>stable</c>).</summary>
    public string? Suite { get; init; }

    /// <summary>The Debian origin, rendered into the <c>Release</c> file's <c>Origin</c> field.</summary>
    public string? Origin { get; init; }

    /// <summary>The Debian label, rendered into the <c>Release</c> file's <c>Label</c> field.</summary>
    public string? Label { get; init; }

    /// <summary>The Debian version, rendered into the <c>Release</c> file's <c>Version</c> field.</summary>
    public string? Version { get; init; }

    public string? Description { get; init; }

    /// <summary>How long a client should treat the generated <c>Release</c> file as fresh, in seconds.</summary>
    public int? ValidTimeDurationSeconds { get; init; }

    /// <summary>The Debian components this distribution serves - <c>main</c>, <c>contrib</c>, and so on.</summary>
    public IReadOnlyList<string>? Components { get; init; }

    /// <summary>The Debian architectures this distribution serves - <c>amd64</c>, <c>arm64</c>, and so on.</summary>
    public IReadOnlyList<string>? Architectures { get; init; }
}