namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>POST /projects/:id/pipeline_schedules/:pipeline_schedule_id/variables</c>.
///     <para>
///         Deliberately narrower than <see cref="CreateVariableRequest" />: a schedule variable takes only a
///         key, a value and a type. GitLab's project and group variable options - masking, protection,
///         environment scope - do not exist on this endpoint.
///     </para>
/// </summary>
public sealed record CreatePipelineScheduleVariableRequest
{
    public required string Key { get; init; }

    public required string Value { get; init; }

    /// <summary>Either <c>env_var</c> (the default) or <c>file</c>.</summary>
    public string? VariableType { get; init; }
}