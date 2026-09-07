namespace GitLab.Client.Models;

/// <summary>
///     Body of <c>POST /projects/:id/terraform/state_protection_rules</c>.
/// </summary>
public sealed record CreateTerraformStateProtectionRuleRequest
{
    /// <summary>The Terraform state name to protect. At most 255 characters, unique within the project.</summary>
    public required string StateName { get; init; }

    /// <summary>The minimum role required to write the state.</summary>
    public required GitLabTerraformStateWriteAccessLevel MinimumAccessLevelForWrite { get; init; }

    /// <summary>
    ///     Where writes are accepted from. Omit to take GitLab's default of
    ///     <see cref="GitLabTerraformStateAllowedFrom.Anywhere" />.
    /// </summary>
    public GitLabTerraformStateAllowedFrom? AllowedFrom { get; init; }
}