namespace GitLab.Client.Models;

/// <summary>
///     Body of <c>POST /projects/:id/ai_agent/identities</c>. Registration is idempotent: the same user,
///     project, agent type and machine fingerprint always resolves to the same identity.
/// </summary>
public sealed record RegisterAgentIdentityRequest
{
    /// <summary>The agent being registered.</summary>
    public required GitLabAgentType AgentType { get; init; }

    /// <summary>SHA-256 hash of the machine identifier, which is what makes the registration per-device.</summary>
    public required string MachineFingerprint { get; init; }
}