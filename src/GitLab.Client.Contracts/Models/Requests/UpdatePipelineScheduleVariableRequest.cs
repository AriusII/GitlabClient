namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>PUT /projects/:id/pipeline_schedules/:pipeline_schedule_id/variables/:key</c>.
///     The key is part of the route, so it cannot be changed - delete the variable and create a new one.
/// </summary>
public sealed record UpdatePipelineScheduleVariableRequest
{
    public string? Value { get; init; }

    /// <summary>Either <c>env_var</c> or <c>file</c>.</summary>
    public string? VariableType { get; init; }
}