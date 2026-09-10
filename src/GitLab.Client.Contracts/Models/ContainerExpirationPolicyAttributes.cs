using GitLab.Client.Models.Requests;

namespace GitLab.Client.Models;

/// <summary>
///     The nested <c>container_expiration_policy_attributes</c> object of
///     <see cref="CreateProjectRequest" /> and <see cref="UpdateProjectRequest" />: the cleanup policy
///     that decides which container-registry tags GitLab deletes on a schedule.
/// </summary>
/// <remarks>
///     The spec types this parameter as a bare untyped <c>object</c>, so the members below come from the
///     endpoint's documented attribute list rather than from a schema. Every one is optional, which also
///     means an attribute GitLab adds later can be sent through a newer version of this record without
///     breaking anything that already compiles.
/// </remarks>
public sealed record ContainerExpirationPolicyAttributes
{
    /// <summary>How often the policy runs - "1d", "7d", "14d", "1month" or "3month".</summary>
    public string? Cadence { get; init; }

    /// <summary>Whether the policy is active.</summary>
    public bool? Enabled { get; init; }

    /// <summary>How many tags matching <see cref="NameRegexKeep" /> to keep per image name.</summary>
    public int? KeepN { get; init; }

    /// <summary>Only delete tags older than this - "7d", "14d", "30d", "60d" or "90d".</summary>
    public string? OlderThan { get; init; }

    /// <summary>Delete tags whose name matches this regular expression.</summary>
    public string? NameRegexDelete { get; init; }

    /// <summary>Keep tags whose name matches this regular expression, whatever the other rules say.</summary>
    public string? NameRegexKeep { get; init; }
}