namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>POST /groups/:id/labels</c>. Unlike <see cref="CreateLabelRequest" /> it has no
///     <c>priority</c>: label priority is a project-level ordering GitLab does not model on groups.
/// </summary>
public sealed record CreateGroupLabelRequest
{
    public required string Name { get; init; }

    /// <summary>Six-digit hex notation with a leading <c>#</c>, or one of the CSS colour names GitLab allows.</summary>
    public required string Color { get; init; }

    public string? Description { get; init; }

    public bool? Archived { get; init; }
}