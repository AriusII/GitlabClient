namespace GitLab.Client.Models;

/// <summary>
///     Request body for the collection form <c>PUT /groups/:id/labels</c>, which identifies the label in the
///     body rather than in the route. Set exactly one of <see cref="LabelId" /> or <see cref="Name" />; use
///     <see cref="UpdateGroupLabelRequest" /> with the <c>/labels/:name</c> route for the common case.
/// </summary>
public sealed record UpdateGroupLabelByIdRequest
{
    /// <summary>The numeric id of the label to update.</summary>
    public long? LabelId { get; init; }

    /// <summary>The current name of the label to update, when addressing it by name instead of id.</summary>
    public string? Name { get; init; }

    /// <summary>Renames the label.</summary>
    public string? NewName { get; init; }

    /// <summary>Six-digit hex notation with a leading <c>#</c>, or one of the CSS colour names GitLab allows.</summary>
    public string? Color { get; init; }

    public string? Description { get; init; }

    public bool? Archived { get; init; }
}