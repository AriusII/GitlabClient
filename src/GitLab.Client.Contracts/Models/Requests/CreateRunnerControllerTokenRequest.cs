namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>POST /runner_controllers/:id/tokens</c>.
///     <para>
///         GitLab returns the freshly minted secret exactly once with the creation response. Persist it immediately and
///         never write it to logs.
///     </para>
/// </summary>
public sealed record CreateRunnerControllerTokenRequest
{
    /// <summary>Free-form description of the token - what it is for, or which controller host holds it.</summary>
    public required string Description { get; init; }
}