using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class PackagesPyPiRepository(IGitLabApiConnection connection) : IPackagesPyPiRepository
{
    /// <summary>
    ///     The form field GitLab's PyPI upload endpoint reads the package file from - "file", the
    ///     <see cref="GitLabFileUpload" /> default, is not the name this endpoint expects.
    /// </summary>
    private const string ContentFieldName = "content";

    public Task<GitLabFileResponse> DownloadFileForGroupAsync(GroupId groupId, string sha256, string fileIdentifier,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("-").Literal("packages").Literal("pypi")
                .Literal("files").Escaped(sha256).Escaped(fileIdentifier).Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetSimpleIndexForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("-").Literal("packages").Literal("pypi")
                .Literal("simple").Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetSimplePackageForGroupAsync(GroupId groupId, string packageName,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal("-").Literal("packages").Literal("pypi")
                .Literal("simple").Escaped(packageName).Build(),
            cancellationToken);
    }

    public Task UploadAsync(ProjectId projectId, GitLabFileUpload content, PyPiPackageUploadRequest metadata,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(metadata);

        return connection.PostFileAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Literal("pypi").Build(),
            content with { FieldName = ContentFieldName },
            BuildFormFields(metadata),
            cancellationToken);
    }

    public Task AuthorizeUploadAsync(ProjectId projectId, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Literal("pypi")
                .Literal("authorize").Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadFileAsync(ProjectId projectId, string sha256, string fileIdentifier,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Literal("pypi")
                .Literal("files").Escaped(sha256).Escaped(fileIdentifier).Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetSimpleIndexForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Literal("pypi")
                .Literal("simple").Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> GetSimplePackageForProjectAsync(ProjectId projectId, string packageName,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Literal("pypi")
                .Literal("simple").Escaped(packageName).Build(),
            cancellationToken);
    }

    private static Dictionary<string, string> BuildFormFields(PyPiPackageUploadRequest metadata)
    {
        Dictionary<string, string> formFields = new(StringComparer.Ordinal) { ["name"] = metadata.Name };

        if (metadata.RequiresPython is { } requiresPython)
        {
            formFields["requires_python"] = requiresPython;
        }

        if (metadata.Md5Digest is { } md5Digest)
        {
            formFields["md5_digest"] = md5Digest;
        }

        if (metadata.Sha256Digest is { } sha256Digest)
        {
            formFields["sha256_digest"] = sha256Digest;
        }

        if (metadata.MetadataVersion is { } metadataVersion)
        {
            formFields["metadata_version"] = metadataVersion;
        }

        if (metadata.AuthorEmail is { } authorEmail)
        {
            formFields["author_email"] = authorEmail;
        }

        if (metadata.Description is { } description)
        {
            formFields["description"] = description;
        }

        if (metadata.DescriptionContentType is { } descriptionContentType)
        {
            formFields["description_content_type"] = descriptionContentType;
        }

        if (metadata.Summary is { } summary)
        {
            formFields["summary"] = summary;
        }

        if (metadata.Keywords is { } keywords)
        {
            formFields["keywords"] = keywords;
        }

        return formFields;
    }
}