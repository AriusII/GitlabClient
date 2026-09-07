namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>POST /runner_controllers/:id/tokens</c>.
///     <para>
///         Creating a token does not hand back its secret: GitLab answers with the token's metadata only.
///         Call <c>RotateTokenAsync</c> to obtain a usable secret.
///     </para>
/// </summary>
public sealed record CreateRunnerControllerTokenRequest
{
    /// <summary>Free-form description of the token - what it is for, or which controller host holds it.</summary>
    public string? Description { get; init; }
}