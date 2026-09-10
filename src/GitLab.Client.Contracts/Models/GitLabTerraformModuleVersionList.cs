namespace GitLab.Client.Models;

/// <summary>
///     The response of the Terraform Module Registry API's "list all available versions for a module"
///     endpoint (<c>/packages/terraform/modules/v1/:module_namespace/:module_name/:module_system/versions</c>).
/// </summary>
public sealed record GitLabTerraformModuleVersionList
{
    /// <summary>
    ///     The module and its published versions. GitLab's response wraps a single module entry in this
    ///     array; the shape mirrors the community Terraform Module Registry Protocol.
    /// </summary>
    public IReadOnlyList<GitLabTerraformModuleVersionsEntry>? Modules { get; init; }
}