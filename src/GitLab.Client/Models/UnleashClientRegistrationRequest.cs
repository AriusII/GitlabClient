namespace GitLab.Client.Models;

/// <summary>
///     Body of <c>POST /feature_flags/unleash/:project_id/client/register</c> and
///     <c>POST /feature_flags/unleash/:project_id/client/metrics</c>.
///     <para>
///         The spec declares only these two members. A real Unleash client also sends its strategy list on
///         register and its per-flag counters on metrics; GitLab accepts and ignores both, so nothing here
///         models them.
///     </para>
/// </summary>
public sealed record UnleashClientRegistrationRequest
{
    /// <summary>The Unleash client's instance ID.</summary>
    public string? InstanceId { get; init; }

    /// <summary>The Unleash client's application name.</summary>
    public string? AppName { get; init; }
}