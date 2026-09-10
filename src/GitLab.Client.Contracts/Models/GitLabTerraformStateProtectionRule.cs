namespace GitLab.Client.Models;

/// <summary>
///     A Terraform state protection rule (<c>/projects/:id/terraform/state_protection_rules</c>) - who may
///     write one named state, and from where. Introduced in GitLab 18.11.
/// </summary>
public sealed record GitLabTerraformStateProtectionRule
{
    public required long Id { get; init; }

    public long? ProjectId { get; init; }

    /// <summary>The Terraform state name the rule protects. Unique per project, at most 255 characters.</summary>
    public required string StateName { get; init; }

    /// <summary>
    ///     The minimum role required to write the state - <c>developer</c>, <c>maintainer</c>, <c>owner</c>
    ///     or <c>admin</c>. A string here because GitLab types the response field as free text; the request
    ///     side is the typed <see cref="GitLabTerraformStateWriteAccessLevel" />.
    /// </summary>
    public string? MinimumAccessLevelForWrite { get; init; }

    /// <summary>
    ///     Where writes are accepted from - <c>anywhere</c>, <c>ci_only</c> or
    ///     <c>ci_on_protected_branch_only</c>. A string for the same reason as
    ///     <see cref="MinimumAccessLevelForWrite" />.
    /// </summary>
    public string? AllowedFrom { get; init; }
}