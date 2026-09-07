namespace GitLab.Client.Models;

/// <summary>
///     A project topic, as returned by the GitLab Project topics API (<c>/topics</c>) - the instance-wide
///     tag vocabulary projects are labelled with.
/// </summary>
/// <remarks>
///     Reading topics is open to any authenticated user; creating, updating, merging and deleting them is
///     administrator-only.
/// </remarks>
public sealed record GitLabTopic
{
    /// <summary>The topic's numeric id, and the value every other topic route takes.</summary>
    public required long Id { get; init; }

    /// <summary>
    ///     The topic's slug - the value that appears on a project's <c>topics</c> list and in search. Unique
    ///     across the instance (or across the organization, where organizations are in use).
    /// </summary>
    public required string Name { get; init; }

    /// <summary>The topic's human-readable title, shown in the UI in place of <see cref="Name" />.</summary>
    public string? Title { get; init; }

    /// <summary>Free-text description shown on the topic's page. Markdown, and empty rather than absent when unset.</summary>
    public string? Description { get; init; }

    /// <summary>How many projects currently carry this topic. GitLab sorts the topic list by this, descending.</summary>
    public int? TotalProjectsCount { get; init; }

    /// <summary>The organization the topic belongs to, on instances where organizations are enabled.</summary>
    public long? OrganizationId { get; init; }

    /// <summary>Absolute URL of the topic's avatar, or null when it has none.</summary>
    public Uri? AvatarUrl { get; init; }
}