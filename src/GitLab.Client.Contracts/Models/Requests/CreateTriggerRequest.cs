namespace GitLab.Client.Models.Requests;

/// <summary>Request body for <c>POST /projects/:id/triggers</c>.</summary>
public sealed record CreateTriggerRequest
{
    /// <summary>Required by GitLab: a trigger token with no description cannot be created.</summary>
    public required string Description { get; init; }

    /// <summary>When the token stops working. Omit for a token that never expires.</summary>
    public DateTimeOffset? ExpiresAt { get; init; }
}