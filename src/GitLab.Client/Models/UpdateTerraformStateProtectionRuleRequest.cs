namespace GitLab.Client.Models;

/// <summary>
///     Body of <c>PATCH /projects/:id/terraform/state_protection_rules/:rule_id</c>. Every member is
///     optional; anything left null is not sent and the rule keeps its current value.
/// </summary>
public sealed record UpdateTerraformStateProtectionRuleRequest
{
    /// <summary>Repoint the rule at a different Terraform state name.</summary>
    public string? StateName { get; init; }

    /// <summary>Raise or lower the minimum role required to write the state.</summary>
    public GitLabTerraformStateWriteAccessLevel? MinimumAccessLevelForWrite { get; init; }

    /// <summary>Change where writes are accepted from.</summary>
    public GitLabTerraformStateAllowedFrom? AllowedFrom { get; init; }
}