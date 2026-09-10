namespace GitLab.Client.Models.Requests;

/// <summary>Request body for <c>POST /groups/:id/epics</c>.</summary>
public sealed record CreateEpicRequest
{
    public required string Title { get; init; }

    public string? Description { get; init; }

    public string? Color { get; init; }

    public bool? Confidential { get; init; }

    /// <summary>Administrative backdating of the epic creation timestamp.</summary>
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>Deprecated by GitLab in favour of <see cref="StartDateFixed" />.</summary>
    public string? StartDate { get; init; }

    public string? StartDateFixed { get; init; }

    public bool? StartDateIsFixed { get; init; }

    /// <summary>Deprecated by GitLab in favour of <see cref="DueDateFixed" />.</summary>
    public string? EndDate { get; init; }

    public string? DueDateFixed { get; init; }

    public bool? DueDateIsFixed { get; init; }

    public IReadOnlyList<string>? Labels { get; init; }

    /// <summary>The database id of the parent epic, if this epic is created as a child.</summary>
    public long? ParentId { get; init; }
}