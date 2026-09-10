using System.Diagnostics.CodeAnalysis;

namespace GitLab.Client.Models.Requests;

/// <summary>Request body for <c>POST /projects/:id/badges</c> and <c>POST /groups/:id/badges</c>.</summary>
public sealed record CreateBadgeRequest
{
    /// <summary>
    ///     Where the badge links to. May contain GitLab placeholders - <c>%{project_path}</c>,
    ///     <c>%{default_branch}</c>, <c>%{commit_sha}</c> - which GitLab resolves per project.
    /// </summary>
    [SuppressMessage("Design", "CA1056",
        Justification =
            "The request must be able to SEND placeholder-bearing URLs such as "
            + "https://example.com/%{project_path}/pipeline.svg, which System.Uri cannot round-trip because "
            + "'%{' is not a valid percent-escape.")]
    public required string LinkUrl { get; init; }

    /// <summary>The badge image source. May contain the same GitLab placeholders as <see cref="LinkUrl" />.</summary>
    [SuppressMessage("Design", "CA1056",
        Justification = "See LinkUrl: badge URLs must be able to carry unescaped GitLab placeholders.")]
    public required string ImageUrl { get; init; }

    public string? Name { get; init; }
}