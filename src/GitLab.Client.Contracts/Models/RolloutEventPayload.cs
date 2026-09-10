using GitLab.Client.Models.Requests;

namespace GitLab.Client.Models;

/// <summary>
///     The <c>data</c>/<c>value</c> payload of a rollout workflow event
///     (<see cref="IngestRolloutEventRequest" />) - flow-graph progress reported by the CD orchestrator
///     (a Starlark workflow run by AutoFlow). Which members are populated depends on the event's
///     <see cref="IngestRolloutEventRequest.Type" />.
/// </summary>
public sealed record RolloutEventPayload
{
    /// <summary>
    ///     Zero-based path to the stage/step in the flow definition. Required for every event except
    ///     <c>com.gitlab.cd.rollout_succeeded</c>.
    /// </summary>
    public IReadOnlyList<int>? Position { get; init; }

    /// <summary>Name of the enclosing stage (an environment tier); absent for a step outside any stage.</summary>
    public string? StageName { get; init; }

    /// <summary>
    ///     Exact name of the target GitLab environment; resolves the rollout environment to update when a
    ///     stage deploys to more than one environment.
    /// </summary>
    public string? Environment { get; init; }

    /// <summary>The step type, present on <c>step_*</c> events.</summary>
    public string? StepType { get; init; }

    /// <summary>
    ///     Name of the <c>Cd::Service</c>, present on <c>service_started</c>/<c>service_succeeded</c>/
    ///     <c>service_failed</c>.
    /// </summary>
    public string? Service { get; init; }

    /// <summary>Failure detail, present on <c>step_failed</c> and <c>service_failed</c>.</summary>
    public string? Error { get; init; }

    /// <summary>Human-readable prompt, present on <c>approval_requested</c>.</summary>
    public string? Reason { get; init; }

    /// <summary>
    ///     Name of the AutoFlow channel to post the approval decision back into, present on
    ///     <c>approval_requested</c>.
    /// </summary>
    public string? Reply { get; init; }
}