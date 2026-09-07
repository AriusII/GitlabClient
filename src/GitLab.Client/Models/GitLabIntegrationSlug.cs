namespace GitLab.Client.Models;

/// <summary>
///     The slugs GitLab addresses its integrations by, as they appear in
///     <c>/projects/:id/integrations/:slug</c> and <c>/groups/:id/integrations/:slug</c>.
///     <para>
///         These are constants rather than an enum on purpose, for the same reason as
///         <see cref="GitLabTokenScopes" />: GitLab adds integrations in minor releases, and the pinned
///         spec already disagrees with itself about the vocabulary (the <c>slug</c> path parameter
///         enumerates only the first twelve, while the per-integration routes enumerate all 51). A closed
///         enum would make an integration GitLab shipped last month unreachable through this library;
///         constants keep the endpoint callable with a plain string while still removing the literals
///         from calling code.
///     </para>
///     <para>
///         Note the spelling: slugs are hyphenated (<c>mattermost-slash-commands</c>), but the one
///         non-CRUD route in this area - the Mattermost slash-command trigger - uses the underscored
///         form <c>mattermost_slash_commands</c> in its path. That is GitLab's inconsistency, not a typo;
///         the trigger has its own method so a caller never has to know.
///     </para>
/// </summary>
public static class GitLabIntegrationSlug
{
    /// <summary>Apple App Store - upload build metadata for iOS releases.</summary>
    public const string AppleAppStore = "apple-app-store";

    /// <summary>Asana - add commit messages as comments on Asana tasks.</summary>
    public const string Asana = "asana";

    /// <summary>Assembla - push notifications to an Assembla space.</summary>
    public const string Assembla = "assembla";

    /// <summary>Atlassian Bamboo - run a Bamboo build plan on push.</summary>
    public const string Bamboo = "bamboo";

    /// <summary>Bugzilla - use Bugzilla as the project's external issue tracker.</summary>
    public const string Bugzilla = "bugzilla";

    /// <summary>Buildkite - run Buildkite pipelines as the project's CI.</summary>
    public const string Buildkite = "buildkite";

    /// <summary>Campfire - push notifications to a Campfire room.</summary>
    public const string Campfire = "campfire";

    /// <summary>ClickUp - use ClickUp as the project's external issue tracker.</summary>
    public const string ClickUp = "clickup";

    /// <summary>Confluence Workspace - replace the project wiki link with a Confluence space.</summary>
    public const string Confluence = "confluence";

    /// <summary>A custom external issue tracker addressed by URL templates.</summary>
    public const string CustomIssueTracker = "custom-issue-tracker";

    /// <summary>Datadog - send pipeline and job events to Datadog CI Visibility.</summary>
    public const string Datadog = "datadog";

    /// <summary>Diffblue Cover - automated Java unit-test generation.</summary>
    public const string DiffblueCover = "diffblue-cover";

    /// <summary>Discord Notifications - post events to a Discord channel webhook.</summary>
    public const string Discord = "discord";

    /// <summary>Drone - run Drone CI builds as the project's CI.</summary>
    public const string DroneCi = "drone-ci";

    /// <summary>Emails on push - mail a commit diff to a list of recipients.</summary>
    public const string EmailsOnPush = "emails-on-push";

    /// <summary>IBM Engineering Workflow Management - use EWM as the external issue tracker.</summary>
    public const string Ewm = "ewm";

    /// <summary>External wiki - replace the project wiki link with an external URL.</summary>
    public const string ExternalWiki = "external-wiki";

    /// <summary>GitGuardian - block pushes that contain leaked secrets.</summary>
    public const string GitGuardian = "git-guardian";

    /// <summary>GitHub - mirror pipeline status back to a GitHub repository.</summary>
    public const string GitHub = "github";

    /// <summary>The GitLab for Slack app - the OAuth-based successor to the Slack webhook integration.</summary>
    public const string GitLabSlackApplication = "gitlab-slack-application";

