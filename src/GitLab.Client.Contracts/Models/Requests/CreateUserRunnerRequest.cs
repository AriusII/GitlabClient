namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>POST /user/runners</c> - the modern replacement for the removed registration
///     token flow, which mints a runner owned by the authenticated user.
///     <para>
///         <see cref="RunnerType" /> decides which of <see cref="GroupId" /> and <see cref="ProjectId" />
///         is mandatory: <c>group_type</c> needs a group, <c>project_type</c> needs a project, and
///         <c>instance_type</c> needs neither but requires instance administrator rights. The spec marks
///         both ids required, which cannot be true of all three runner types at once; they are optional
///         here and GitLab answers a <see cref="Abstractions.Exceptions.GitLabValidationException" /> when
///         the one its runner type needs is missing.
///     </para>
///     <para>
///         <see cref="RunnerType" /> and <see cref="AccessLevel" /> are modelled as strings rather than
///         enums to stay consistent with <see cref="GitLabRunner.RunnerType" /> and
///         <see cref="UpdateRunnerRequest.AccessLevel" />, which read the same two fields back.
///     </para>
/// </summary>
public sealed record CreateUserRunnerRequest
{
    /// <summary>The scope of the runner: <c>instance_type</c>, <c>group_type</c> or <c>project_type</c>.</summary>
    public required string RunnerType { get; init; }

    /// <summary>The group the runner is created in. Required when <see cref="RunnerType" /> is <c>group_type</c>.</summary>
    public long? GroupId { get; init; }

    /// <summary>The project the runner is created in. Required when <see cref="RunnerType" /> is <c>project_type</c>.</summary>
    public long? ProjectId { get; init; }

    /// <summary>Free-form description of the runner.</summary>
    public string? Description { get; init; }

    /// <summary>Free-form maintenance note, up to 1024 characters.</summary>
    public string? MaintenanceNote { get; init; }

    /// <summary>Whether the runner should ignore new jobs. GitLab defaults this to false.</summary>
    public bool? Paused { get; init; }

    /// <summary>Whether the runner refuses to be assigned to further projects. GitLab defaults this to false.</summary>
    public bool? Locked { get; init; }

    /// <summary>The access level of the runner: <c>not_protected</c> or <c>ref_protected</c>.</summary>
    public string? AccessLevel { get; init; }

    /// <summary>Whether the runner may pick up jobs that carry no tags. GitLab defaults this to true.</summary>
    public bool? RunUntagged { get; init; }

    /// <summary>The runner's tags.</summary>
    public IReadOnlyList<string>? TagList { get; init; }

    /// <summary>Maximum time in seconds a job may run on this runner.</summary>
    public int? MaximumTimeout { get; init; }

    /// <summary>
    ///     When the authentication token should expire. GitLab requires it to be between five minutes and
    ///     fifteen days in the future.
    /// </summary>
    public DateTimeOffset? TokenExpiresAt { get; init; }

    /// <summary>
    ///     The deadline for rotating the token. Only accepted together with <see cref="TokenExpiresAt" />,
    ///     and must not be later than it.
    /// </summary>
    public DateTimeOffset? TokenRotationDeadline { get; init; }
}