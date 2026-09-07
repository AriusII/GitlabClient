using System.Text.Json;

using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Features" API area (<c>/features</c>) - the instance-wide Flipper flags GitLab
///     itself is developed behind - together with the Unleash endpoints
///     (<c>/feature_flags/unleash/:project_id</c>) that serve a project's flags to an Unleash client.
///     <para>
///         The <c>/features</c> half is administrator-only and instance-wide: it toggles GitLab's own
///         development flags, not a project's. A project's feature flags live on
///         <see cref="IFeatureFlagsClient" />.
///     </para>
///     <para>
///         The Unleash half is normally spoken by an Unleash SDK rather than by hand, and authenticates
///         with the project's Unleash instance ID instead of a personal access token; it is exposed here
///         for tooling that needs to see exactly what a client would be served.
///     </para>
/// </summary>
public interface IFeaturesClient
{
    /// <summary>Streams every instance feature and the gates currently set on it.</summary>
    IAsyncEnumerable<GitLabFeature> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the YAML definitions the instance ships for its own flags - owner, type, milestone and
    ///     rollout issue. This is the catalogue; <see cref="ListAsync" /> is the current state.
    /// </summary>
    IAsyncEnumerable<GitLabFeatureDefinition> ListDefinitionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates the feature if it does not exist yet and sets one gate on it. Build the request with
    ///     <see cref="SetFeatureRequest.Enable" />, <see cref="SetFeatureRequest.Disable" /> or
    ///     <see cref="SetFeatureRequest.ForPercentage" />.
    /// </summary>
    Task<GitLabFeature> SetAsync(string name, SetFeatureRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a feature gate. GitLab answers the same way whether or not the gate existed, so this
    ///     never reports a missing feature.
    /// </summary>
    Task DeleteAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the project's flags as an Unleash client would see them
    ///     (<c>GET /feature_flags/unleash/:project_id</c>).
    ///     <para>
    ///         The spec declares no response schema - the body is Unleash's own client-features document,
    ///         versioned by Unleash rather than by GitLab - so it is returned as a raw
    ///         <see cref="JsonElement" /> instead of a shape this library would be guessing at.
    ///     </para>
    /// </summary>
    Task<JsonElement> GetUnleashFeaturesAsync(ProjectId projectId, UnleashClientOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     The <c>/client/features</c> form of <see cref="GetUnleashFeaturesAsync" />, which is the path
    ///     current Unleash SDKs poll. Returns a raw <see cref="JsonElement" /> for the same reason.
    /// </summary>
    Task<JsonElement> GetUnleashClientFeaturesAsync(ProjectId projectId, UnleashClientOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Registers an Unleash client against the project so GitLab can list it as connected.</summary>
    Task RegisterUnleashClientAsync(ProjectId projectId, UnleashClientRegistrationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Reports an Unleash client's usage counters back to GitLab.</summary>
    Task ReportUnleashMetricsAsync(ProjectId projectId, UnleashClientRegistrationRequest request,
        CancellationToken cancellationToken = default);
}