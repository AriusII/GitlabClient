namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>PUT /projects/:id/labels/:name</c>. GitLab requires at least one member to be
///     set; every one of them is optional individually.
/// </summary>
public sealed record UpdateLabelRequest
{
    /// <summary>Renames the label. The current name travels in the route, not the body.</summary>
    public string? NewName { get; init; }

    /// <summary>Six-digit hex notation with a leading <c>#</c>, or one of the CSS colour names GitLab allows.</summary>
    public string? Color { get; init; }

    public string? Description { get; init; }

    public bool? Archived { get; init; }

    public int? Priority { get; init; }
}