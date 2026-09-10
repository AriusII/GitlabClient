namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>PUT /projects/:id/releases/:tag_name</c>. Every property is optional: the
///     library's <c>WhenWritingNull</c> policy omits the ones left unset, so an update touches only the
///     fields it names.
/// </summary>
public sealed record UpdateReleaseRequest
{
    public string? Name { get; init; }

    /// <summary>The description of the release. Markdown is accepted.</summary>
    public string? Description { get; init; }

    /// <summary>When the release is - or was - ready.</summary>
    public DateTimeOffset? ReleasedAt { get; init; }

    /// <summary>
    ///     The title of each milestone to associate with the release. Cannot be combined with
    ///     <see cref="MilestoneIds" />; pass an empty list to detach every milestone.
    /// </summary>
    public IReadOnlyList<string>? Milestones { get; init; }

    /// <summary>
    ///     The ID of each milestone to associate with the release. Cannot be combined with
    ///     <see cref="Milestones" />.
    /// </summary>
    public IReadOnlyList<long>? MilestoneIds { get; init; }
}