    /// <summary>Google Artifact Registry.</summary>
    public const string GoogleCloudPlatformArtifactRegistry = "google-cloud-platform-artifact-registry";

    /// <summary>Google Cloud Identity and Access Management - keyless authentication to Google Cloud.</summary>
    public const string GoogleCloudPlatformWorkloadIdentityFederation =
        "google-cloud-platform-workload-identity-federation";

    /// <summary>Google Play - upload build artifacts to the Google Play console.</summary>
    public const string GooglePlay = "google-play";

    /// <summary>Google Chat - post events to a Google Chat space.</summary>
    public const string HangoutsChat = "hangouts-chat";

    /// <summary>Harbor - use a Harbor registry for the project's container images.</summary>
    public const string Harbor = "harbor";

    /// <summary>irker (IRC gateway) - relay events to IRC channels.</summary>
    public const string Irker = "irker";

    /// <summary>Jenkins - trigger a Jenkins job on push and merge request events.</summary>
    public const string Jenkins = "jenkins";

    /// <summary>Jira - cross-link commits and merge requests with Jira issues.</summary>
    public const string Jira = "jira";

    /// <summary>The GitLab for Jira Cloud app.</summary>
    public const string JiraCloudApp = "jira-cloud-app";

    /// <summary>Linear - use Linear as the project's external issue tracker.</summary>
    public const string Linear = "linear";

    /// <summary>Matrix - post events to a Matrix room.</summary>
    public const string Matrix = "matrix";

    /// <summary>Mattermost notifications - post events to a Mattermost channel webhook.</summary>
    public const string Mattermost = "mattermost";

    /// <summary>Mattermost slash commands - run GitLab commands from Mattermost.</summary>
    public const string MattermostSlashCommands = "mattermost-slash-commands";

    /// <summary>Microsoft Teams notifications - post events to a Teams webhook.</summary>
    public const string MicrosoftTeams = "microsoft-teams";

    /// <summary>Mock CI - a development-only stand-in for a CI integration.</summary>
    public const string MockCi = "mock-ci";

    /// <summary>Mock monitoring - a development-only stand-in for a monitoring integration.</summary>
    public const string MockMonitoring = "mock-monitoring";

    /// <summary>Packagist - notify Packagist when a PHP package is pushed.</summary>
    public const string Packagist = "packagist";

    /// <summary>Phorge - use Phorge as the project's external issue tracker.</summary>
    public const string Phorge = "phorge";

    /// <summary>Pipeline status emails - mail pipeline results to a list of recipients.</summary>
    public const string PipelinesEmail = "pipelines-email";

    /// <summary>Pivotal Tracker - add commit messages as comments on Tracker stories.</summary>
    public const string PivotalTracker = "pivotaltracker";

    /// <summary>Pumble - post events to a Pumble channel.</summary>
    public const string Pumble = "pumble";

    /// <summary>Pushover - push GitLab events to a mobile device.</summary>
    public const string Pushover = "pushover";

    /// <summary>Redmine - use Redmine as the project's external issue tracker.</summary>
    public const string Redmine = "redmine";

    /// <summary>Slack notifications - post events to a Slack incoming webhook.</summary>
    public const string Slack = "slack";

    /// <summary>Squash TM - synchronise requirements with Squash Test Management.</summary>
    public const string SquashTm = "squash-tm";

    /// <summary>JetBrains TeamCity - run TeamCity builds as the project's CI.</summary>
    public const string TeamCity = "teamcity";

    /// <summary>Telegram - post events to a Telegram chat.</summary>
    public const string Telegram = "telegram";

    /// <summary>Unify Circuit - post events to a Unify Circuit conversation.</summary>
    public const string UnifyCircuit = "unify-circuit";

    /// <summary>Webex Teams - post events to a Webex space.</summary>
    public const string WebexTeams = "webex-teams";

    /// <summary>YouTrack - use YouTrack as the project's external issue tracker.</summary>
    public const string YouTrack = "youtrack";

    /// <summary>ZenTao - use ZenTao as the project's external issue tracker.</summary>
    public const string Zentao = "zentao";
}