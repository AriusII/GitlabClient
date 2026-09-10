namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for the runner-process registration protocol, <c>POST /runners</c>.
///     <para>
///         This endpoint consumes a registration token and is distinct from
///         <c>POST /user/runners</c>, which creates a runner for the authenticated GitLab user. New
///         applications should generally prefer the user-owned runner flow. This protocol shape remains
///         useful when implementing a custom GitLab Runner-compatible process.
///     </para>
/// </summary>
public sealed record RegisterRunnerRequest
{
    /// <summary>The registration token supplied by the runner's owner. It is required by GitLab.</summary>
    public required string Token { get; init; }

    /// <summary>Free-form description of the runner.</summary>
    public string? Description { get; init; }

    /// <summary>Free-form maintenance note, limited to 1024 characters by GitLab.</summary>
    public string? MaintenanceNote { get; init; }

    /// <summary>Runtime metadata reported by the registering runner process.</summary>
    public GitLabRunnerMetadata? Info { get; init; }

    /// <summary>Whether the runner should initially ignore new jobs.</summary>
    public bool? Paused { get; init; }

    /// <summary>Whether GitLab must lock the runner to its current project.</summary>
    public bool? Locked { get; init; }

    /// <summary>The reference protection level: <c>not_protected</c> or <c>ref_protected</c>.</summary>
    public string? AccessLevel { get; init; }

    /// <summary>Whether the runner may execute untagged jobs.</summary>
    public bool? RunUntagged { get; init; }

    /// <summary>The complete set of runner tags.</summary>
    public IReadOnlyList<string>? TagList { get; init; }

    /// <summary>The maximum job runtime in seconds.</summary>
    public int? MaximumTimeout { get; init; }
}