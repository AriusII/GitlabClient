namespace GitLab.Client.Models;

/// <summary>
///     A runner controller - the instance-level component that evaluates CI jobs on behalf of the runners
///     in its scope (<c>/runner_controllers</c>).
///     <para>
///         GitLab has two shapes for this entity: the list, register, update and delete endpoints return
///         the summary form, while <c>GET /runner_controllers/:id</c> returns the "detail" form. Both are
///         modelled here, so <see cref="Connected" /> is detail-only and comes back
///         <see langword="null" /> from every other call.
///     </para>
///     <para>This API area carries GitLab's <c>experiment</c> lifecycle marker and may change.</para>
/// </summary>
public sealed record GitLabRunnerController
{
    public required long Id { get; init; }

    public string? Description { get; init; }

    /// <summary>Whether the controller is disabled, live, or evaluating without acting.</summary>
    public GitLabRunnerControllerState? State { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>Whether the controller currently holds a connection to the instance. Detail-only.</summary>
    public bool? Connected { get; init; }
}