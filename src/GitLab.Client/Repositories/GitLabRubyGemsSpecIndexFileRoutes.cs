using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

/// <summary>
///     Projects <see cref="GitLabRubyGemsSpecIndexFile" /> onto the <c>{file_name}</c> path segment of
///     the RubyGems spec-index download endpoint. The mapping is an explicit switch rather than a naming
///     convention so that adding an enum member without giving it a wire name is a compile error
///     (CS8509), not a <c>404</c> at run time.
/// </summary>
internal static class GitLabRubyGemsSpecIndexFileRoutes
{
    /// <summary>
    ///     The GitLab wire name for a spec index file. Safe to pass to
    ///     <see cref="Infrastructure.Routing.GitLabRouteBuilder.Literal" />: every value is a fixed file
    ///     name from a closed vocabulary, never caller-supplied text.
    /// </summary>
    internal static string ToRouteValue(this GitLabRubyGemsSpecIndexFile file)
    {
        return file switch
        {
            GitLabRubyGemsSpecIndexFile.Specs => "specs.4.8.gz",
            GitLabRubyGemsSpecIndexFile.LatestSpecs => "latest_specs.4.8.gz",
            GitLabRubyGemsSpecIndexFile.PrereleaseSpecs => "prerelease_specs.4.8.gz",
            _ => throw new ArgumentOutOfRangeException(nameof(file), file, "Unknown RubyGems spec index file.")
        };
    }
}