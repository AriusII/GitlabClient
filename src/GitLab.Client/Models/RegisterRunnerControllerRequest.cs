namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>POST /runner_controllers</c>. Both members are optional; omitted properties are
///     not sent, and GitLab defaults <see cref="State" /> to
///     <see cref="GitLabRunnerControllerState.Disabled" />.
/// </summary>
public sealed record RegisterRunnerControllerRequest
{
    /// <summary>Free-form description of the controller.</summary>
    public string? Description { get; init; }

    /// <summary>Whether the controller starts disabled, live, or evaluating without acting.</summary>
    public GitLabRunnerControllerState? State { get; init; }
}