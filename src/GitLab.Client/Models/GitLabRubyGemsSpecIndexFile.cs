namespace GitLab.Client.Models;

/// <summary>
///     Which RubyGems spec index file to download
///     (<c>GET /projects/:id/packages/rubygems/:file_name</c>).
///     <para>
///         A closed vocabulary the spec enumerates on the path parameter, so it is modelled as an enum
///         rather than a free-text segment: an unknown file name is a <c>404</c> from GitLab, and a
///         compile error is a cheaper way to find that out. It never appears in a JSON payload - the
///         repository projects it onto the route.
///     </para>
/// </summary>
public enum GitLabRubyGemsSpecIndexFile
{
    /// <summary><c>specs.4.8.gz</c> - every gem version ever published to this registry.</summary>
    Specs,

    /// <summary><c>latest_specs.4.8.gz</c> - only the latest version of each gem.</summary>
    LatestSpecs,

    /// <summary><c>prerelease_specs.4.8.gz</c> - prerelease versions only.</summary>
    PrereleaseSpecs
}