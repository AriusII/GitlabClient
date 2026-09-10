using System.Text;
using System.Text.Json;

namespace GitLab.Client.Models.Requests;

/// <summary>
///     Body for <c>POST /projects/remote-import-s3</c>, which imports a project from an export archive
///     stored in an AWS S3 bucket.
/// </summary>
/// <remarks>
///     <see cref="AccessKeyId" /> and <see cref="SecretAccessKey" /> are AWS credentials. Keep them on
///     this record: never log a populated request, and never fold one into a message a user or a log sink
///     will see.
/// </remarks>
public sealed record ImportProjectFromS3Request
{
    /// <summary>The AWS region the bucket lives in, such as <c>eu-west-1</c>.</summary>
    public required string Region { get; init; }

    /// <summary>The bucket holding the archive.</summary>
    public required string BucketName { get; init; }

    /// <summary>The object key of the archive within the bucket.</summary>
    public required string FileKey { get; init; }

    /// <summary>The AWS access key id to read the object with.</summary>
    public required string AccessKeyId { get; init; }

    /// <summary>The AWS secret access key paired with <see cref="AccessKeyId" />. A secret.</summary>
    public required string SecretAccessKey { get; init; }

    /// <summary>The path (and, unless <see cref="Name" /> is given, the name) of the project to create.</summary>
    public required string Path { get; init; }

    /// <summary>The name of the new project. Defaults to <see cref="Path" />.</summary>
    public string? Name { get; init; }

    /// <summary>
    ///     The legacy ID or path of the namespace to import into. Mutually exclusive with
    ///     <see cref="NamespaceId" /> and <see cref="NamespacePath" />.
    /// </summary>
    public string? Namespace { get; init; }

    /// <summary>
    ///     The ID of the namespace to import into. Defaults to the caller's own namespace. Mutually
    ///     exclusive with <see cref="Namespace" /> and <see cref="NamespacePath" />.
    /// </summary>
    public long? NamespaceId { get; init; }

    /// <summary>
    ///     The path of the namespace to import into. Mutually exclusive with <see cref="Namespace" /> and
    ///     <see cref="NamespaceId" />.
    /// </summary>
    public string? NamespacePath { get; init; }

    /// <summary>Replace an existing project of the same name in the same namespace. Defaults to false.</summary>
    public bool? Overwrite { get; init; }

    /// <summary>
    ///     Project settings to apply on top of what the archive carries, carried as a raw
    ///     <see cref="JsonElement" /> for the same reason as
    ///     <see cref="ImportProjectFromRemoteArchiveRequest.OverrideParams" />.
    /// </summary>
    public JsonElement? OverrideParams { get; init; }

    private bool PrintMembers(StringBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Append("Region = ").Append(Region)
            .Append(", BucketName = ").Append(BucketName)
            .Append(", FileKey = ").Append(FileKey)
            .Append(", AccessKeyId = ")
            .Append(string.IsNullOrEmpty(AccessKeyId) ? "[none]" : "[redacted]")
            .Append(", SecretAccessKey = ")
            .Append(string.IsNullOrEmpty(SecretAccessKey) ? "[none]" : "[redacted]")
            .Append(", Path = ").Append(Path)
            .Append(", Name = ").Append(Name)
            .Append(", Namespace = ").Append(Namespace)
            .Append(", NamespaceId = ").Append(NamespaceId)
            .Append(", NamespacePath = ").Append(NamespacePath)
            .Append(", Overwrite = ").Append(Overwrite)
            .Append(", OverrideParams = ").Append(OverrideParams.HasValue ? "[present]" : "[none]");

        return true;
    }
}