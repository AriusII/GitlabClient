namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>PUT /groups/:id/labels/:name</c>. GitLab requires at least one member to be set.
/// </summary>
public sealed record UpdateGroupLabelRequest
{
    /// <summary>Renames the label. The current name travels in the route, not the body.</summary>
    public string? NewName { get; init; }

    /// <summary>Six-digit hex notation with a leading <c>#</c>, or one of the CSS colour names GitLab allows.</summary>
    public string? Color { get; init; }

    public string? Description { get; init; }

    public bool? Archived { get; init; }
}