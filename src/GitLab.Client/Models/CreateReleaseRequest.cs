namespace GitLab.Client.Models;

/// <summary>Request body for <c>POST /projects/:id/releases</c>.</summary>
public sealed record CreateReleaseRequest
{
    public required string TagName { get; init; }

    /// <summary>
    ///     Message to use if <see cref="TagName" /> doesn't exist yet and GitLab creates it as a new
    ///     annotated tag.
    /// </summary>
    public string? TagMessage { get; init; }

    public string? Ref { get; init; }

    public string? Name { get; init; }

    public string? Description { get; init; }

    /// <summary>
    ///     When the release is - or was - ready. Defaults to the current time; only set this when
    ///     creating an upcoming or historical release.
    /// </summary>
    public DateTimeOffset? ReleasedAt { get; init; }

    /// <summary>
    ///     The title of each milestone to associate with the release. Cannot be combined with
    ///     <see cref="MilestoneIds" />.
    /// </summary>
    public IReadOnlyList<string>? Milestones { get; init; }

    /// <summary>
    ///     The ID of each milestone to associate with the release. Cannot be combined with
    ///     <see cref="Milestones" />.
    /// </summary>
    public IReadOnlyList<long>? MilestoneIds { get; init; }
}