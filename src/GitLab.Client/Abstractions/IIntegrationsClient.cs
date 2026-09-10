using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

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
public interface IIntegrationsClient
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

    // Typed setters are deliberately kept on this direct endpoint abstraction: no service/repository layer
    // is needed between a caller and the matching GitLab integration route.
    /// <summary>Creates or replaces the Apple App Store settings of a project.</summary>
    Task<GitLabIntegration> SetAppleAppStoreAsync(ProjectId projectId, AppleAppStoreIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the Asana settings of a project.</summary>
    Task<GitLabIntegration> SetAsanaAsync(ProjectId projectId, AsanaIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the Assembla settings of a project.</summary>
    Task<GitLabIntegration> SetAssemblaAsync(ProjectId projectId, AssemblaIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the Bamboo settings of a project.</summary>
    Task<GitLabIntegration> SetBambooAsync(ProjectId projectId, BambooIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the Bugzilla settings of a project.</summary>
    Task<GitLabIntegration> SetBugzillaAsync(ProjectId projectId, BugzillaIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the Buildkite settings of a project.</summary>
    Task<GitLabIntegration> SetBuildkiteAsync(ProjectId projectId, BuildkiteIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the Campfire settings of a project.</summary>
    Task<GitLabIntegration> SetCampfireAsync(ProjectId projectId, CampfireIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the ClickUp settings of a project.</summary>
    Task<GitLabIntegration> SetClickUpAsync(ProjectId projectId, ClickUpIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the Confluence settings of a project.</summary>
    Task<GitLabIntegration> SetConfluenceAsync(ProjectId projectId, ConfluenceIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the custom issue-tracker settings of a project.</summary>
    Task<GitLabIntegration> SetCustomIssueTrackerAsync(ProjectId projectId,
        CustomIssueTrackerIntegrationRequest settings, CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the Datadog settings of a project.</summary>
    Task<GitLabIntegration> SetDatadogAsync(ProjectId projectId, DatadogIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the Diffblue Cover settings of a project.</summary>
    Task<GitLabIntegration> SetDiffblueCoverAsync(ProjectId projectId, DiffblueCoverIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the Discord settings of a project.</summary>
    Task<GitLabIntegration> SetDiscordAsync(ProjectId projectId, DiscordIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the Drone CI settings of a project.</summary>
    Task<GitLabIntegration> SetDroneCiAsync(ProjectId projectId, DroneCiIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the Emails on Push settings of a project.</summary>
    Task<GitLabIntegration> SetEmailsOnPushAsync(ProjectId projectId, EmailsOnPushIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the IBM EWM settings of a project.</summary>
    Task<GitLabIntegration> SetEwmAsync(ProjectId projectId, EwmIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the external wiki settings of a project.</summary>
    Task<GitLabIntegration> SetExternalWikiAsync(ProjectId projectId, ExternalWikiIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the GitGuardian settings of a project.</summary>
    Task<GitLabIntegration> SetGitGuardianAsync(ProjectId projectId, GitGuardianIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the GitHub settings of a project.</summary>
    Task<GitLabIntegration> SetGitHubAsync(ProjectId projectId, GitHubIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the GitLab for Slack application settings of a project.</summary>
    Task<GitLabIntegration> SetGitLabSlackApplicationAsync(ProjectId projectId,
        GitLabSlackApplicationIntegrationRequest settings, CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the Google Cloud Artifact Registry settings of a project.</summary>
    Task<GitLabIntegration> SetGoogleCloudPlatformArtifactRegistryAsync(ProjectId projectId,
        GoogleCloudPlatformArtifactRegistryIntegrationRequest settings, CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the Google Cloud workload identity federation settings of a project.</summary>
    Task<GitLabIntegration> SetGoogleCloudPlatformWorkloadIdentityFederationAsync(ProjectId projectId,
        GoogleCloudPlatformWorkloadIdentityFederationIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the Google Play settings of a project.</summary>
    Task<GitLabIntegration> SetGooglePlayAsync(ProjectId projectId, GooglePlayIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the Hangouts Chat settings of a project.</summary>
    Task<GitLabIntegration> SetHangoutsChatAsync(ProjectId projectId, HangoutsChatIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the Harbor settings of a project.</summary>
    Task<GitLabIntegration> SetHarborAsync(ProjectId projectId, HarborIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the Irker settings of a project.</summary>
    Task<GitLabIntegration> SetIrkerAsync(ProjectId projectId, IrkerIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the Jenkins settings of a project.</summary>
    Task<GitLabIntegration> SetJenkinsAsync(ProjectId projectId, JenkinsIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the Jira settings of a project.</summary>
    Task<GitLabIntegration> SetJiraAsync(ProjectId projectId, JiraIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the Jira Cloud app settings of a project.</summary>
    Task<GitLabIntegration> SetJiraCloudAppAsync(ProjectId projectId, JiraCloudAppIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the Linear settings of a project.</summary>
    Task<GitLabIntegration> SetLinearAsync(ProjectId projectId, LinearIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the Matrix settings of a project.</summary>
    Task<GitLabIntegration> SetMatrixAsync(ProjectId projectId, MatrixIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the Mattermost notification settings of a project.</summary>
    Task<GitLabIntegration> SetMattermostAsync(ProjectId projectId, MattermostIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the Mattermost slash-command settings of a project.</summary>
    Task<GitLabIntegration> SetMattermostSlashCommandsAsync(ProjectId projectId,
        MattermostSlashCommandsIntegrationRequest settings, CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the Microsoft Teams settings of a project.</summary>
    Task<GitLabIntegration> SetMicrosoftTeamsAsync(ProjectId projectId, MicrosoftTeamsIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the Mock CI settings of a project.</summary>
    Task<GitLabIntegration> SetMockCiAsync(ProjectId projectId, MockCiIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the Mock monitoring settings of a project.</summary>
    Task<GitLabIntegration> SetMockMonitoringAsync(ProjectId projectId, MockMonitoringIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the Packagist settings of a project.</summary>
    Task<GitLabIntegration> SetPackagistAsync(ProjectId projectId, PackagistIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the Phorge settings of a project.</summary>
    Task<GitLabIntegration> SetPhorgeAsync(ProjectId projectId, PhorgeIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the pipeline-status email settings of a project.</summary>
    Task<GitLabIntegration> SetPipelinesEmailAsync(ProjectId projectId, PipelinesEmailIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the Pivotal Tracker settings of a project.</summary>
    Task<GitLabIntegration> SetPivotalTrackerAsync(ProjectId projectId, PivotalTrackerIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the Pumble settings of a project.</summary>
    Task<GitLabIntegration> SetPumbleAsync(ProjectId projectId, PumbleIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the Pushover settings of a project.</summary>
    Task<GitLabIntegration> SetPushoverAsync(ProjectId projectId, PushoverIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the Redmine settings of a project.</summary>
    Task<GitLabIntegration> SetRedmineAsync(ProjectId projectId, RedmineIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the Slack settings of a project.</summary>
    Task<GitLabIntegration> SetSlackAsync(ProjectId projectId, SlackIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the Squash TM settings of a project.</summary>
    Task<GitLabIntegration> SetSquashTmAsync(ProjectId projectId, SquashTmIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the TeamCity settings of a project.</summary>
    Task<GitLabIntegration> SetTeamCityAsync(ProjectId projectId, TeamCityIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the Telegram settings of a project.</summary>
    Task<GitLabIntegration> SetTelegramAsync(ProjectId projectId, TelegramIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the Unify Circuit settings of a project.</summary>
    Task<GitLabIntegration> SetUnifyCircuitAsync(ProjectId projectId, UnifyCircuitIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the Webex Teams settings of a project.</summary>
    Task<GitLabIntegration> SetWebexTeamsAsync(ProjectId projectId, WebexTeamsIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the YouTrack settings of a project.</summary>
    Task<GitLabIntegration> SetYouTrackAsync(ProjectId projectId, YouTrackIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the ZenTao settings of a project.</summary>
    Task<GitLabIntegration> SetZentaoAsync(ProjectId projectId, ZentaoIntegrationRequest settings,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a project integration through GitLab's obsolete <c>/services</c> alias.</summary>
    [Obsolete("Use the modern /integrations route instead (GetAsync).")]
    Task<GitLabIntegration> GetServiceAsync(ProjectId projectId, string slug,
        CancellationToken cancellationToken = default);

    /// <summary>Disables a project integration through GitLab's obsolete <c>/services</c> alias.</summary>
    [Obsolete("Use the modern /integrations route instead (DisableAsync).")]
    Task DisableServiceAsync(ProjectId projectId, string slug, CancellationToken cancellationToken = default);

    /// <summary>Streams project integrations through GitLab's obsolete <c>/services</c> alias.</summary>
    [Obsolete("Use the modern /integrations route instead (ListAsync).")]
    IAsyncEnumerable<GitLabIntegration> ListServicesAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>Processes an incoming Slack Events API callback.</summary>
    Task ReceiveSlackEventAsync(SlackEventRequest request, CancellationToken cancellationToken = default);

    /// <summary>Processes an incoming Slack interaction callback.</summary>
    Task ProcessSlackInteractionAsync(CancellationToken cancellationToken = default);

    /// <summary>Processes an incoming Slack options callback.</summary>
    Task ProcessSlackOptionsAsync(CancellationToken cancellationToken = default);

    /// <summary>Triggers a GitLab command issued from Slack.</summary>
    Task TriggerSlackCommandAsync(TriggerSlackCommandRequest request, CancellationToken cancellationToken = default);

    /// <summary>Triggers a GitLab command issued from a project's Mattermost integration.</summary>
    Task TriggerMattermostSlashCommandAsync(ProjectId projectId, TriggerMattermostSlashCommandRequest request,
        CancellationToken cancellationToken = default);
}