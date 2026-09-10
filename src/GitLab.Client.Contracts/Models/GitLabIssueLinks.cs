namespace GitLab.Client.Models;

/// <summary>The endpoint routes GitLab exposes in an issue's <c>_links</c> member.</summary>
/// <remarks>
///     The GitLab 19.4 OpenAPI schema declares these members as strings without a URI format. They remain
///     strings here so a valid response is preserved exactly, including relative or instance-specific routes.
/// </remarks>
public sealed record GitLabIssueLinks
{
    public string? Self { get; init; }

    public string? Notes { get; init; }

    public string? AwardEmoji { get; init; }

    public string? Project { get; init; }

    public string? ClosedAsDuplicateOf { get; init; }
}