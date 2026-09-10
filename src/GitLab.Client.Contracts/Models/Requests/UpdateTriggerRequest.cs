namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>PUT /projects/:id/triggers/:trigger_id</c>. The description is the only field
///     GitLab lets you change; the token value itself is immutable.
/// </summary>
public sealed record UpdateTriggerRequest
{
    public string? Description { get; init; }
}