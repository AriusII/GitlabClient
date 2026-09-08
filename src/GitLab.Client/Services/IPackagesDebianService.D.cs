using GitLab.Client.Abstractions;
using GitLab.Client.Domain;

namespace GitLab.Client.Services;

/// <summary>Part D: the paired package-file upload operation.</summary>
internal partial interface IPackagesDebianService
{
    Task UploadPackageFileAsync(ProjectId projectId, string fileName, GitLabFileUpload file,
        CancellationToken cancellationToken = default);
}