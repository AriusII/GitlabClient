namespace GitLab.Client.Models;

/// <summary>
///     The compact release projection embedded in a repository tag response. This is intentionally distinct
///     from <see cref="GitLabRelease" />, whose endpoint supplies a substantially larger release payload.
/// </summary>
public sealed record GitLabTagRelease
{
    /// <summary>The tag the release belongs to.</summary>
    public string? TagName { get; init; }

    public string? Description { get; init; }
}