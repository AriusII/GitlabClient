using GitLab.Client.Domain;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps GitLab's Cargo registry (<c>/projects/:id/packages/cargo/...</c>) - the sparse-index
///     protocol Cargo itself speaks, plus <c>.crate</c> downloads. GitLab has no Cargo publish endpoint;
///     publishing goes through <c>cargo publish</c> against the sparse index's own upload machinery,
///     which is not exposed as a documented REST operation.
///     <para>
///         Every response here is raw bytes rather than JSON - the sparse-index entries are
///         newline-delimited JSON documents the caller parses itself, and <c>config.json</c> and the
///         crate download are opaque too - so every method returns a <see cref="GitLabFileResponse" />
///         the caller must dispose.
///     </para>
///     <para>
///         Cargo's sparse-index protocol shapes the crate's URL by the length of its name: 1- and
///         2-character names get their own flat route, a 3-character name adds one prefix character, and
///         everything else (4+ characters) is split into two 2-character prefix segments. That is why
///         this client exposes four differently-shaped lookup methods instead of one - the shape is a
///         property of the protocol, not an API design choice.
///     </para>
/// </summary>
public interface IPackagesCargoClient
{
    /// <summary>Gets the sparse-index entry for a 1-character crate name.</summary>
    Task<GitLabFileResponse> GetSparseIndexForOneCharacterNameAsync(ProjectId projectId, string packageName,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the sparse-index entry for a 2-character crate name.</summary>
    Task<GitLabFileResponse> GetSparseIndexForTwoCharacterNameAsync(ProjectId projectId, string packageName,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the sparse-index entry for a 3-character crate name. <paramref name="firstChar" /> is the
    ///     crate name's first character, exactly as the Cargo sparse-index protocol splits the route.
    /// </summary>
    Task<GitLabFileResponse> GetSparseIndexForThreeCharacterNameAsync(ProjectId projectId, string firstChar,
        string packageName, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the sparse-index entry for a crate name of 4 or more characters.
    ///     <paramref name="prefix1" /> is the name's first two characters and <paramref name="prefix2" /> its
    ///     next two, exactly as the Cargo sparse-index protocol splits the route.
    /// </summary>
    Task<GitLabFileResponse> GetSparseIndexAsync(ProjectId projectId, string prefix1, string prefix2,
        string packageName, CancellationToken cancellationToken = default);

    /// <summary>Gets the registry's <c>config.json</c>, which points a Cargo client at this index and its API.</summary>
    Task<GitLabFileResponse> GetConfigAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    /// <summary>Downloads the <c>.crate</c> file for one published version of a crate.</summary>
    Task<GitLabFileResponse> DownloadCrateAsync(ProjectId projectId, string packageName, string packageVersion,
        CancellationToken cancellationToken = default);
}