using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Integrations" API area (<c>/projects/:id/integrations</c> and
///     <c>/groups/:id/integrations</c>) - the third-party services a project or group is wired up to:
///     Slack, Jira, Jenkins, Datadog, and the ~50 others GitLab ships.
///     <para>
///         The surface is generic rather than one method per integration, because the API is: every one
///         of the 51 slugs answers the same <c>GET</c> / <c>PUT</c> / <c>DELETE</c> triple at the same
///         route, differing only in the settings object carried in the body. Address an integration by
///         slug (<see cref="GitLabIntegrationSlug" />) and pass its settings either as a typed record
///         with its own <see cref="JsonTypeInfo{T}" />, or - for an integration this library has no type
///         for yet - as a plain dictionary. Both reach the same endpoint.
///     </para>
///     <para>
///         GitLab still serves the project half of this area under its former name,
///         <c>/projects/:id/services/...</c>: the same slugs, verbs and schemas, ~55 duplicate operations
///         in the spec. That alias is deliberately not wrapped - it is this API under an abandoned name,
///         and the spec does not flag it deprecated only because most GitLab deprecations are prose-only.
///     </para>
///     <para>
///         Two things to know before writing settings. A <c>PUT</c> is a full replace of the settings
///         object, not a merge, so omitting a member clears it. And GitLab masks credentials out of the
///         settings it returns, so reading an integration and writing the result straight back would
///         blank its token or password.
///     </para>
/// </summary>
public partial interface IIntegrationsClient
{
    /// <summary>Streams the integrations that are active on a project.</summary>
    IAsyncEnumerable<GitLabIntegration> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>Streams the integrations that are active on a group.</summary>
    IAsyncEnumerable<GitLabIntegration> ListForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets one project integration's settings by slug - the only call that populates
    ///     <see cref="GitLabIntegration.Properties" />. Throws
    ///     <see cref="Exceptions.GitLabNotFoundException" /> when the integration has never been
    ///     configured on the project.
    /// </summary>
    Task<GitLabIntegration> GetAsync(ProjectId projectId, string slug,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one group integration's settings by slug.</summary>
    Task<GitLabIntegration> GetForGroupAsync(GroupId groupId, string slug,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates or replaces a project integration's settings from a raw property bag - the escape
    ///     hatch for an integration this library has no typed settings record for. Keys are GitLab's own
    ///     snake_case parameter names and are sent verbatim.
    /// </summary>
    Task<GitLabIntegration> SetAsync(ProjectId projectId, string slug,
        IReadOnlyDictionary<string, JsonElement> settings, CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces a group integration's settings from a raw property bag.</summary>
    Task<GitLabIntegration> SetForGroupAsync(GroupId groupId, string slug,
        IReadOnlyDictionary<string, JsonElement> settings, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates or replaces a project integration's settings from a typed record, serialized through
    ///     the caller's own source-generated <paramref name="settingsTypeInfo" /> - never reflection, so
    ///     this stays Native-AOT and trim clean. The typed per-slug setters on this client are thin
    ///     wrappers over this method.
    /// </summary>
    Task<GitLabIntegration> SetAsync<TSettings>(ProjectId projectId, string slug, TSettings settings,
        JsonTypeInfo<TSettings> settingsTypeInfo, CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces a group integration's settings from a typed record.</summary>
    Task<GitLabIntegration> SetForGroupAsync<TSettings>(GroupId groupId, string slug, TSettings settings,
        JsonTypeInfo<TSettings> settingsTypeInfo, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Disables a project integration and discards its settings. GitLab's own name for the operation;
    ///     it is a <c>DELETE</c>, and re-enabling means configuring the integration again.
    /// </summary>
    Task DisableAsync(ProjectId projectId, string slug, CancellationToken cancellationToken = default);

    /// <summary>Disables a group integration and discards its settings.</summary>
    Task DisableForGroupAsync(GroupId groupId, string slug, CancellationToken cancellationToken = default);
}