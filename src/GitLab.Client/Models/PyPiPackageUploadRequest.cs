namespace GitLab.Client.Models;

/// <summary>
///     The non-file half of <c>POST /projects/:id/packages/pypi</c>, which uploads a PyPI package as
///     <c>multipart/form-data</c>. The package file itself is passed separately, as the upload's file
///     part (sent under the wire field name <c>content</c>, not the library's usual <c>file</c>).
/// </summary>
/// <remarks>
///     This record is projected onto multipart form fields rather than serialized as a JSON body - the
///     endpoint accepts no JSON. It carries System.Text.Json metadata all the same, so that the library's
///     "every model is registered" guard stays a real check rather than one with exemptions.
/// </remarks>
public sealed record PyPiPackageUploadRequest
{
    /// <summary>The package's distribution name, as PyPI's <c>name</c> metadata field.</summary>
    public required string Name { get; init; }

    /// <summary>The Python version specifier the package requires, such as <c>&gt;=3.7</c>.</summary>
    public string? RequiresPython { get; init; }

    /// <summary>MD5 checksum of the package file.</summary>
    public string? Md5Digest { get; init; }

    /// <summary>SHA256 checksum of the package file.</summary>
    public string? Sha256Digest { get; init; }

    /// <summary>The PyPI core-metadata version the package declares, such as <c>2.3</c>.</summary>
    public string? MetadataVersion { get; init; }

    /// <summary>The package author's email address.</summary>
    public string? AuthorEmail { get; init; }

    /// <summary>The package's long-form description.</summary>
    public string? Description { get; init; }

    /// <summary>The content type of <see cref="Description" />, such as <c>text/markdown; charset=UTF-8</c>.</summary>
    public string? DescriptionContentType { get; init; }

    /// <summary>A short one-line summary of the package.</summary>
    public string? Summary { get; init; }

    /// <summary>Comma-separated keywords describing the package.</summary>
    public string? Keywords { get; init; }
}