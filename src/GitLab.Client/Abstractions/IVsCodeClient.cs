using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "VSCode" API area (<c>/vscode/settings_sync/...</c>) - the server half of Visual
///     Studio Code's Settings Sync protocol, which lets a user's editor preferences follow them between
///     VS Code and the GitLab Web IDE.
///     <para>
///         <b>Who this is for.</b> These endpoints exist to be driven by an editor, not by application
///         code: the VS Code extension and the Web IDE call them to push and pull the user's own
///         settings, and every route is implicitly scoped to the authenticated user - there is no
///         user id anywhere in the surface. There is no reason to reach for this client unless you are
///         building an editor integration or migrating a user's synced settings; nothing else in this
///         library depends on it.
///     </para>
///     <para>
///         <b>Maturity.</b> The pinned spec (19.4) attaches no <c>x-gitlab-lifecycle</c> marker to any of
///         these ten operations, so they are not formally flagged experimental - but the tag is young and
///         GitLab-internal-leaning, and its schemas carry no declared field documentation at all. Every
///         response DTO here is therefore entirely nullable and entirely <see langword="string" />-typed,
///         matching the spec exactly rather than guessing at richer shapes that a later release could
///         contradict.
///     </para>
///     <para>
///         <b>The two route families.</b> Every operation exists twice in the spec: once user-global, and
///         once with a <c>{settings_context_hash}</c> segment that scopes the sync to one settings
///         context (the Web IDE uses it to keep per-context settings apart). They are exposed as one
///         method each with an optional <c>settingsContextHash</c>: leave it <see langword="null" /> for
///         the user-global form, pass a hash for the scoped one.
///     </para>
/// </summary>
public interface IVsCodeClient
{
    /// <summary>
    ///     Gets the Settings Sync manifest - what an editor polls to learn whether the server holds
    ///     anything newer than its local copy.
    /// </summary>
    /// <param name="settingsContextHash">
    ///     The settings context to scope to, or <see langword="null" /> for the user-global manifest.
    /// </param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task<GitLabVsCodeSettingsManifest> GetManifestAsync(string? settingsContextHash = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the stored revisions of one settings resource as references - a URL and a creation
    ///     time each, without the bodies. Fetch a body with
    ///     <see cref="GetSettingAsync(GitLabVsCodeSettingResource, string, string?, CancellationToken)" />.
    /// </summary>
    /// <param name="resourceName">Which settings resource to list revisions of.</param>
    /// <param name="settingsContextHash">
    ///     The settings context to scope to, or <see langword="null" /> for the user-global collection.
    /// </param>
    /// <param name="cancellationToken">Cancels the request.</param>
    IAsyncEnumerable<GitLabVsCodeSettingReference> ListReferencesAsync(GitLabVsCodeSettingResource resourceName,
        string? settingsContextHash = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets one stored revision of a settings resource, with its payload.
    ///     <para>
    ///         The spec documents a <c>204 No Content</c> answer alongside the <c>200</c>: GitLab returns
    ///         it when there is nothing stored for the revision. A success with no body is a protocol
    ///         error everywhere else in this library, so the transport raises
    ///         <see cref="Exceptions.GitLabApiException" /> ("GitLab returned an empty response body")
    ///         rather than inventing an empty <see cref="GitLabVsCodeSetting" /> - read that exception as
    ///         "nothing synced yet", not as a failure.
    ///     </para>
    /// </summary>
    /// <param name="resourceName">Which settings resource the revision belongs to.</param>
    /// <param name="id">
    ///     The revision id, as taken from a <see cref="GitLabVsCodeSettingReference.Url" />. VS Code also
    ///     accepts the sentinel <c>latest</c> here.
    /// </param>
    /// <param name="settingsContextHash">
    ///     The settings context to scope to, or <see langword="null" /> for the user-global collection.
    /// </param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task<GitLabVsCodeSetting> GetSettingAsync(GitLabVsCodeSettingResource resourceName, string id,
        string? settingsContextHash = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates or updates a settings resource.
    ///     <para>
    ///         <b>No payload is sent.</b> The spec declares no request body for this operation - it
    ///         documents only the <c>resource_name</c> path parameter - so this library does not invent
    ///         one. The call is therefore useful for exercising the endpoint, but an editor that needs to
    ///         upload actual setting content must go through <see cref="IGitLabApiConnection" /> with a
    ///         body of its own until the spec describes one.
    ///     </para>
    /// </summary>
    /// <param name="resourceName">Which settings resource to write.</param>
    /// <param name="settingsContextHash">
    ///     The settings context to scope to, or <see langword="null" /> for the user-global collection.
    /// </param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task CreateOrUpdateAsync(GitLabVsCodeSettingResource resourceName, string? settingsContextHash = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes every synced settings resource belonging to the authenticated user - the "turn off and
    ///     reset sync" operation, not a per-resource delete. There is no per-resource or per-revision
    ///     delete in this API area.
    /// </summary>
    /// <param name="settingsContextHash">
    ///     The settings context to clear, or <see langword="null" /> to clear the user-global collection.
    /// </param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task DeleteCollectionAsync(string? settingsContextHash = null, CancellationToken cancellationToken = default);
}