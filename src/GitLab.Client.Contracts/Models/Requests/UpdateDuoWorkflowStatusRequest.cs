namespace GitLab.Client.Models.Requests;

/// <summary>Body of <c>PATCH /ai/duo_workflows/workflows/:id</c>.</summary>
public sealed record UpdateDuoWorkflowStatusRequest
{
    /// <summary>
    ///     The state transition to apply - <c>finish</c>, <c>drop</c>, <c>pause</c>, <c>resume</c> and so
    ///     on. Left as free text because the spec declares no enumeration for it, and a value GitLab adds
    ///     later must not become unreachable from this client.
    /// </summary>
    public required string StatusEvent { get; init; }
}