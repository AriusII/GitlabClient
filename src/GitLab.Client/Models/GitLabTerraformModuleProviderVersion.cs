namespace GitLab.Client.Models;

/// <summary>A provider version requirement within a <see cref="GitLabTerraformModuleRoot" />.</summary>
public sealed record GitLabTerraformModuleProviderVersion
{
    /// <summary>The provider name, for example <c>local</c> or <c>aws</c>.</summary>
    public string? Name { get; init; }

    /// <summary>The required provider version constraint, when GitLab reports one.</summary>
    public string? Version { get; init; }
}