namespace GitLab.Client.Models.Requests;

/// <summary>Request body for <c>POST /projects/:id/labels</c>.</summary>
public sealed record CreateLabelRequest
{
    public required string Name { get; init; }

    /// <summary>
    ///     Six-digit hex notation with a leading <c>#</c> (<c>#FFAABB</c>), or one of the CSS colour names
    ///     GitLab allows. Required by GitLab, unlike on the update form.
    /// </summary>
    public required string Color { get; init; }

    public string? Description { get; init; }

    public bool? Archived { get; init; }

    /// <summary>Pin the label to the top of the project's list; <c>null</c> leaves it unprioritised.</summary>
    public int? Priority { get; init; }
}