namespace GitLab.Client.Models;

/// <summary>
///     The non-file half of <c>POST /projects/import</c>, which uploads a project export archive as
///     <c>multipart/form-data</c>. The archive itself is passed separately, as the upload's file part.
/// </summary>
/// <remarks>
///     This record is projected onto multipart form fields rather than serialized as a JSON body - the
///     endpoint accepts no JSON. It carries System.Text.Json metadata all the same, so that the library's
///     "every model is registered" guard stays a real check rather than one with exemptions.
/// </remarks>
public sealed record ImportProjectArchiveRequest
{
    /// <summary>The path (and, unless <see cref="Name" /> is given, the name) of the project to create.</summary>
    public required string Path { get; init; }

    /// <summary>The name of the new project. Defaults to <see cref="Path" />.</summary>
    public string? Name { get; init; }

    /// <summary>
    ///     The ID of the namespace to import into. Defaults to the caller's own namespace. Mutually
    ///     exclusive with <see cref="NamespacePath" />.
    /// </summary>
    public long? NamespaceId { get; init; }

    /// <summary>
    ///     The path of the namespace to import into, such as <c>new_path/gitlab</c>. Mutually exclusive
    ///     with <see cref="NamespaceId" />.
    /// </summary>
    public string? NamespacePath { get; init; }

    /// <summary>Replace an existing project of the same name in the same namespace. Defaults to false.</summary>
    public bool? Overwrite { get; init; }

    /// <summary>
    ///     Project settings to apply on top of what the archive carries, keyed by GitLab's own parameter
    ///     names (<c>description</c>, <c>visibility</c>, <c>build_timeout</c>, and so on). Sent as
    ///     <c>override_params[key]</c> form fields, so every value goes on the wire as text.
    /// </summary>
    public IReadOnlyDictionary<string, string>? OverrideParams { get; init; }
}