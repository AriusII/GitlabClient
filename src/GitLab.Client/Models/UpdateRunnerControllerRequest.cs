namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>PUT /runner_controllers/:id</c>. Every member is optional - omitted properties
///     are not sent, so they keep their current server-side value.
/// </summary>
public sealed record UpdateRunnerControllerRequest
{
    /// <summary>Free-form description of the controller.</summary>
    public string? Description { get; init; }

    /// <summary>Whether the controller is disabled, live, or evaluating without acting.</summary>
    public GitLabRunnerControllerState? State { get; init; }
}