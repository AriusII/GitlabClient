namespace GitLab.Client.Models;

/// <summary>
///     The body of <c>POST /groups/:id/dependency_list_exports</c>. Both members are optional; GitLab
///     defaults to <see cref="GitLabGroupDependencyListExportType.JsonArray" /> and to not sending an
///     email.
/// </summary>
public sealed record CreateGroupDependencyListExportRequest
{
    /// <summary>Email the requesting user once the export has finished generating.</summary>
    public bool? SendEmail { get; init; }

    /// <summary>The file format to produce.</summary>
    public GitLabGroupDependencyListExportType? ExportType { get; init; }
}