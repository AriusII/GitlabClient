namespace GitLab.Client.Models.Requests;

/// <summary>Partial update body for <c>PUT /groups/:id/epics/:epic_iid</c>.</summary>
public sealed record UpdateEpicRequest
{
    public string? Title { get; init; }

    public string? Color { get; init; }

    public string? Description { get; init; }

    public bool? Confidential { get; init; }

    /// <summary>Administrative backdating of the epic update timestamp.</summary>
    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>Deprecated by GitLab in favour of <see cref="StartDateFixed" />.</summary>
    public string? StartDate { get; init; }

    public string? StartDateFixed { get; init; }

    public bool? StartDateIsFixed { get; init; }

    /// <summary>Deprecated by GitLab in favour of <see cref="DueDateFixed" />.</summary>
    public string? EndDate { get; init; }

    public string? DueDateFixed { get; init; }

    public bool? DueDateIsFixed { get; init; }

    /// <summary>Replaces the full label set.</summary>
    public IReadOnlyList<string>? Labels { get; init; }

    public IReadOnlyList<string>? AddLabels { get; init; }

    public IReadOnlyList<string>? RemoveLabels { get; init; }

    public GitLabEpicStateEvent? StateEvent { get; init; }

    public long? ParentId { get; init; }
}