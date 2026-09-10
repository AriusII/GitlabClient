namespace GitLab.Client.Models;

/// <summary>
///     One point of a project's deployment-frequency series, as returned by
///     <c>GET /projects/:id/analytics/deployment_frequency</c>. GitLab's OpenAPI document types all three
///     members as plain strings rather than numbers/dates, so they are kept that way here instead of
///     guessing a stricter shape.
/// </summary>
public sealed record GitLabDeploymentFrequency
{
    public required string Value { get; init; }

    public required string From { get; init; }

    public required string To { get; init; }
}