namespace GitLab.Client.Models;

/// <summary>
///     The response of publishing a Terraform module package file
///     (<c>PUT /projects/:id/packages/terraform/modules/:module_name/:module_system/:module_version/file</c>).
///     GitLab answers <c>201 Created</c> with a confirmation message rather than a created resource.
/// </summary>
public sealed record GitLabTerraformModuleUploadResult
{
    /// <summary>GitLab's confirmation message, for example <c>"201 Created"</c>.</summary>
    public string? Message { get; init; }
}