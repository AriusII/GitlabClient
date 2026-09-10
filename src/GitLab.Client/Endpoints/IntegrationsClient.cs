using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class IntegrationsClient(IGitLabApiConnection connection) : IIntegrationsClient
{
    /// <summary>
    ///     The one fixed path word this whole resource is built on. GitLab also serves every project
    ///     route below under the older <c>/services</c> spelling - byte-identical slugs, verbs and
    ///     schemas - and that alias is deliberately not wrapped: it is the same API under an
    ///     abandoned name, and wrapping it would double the surface for nothing.
    /// </summary>
    private const string Integrations = "integrations";

    private const string Services = "services";

    public IAsyncEnumerable<GitLabIntegration> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal(Integrations).Build(),
            GitLabJsonContext.Default.GitLabIntegrationArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabIntegration> ListForGroupAsync(GroupId groupId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal(Integrations).Build(),
            GitLabJsonContext.Default.GitLabIntegrationArray,
            cancellationToken);
    }

    public Task<GitLabIntegration> GetAsync(ProjectId projectId, string slug,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            ProjectIntegrationRoute(projectId, slug),
            GitLabJsonContext.Default.GitLabIntegration,
            cancellationToken);
    }

    public Task<GitLabIntegration> GetForGroupAsync(GroupId groupId, string slug,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GroupIntegrationRoute(groupId, slug),
            GitLabJsonContext.Default.GitLabIntegration,
            cancellationToken);
    }

    public Task<GitLabIntegration> SetAsync(ProjectId projectId, string slug,
        IReadOnlyDictionary<string, JsonElement> settings, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            ProjectIntegrationRoute(projectId, slug),
            settings,
            GitLabJsonContext.Default.IntegrationSettings,
            GitLabJsonContext.Default.GitLabIntegration,
            cancellationToken);
    }

    public Task<GitLabIntegration> SetForGroupAsync(GroupId groupId, string slug,
        IReadOnlyDictionary<string, JsonElement> settings, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GroupIntegrationRoute(groupId, slug),
            settings,
            GitLabJsonContext.Default.IntegrationSettings,
            GitLabJsonContext.Default.GitLabIntegration,
            cancellationToken);
    }

    public Task<GitLabIntegration> SetAsync<TSettings>(ProjectId projectId, string slug, TSettings settings,
        JsonTypeInfo<TSettings> settingsTypeInfo, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            ProjectIntegrationRoute(projectId, slug),
            settings,
            settingsTypeInfo,
            GitLabJsonContext.Default.GitLabIntegration,
            cancellationToken);
    }

    public Task<GitLabIntegration> SetForGroupAsync<TSettings>(GroupId groupId, string slug, TSettings settings,
        JsonTypeInfo<TSettings> settingsTypeInfo, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GroupIntegrationRoute(groupId, slug),
            settings,
            settingsTypeInfo,
            GitLabJsonContext.Default.GitLabIntegration,
            cancellationToken);
    }

    public Task DisableAsync(ProjectId projectId, string slug, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(ProjectIntegrationRoute(projectId, slug), cancellationToken);
    }

    public Task DisableForGroupAsync(GroupId groupId, string slug, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(GroupIntegrationRoute(groupId, slug), cancellationToken);
    }

    public Task<GitLabIntegration> SetAppleAppStoreAsync(ProjectId id, AppleAppStoreIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.AppleAppStore, settings,
            GitLabJsonContext.Default.AppleAppStoreIntegrationRequest, ct);
    }

    public Task<GitLabIntegration> SetAsanaAsync(ProjectId id, AsanaIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.Asana, settings, GitLabJsonContext.Default.AsanaIntegrationRequest,
            ct);
    }

    public Task<GitLabIntegration> SetAssemblaAsync(ProjectId id, AssemblaIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.Assembla, settings,
            GitLabJsonContext.Default.AssemblaIntegrationRequest, ct);
    }

    public Task<GitLabIntegration> SetBambooAsync(ProjectId id, BambooIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.Bamboo, settings, GitLabJsonContext.Default.BambooIntegrationRequest,
            ct);
    }

    public Task<GitLabIntegration> SetBugzillaAsync(ProjectId id, BugzillaIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.Bugzilla, settings,
            GitLabJsonContext.Default.BugzillaIntegrationRequest, ct);
    }

    public Task<GitLabIntegration> SetBuildkiteAsync(ProjectId id, BuildkiteIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.Buildkite, settings,
            GitLabJsonContext.Default.BuildkiteIntegrationRequest, ct);
    }

    public Task<GitLabIntegration> SetCampfireAsync(ProjectId id, CampfireIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.Campfire, settings,
            GitLabJsonContext.Default.CampfireIntegrationRequest, ct);
    }

    public Task<GitLabIntegration> SetClickUpAsync(ProjectId id, ClickUpIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.ClickUp, settings,
            GitLabJsonContext.Default.ClickUpIntegrationRequest,
            ct);
    }

    public Task<GitLabIntegration> SetConfluenceAsync(ProjectId id, ConfluenceIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.Confluence, settings,
            GitLabJsonContext.Default.ConfluenceIntegrationRequest, ct);
    }

    public Task<GitLabIntegration> SetCustomIssueTrackerAsync(ProjectId id,
        CustomIssueTrackerIntegrationRequest settings, CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.CustomIssueTracker, settings,
            GitLabJsonContext.Default.CustomIssueTrackerIntegrationRequest, ct);
    }

    public Task<GitLabIntegration> SetDatadogAsync(ProjectId id, DatadogIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.Datadog, settings,
            GitLabJsonContext.Default.DatadogIntegrationRequest, ct);
    }

    public Task<GitLabIntegration> SetDiffblueCoverAsync(ProjectId id, DiffblueCoverIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.DiffblueCover, settings,
            GitLabJsonContext.Default.DiffblueCoverIntegrationRequest, ct);
    }

    public Task<GitLabIntegration> SetDiscordAsync(ProjectId id, DiscordIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.Discord, settings,
            GitLabJsonContext.Default.DiscordIntegrationRequest, ct);
    }

    public Task<GitLabIntegration> SetDroneCiAsync(ProjectId id, DroneCiIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.DroneCi, settings,
            GitLabJsonContext.Default.DroneCiIntegrationRequest, ct);
    }

    public Task<GitLabIntegration> SetEmailsOnPushAsync(ProjectId id, EmailsOnPushIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.EmailsOnPush, settings,
            GitLabJsonContext.Default.EmailsOnPushIntegrationRequest, ct);
    }

    public Task<GitLabIntegration> SetEwmAsync(ProjectId id, EwmIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.Ewm, settings, GitLabJsonContext.Default.EwmIntegrationRequest, ct);
    }

    public Task<GitLabIntegration> SetExternalWikiAsync(ProjectId id, ExternalWikiIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.ExternalWiki, settings,
            GitLabJsonContext.Default.ExternalWikiIntegrationRequest, ct);
    }

    public Task<GitLabIntegration> SetGitGuardianAsync(ProjectId id, GitGuardianIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.GitGuardian, settings,
            GitLabJsonContext.Default.GitGuardianIntegrationRequest, ct);
    }

    public Task<GitLabIntegration> SetGitHubAsync(ProjectId id, GitHubIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.GitHub, settings, GitLabJsonContext.Default.GitHubIntegrationRequest,
            ct);
    }

    public Task<GitLabIntegration> SetGitLabSlackApplicationAsync(ProjectId id,
        GitLabSlackApplicationIntegrationRequest settings, CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.GitLabSlackApplication, settings,
            GitLabJsonContext.Default.GitLabSlackApplicationIntegrationRequest, ct);
    }

    public Task<GitLabIntegration> SetGoogleCloudPlatformArtifactRegistryAsync(ProjectId id,
        GoogleCloudPlatformArtifactRegistryIntegrationRequest settings, CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.GoogleCloudPlatformArtifactRegistry, settings,
            GitLabJsonContext.Default.GoogleCloudPlatformArtifactRegistryIntegrationRequest, ct);
    }

    public Task<GitLabIntegration> SetGoogleCloudPlatformWorkloadIdentityFederationAsync(ProjectId id,
        GoogleCloudPlatformWorkloadIdentityFederationIntegrationRequest settings, CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.GoogleCloudPlatformWorkloadIdentityFederation, settings,
            GitLabJsonContext.Default.GoogleCloudPlatformWorkloadIdentityFederationIntegrationRequest, ct);
    }

    public Task<GitLabIntegration> SetGooglePlayAsync(ProjectId id, GooglePlayIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.GooglePlay, settings,
            GitLabJsonContext.Default.GooglePlayIntegrationRequest, ct);
    }

    public Task<GitLabIntegration> SetHangoutsChatAsync(ProjectId id, HangoutsChatIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.HangoutsChat, settings,
            GitLabJsonContext.Default.HangoutsChatIntegrationRequest, ct);
    }

    public Task<GitLabIntegration> SetHarborAsync(ProjectId id, HarborIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.Harbor, settings, GitLabJsonContext.Default.HarborIntegrationRequest,
            ct);
    }

    public Task<GitLabIntegration> SetIrkerAsync(ProjectId id, IrkerIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.Irker, settings, GitLabJsonContext.Default.IrkerIntegrationRequest,
            ct);
    }

    public Task<GitLabIntegration> SetJenkinsAsync(ProjectId id, JenkinsIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.Jenkins, settings,
            GitLabJsonContext.Default.JenkinsIntegrationRequest, ct);
    }

    public Task<GitLabIntegration> SetJiraAsync(ProjectId id, JiraIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.Jira, settings, GitLabJsonContext.Default.JiraIntegrationRequest, ct);
    }

    public Task<GitLabIntegration> SetJiraCloudAppAsync(ProjectId id, JiraCloudAppIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.JiraCloudApp, settings,
            GitLabJsonContext.Default.JiraCloudAppIntegrationRequest, ct);
    }

    public Task<GitLabIntegration> SetLinearAsync(ProjectId id, LinearIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.Linear, settings, GitLabJsonContext.Default.LinearIntegrationRequest,
            ct);
    }

    public Task<GitLabIntegration> SetMatrixAsync(ProjectId id, MatrixIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.Matrix, settings, GitLabJsonContext.Default.MatrixIntegrationRequest,
            ct);
    }

    public Task<GitLabIntegration> SetMattermostAsync(ProjectId id, MattermostIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.Mattermost, settings,
            GitLabJsonContext.Default.MattermostIntegrationRequest, ct);
    }

    public Task<GitLabIntegration> SetMattermostSlashCommandsAsync(ProjectId id,
        MattermostSlashCommandsIntegrationRequest settings, CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.MattermostSlashCommands, settings,
            GitLabJsonContext.Default.MattermostSlashCommandsIntegrationRequest, ct);
    }

    public Task<GitLabIntegration> SetMicrosoftTeamsAsync(ProjectId id, MicrosoftTeamsIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.MicrosoftTeams, settings,
            GitLabJsonContext.Default.MicrosoftTeamsIntegrationRequest, ct);
    }

    public Task<GitLabIntegration> SetMockCiAsync(ProjectId id, MockCiIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.MockCi, settings, GitLabJsonContext.Default.MockCiIntegrationRequest,
            ct);
    }

    public Task<GitLabIntegration> SetMockMonitoringAsync(ProjectId id, MockMonitoringIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.MockMonitoring, settings,
            GitLabJsonContext.Default.MockMonitoringIntegrationRequest, ct);
    }

    public Task<GitLabIntegration> SetPackagistAsync(ProjectId id, PackagistIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.Packagist, settings,
            GitLabJsonContext.Default.PackagistIntegrationRequest, ct);
    }

    public Task<GitLabIntegration> SetPhorgeAsync(ProjectId id, PhorgeIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.Phorge, settings, GitLabJsonContext.Default.PhorgeIntegrationRequest,
            ct);
    }

    public Task<GitLabIntegration> SetPipelinesEmailAsync(ProjectId id, PipelinesEmailIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.PipelinesEmail, settings,
            GitLabJsonContext.Default.PipelinesEmailIntegrationRequest, ct);
    }

    public Task<GitLabIntegration> SetPivotalTrackerAsync(ProjectId id, PivotalTrackerIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.PivotalTracker, settings,
            GitLabJsonContext.Default.PivotalTrackerIntegrationRequest, ct);
    }

    public Task<GitLabIntegration> SetPumbleAsync(ProjectId id, PumbleIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.Pumble, settings, GitLabJsonContext.Default.PumbleIntegrationRequest,
            ct);
    }

    public Task<GitLabIntegration> SetPushoverAsync(ProjectId id, PushoverIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.Pushover, settings,
            GitLabJsonContext.Default.PushoverIntegrationRequest, ct);
    }

    public Task<GitLabIntegration> SetRedmineAsync(ProjectId id, RedmineIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.Redmine, settings,
            GitLabJsonContext.Default.RedmineIntegrationRequest,
            ct);
    }

    public Task<GitLabIntegration> SetSlackAsync(ProjectId id, SlackIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.Slack, settings, GitLabJsonContext.Default.SlackIntegrationRequest,
            ct);
    }

    public Task<GitLabIntegration> SetSquashTmAsync(ProjectId id, SquashTmIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.SquashTm, settings,
            GitLabJsonContext.Default.SquashTmIntegrationRequest, ct);
    }

    public Task<GitLabIntegration> SetTeamCityAsync(ProjectId id, TeamCityIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.TeamCity, settings,
            GitLabJsonContext.Default.TeamCityIntegrationRequest, ct);
    }

    public Task<GitLabIntegration> SetTelegramAsync(ProjectId id, TelegramIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.Telegram, settings,
            GitLabJsonContext.Default.TelegramIntegrationRequest, ct);
    }

    public Task<GitLabIntegration> SetUnifyCircuitAsync(ProjectId id, UnifyCircuitIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.UnifyCircuit, settings,
            GitLabJsonContext.Default.UnifyCircuitIntegrationRequest, ct);
    }

    public Task<GitLabIntegration> SetWebexTeamsAsync(ProjectId id, WebexTeamsIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.WebexTeams, settings,
            GitLabJsonContext.Default.WebexTeamsIntegrationRequest, ct);
    }

    public Task<GitLabIntegration> SetYouTrackAsync(ProjectId id, YouTrackIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.YouTrack, settings,
            GitLabJsonContext.Default.YouTrackIntegrationRequest, ct);
    }

    public Task<GitLabIntegration> SetZentaoAsync(ProjectId id, ZentaoIntegrationRequest settings,
        CancellationToken ct = default)
    {
        return SetAsync(id, GitLabIntegrationSlug.Zentao, settings, GitLabJsonContext.Default.ZentaoIntegrationRequest,
            ct);
    }

    [Obsolete("Use the modern /integrations route instead (GetAsync).")]
    public Task<GitLabIntegration> GetServiceAsync(ProjectId projectId, string slug,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(ProjectServiceRoute(projectId, slug), GitLabJsonContext.Default.GitLabIntegration,
            cancellationToken);
    }

    [Obsolete("Use the modern /integrations route instead (DisableAsync).")]
    public Task DisableServiceAsync(ProjectId projectId, string slug, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(ProjectServiceRoute(projectId, slug), cancellationToken);
    }

    [Obsolete("Use the modern /integrations route instead (ListAsync).")]
    public IAsyncEnumerable<GitLabIntegration> ListServicesAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal(Services).Build(),
            GitLabJsonContext.Default.GitLabIntegrationArray, cancellationToken);
    }

    public Task ReceiveSlackEventAsync(SlackEventRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(GitLabRouteBuilder.Create(Integrations).Literal("slack").Literal("events").Build(),
            request, GitLabJsonContext.Default.SlackEventRequest, cancellationToken);
    }

    public Task ProcessSlackInteractionAsync(CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create(Integrations).Literal("slack").Literal("interactions").Build(),
            cancellationToken);
    }

    public Task ProcessSlackOptionsAsync(CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(GitLabRouteBuilder.Create(Integrations).Literal("slack").Literal("options").Build(),
            cancellationToken);
    }

    public Task TriggerSlackCommandAsync(TriggerSlackCommandRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(GitLabRouteBuilder.Create("slack").Literal("trigger").Build(), request,
            GitLabJsonContext.Default.TriggerSlackCommandRequest, cancellationToken);
    }

    public Task TriggerMattermostSlashCommandAsync(ProjectId projectId, TriggerMattermostSlashCommandRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal(Integrations)
                .Literal("mattermost_slash_commands").Literal("trigger").Build(), request,
            GitLabJsonContext.Default.TriggerMattermostSlashCommandRequest, cancellationToken);
    }

    /// <summary>
    ///     The slug is caller-supplied text, so it goes through <c>Escaped</c> rather than <c>Literal</c>.
    ///     Every slug GitLab ships today is made of unreserved characters and survives unchanged, which is
    ///     exactly why this is easy to get wrong: <c>Literal</c> would look correct for all 51 of them and
    ///     then silently emit a broken path for the first slug that is not, or for the typo'd string a
    ///     caller passes by hand.
    /// </summary>
    private static Uri ProjectIntegrationRoute(ProjectId projectId, string slug)
    {
        return GitLabRouteBuilder.Create("projects").Segment(projectId).Literal(Integrations).Escaped(slug).Build();
    }

    private static Uri GroupIntegrationRoute(GroupId groupId, string slug)
    {
        return GitLabRouteBuilder.Create("groups").Segment(groupId).Literal(Integrations).Escaped(slug).Build();
    }

    private static Uri ProjectServiceRoute(ProjectId projectId, string slug)
    {
        return GitLabRouteBuilder.Create("projects").Segment(projectId).Literal(Services).Escaped(slug).Build();
    }
}