namespace GitLab.Client.Models;

/// <summary>One module entry within a <see cref="GitLabTerraformModuleVersionList" />.</summary>
public sealed record GitLabTerraformModuleVersionsEntry
{
    /// <summary>Every published version of the module, along with its dependency and provider metadata.</summary>
    public IReadOnlyList<GitLabTerraformModuleVersionInfo>? Versions { get; init; }

    /// <summary>The module's source repository.</summary>
    public Uri? Source { get; init; }
}