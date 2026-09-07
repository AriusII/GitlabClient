namespace GitLab.Client.Models;

/// <summary>
///     The trimmed three-field view of an external status check that GitLab echoes back inside
///     <see cref="GitLabStatusCheckResponse" />. Distinct from <see cref="GitLabExternalStatusCheck" />, which
///     is the full project-level configuration.
/// </summary>
public sealed record GitLabStatusCheckSummary
{
    public long? Id { get; init; }

    public string? Name { get; init; }

    public Uri? ExternalUrl { get; init; }
}