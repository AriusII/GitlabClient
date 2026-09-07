using System.Diagnostics.CodeAnalysis;

namespace GitLab.Client.Models;

/// <summary>
///     The current instance-wide application settings, as returned by both
///     <c>GET /application/settings</c> and <c>PUT /application/settings</c>. Administrators only.
///     <para>
///         GitLab's own OpenAPI document types nearly every member of this response as a bare
///         <c>string</c>, including fields that are booleans or integers on the request side
///         (<see cref="UpdateApplicationSettingsRequest" />) and in the rendered API docs - a known gap
///         in GitLab's spec generation for this large, dynamically-assembled entity, not a modeling
///         choice made here. Every member is therefore kept as <see cref="string" /> to match the
///         spec exactly rather than guess at a richer type per field: a wrong guess would turn a
///         healthy response into a <see cref="System.Text.Json.JsonException" />, where a string
///         merely asks the caller to parse it. Update requests should go through the correctly-typed
///         <see cref="UpdateApplicationSettingsRequest" /> instead of round-tripping this type.
///     </para>
/// </summary>
public sealed record GitLabApplicationSettings
{
    public string? Id { get; init; }

    public string? PerformanceBarAllowedGroupId { get; init; }

    public string? AbuseNotificationEmail { get; init; }

    public string? AdminMode { get; init; }

    public string? AfterSignOutPath { get; init; }

    public string? AfterSignUpText { get; init; }

    public string? AkismetApiKey { get; init; }

    public string? AkismetEnabled { get; init; }

    public string? AllowLocalRequestsFromWebHooksAndServices { get; init; }

    public string? AllowLocalRequestsFromSystemHooks { get; init; }

    public string? AllowPossibleSpam { get; init; }

    public string? DnsRebindingProtectionEnabled { get; init; }

    public string? ArchiveBuildsInHumanReadable { get; init; }

    public string? AssetProxyEnabled { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "Every member of this response is kept as string; see the type-level remarks.")]
    public string? AssetProxyUrl { get; init; }

    public string? AssetProxyAllowlist { get; init; }

    public string? StaticObjectsExternalStorageAuthToken { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "Every member of this response is kept as string; see the type-level remarks.")]
    public string? StaticObjectsExternalStorageUrl { get; init; }

    public string? AuthnDataRetentionCleanupEnabled { get; init; }

    public string? AuthorizedKeysEnabled { get; init; }

    public string? AutoDevopsEnabled { get; init; }

    public string? AutoDevopsDomain { get; init; }

    public string? AutocompleteUsersLimit { get; init; }

    public string? AutocompleteUsersUnauthenticatedLimit { get; init; }

    public string? AllowBypassPlaceholderConfirmation { get; init; }

    public string? CiDeletePipelinesInSecondsLimitHumanReadable { get; init; }

    public string? CiJobLiveTraceEnabled { get; init; }

    public string? CiPartitionsInSecondsLimitHumanReadable { get; init; }

    public string? ConcurrentGithubImportJobsLimit { get; init; }

    public string? ConcurrentBitbucketImportJobsLimit { get; init; }

    public string? ConcurrentBitbucketServerImportJobsLimit { get; init; }

    public string? ConcurrentPullRequestImportJobsLimit { get; init; }

    public string? ImportJobsConcurrencyLimit { get; init; }

    public string? ContainerExpirationPoliciesEnableHistoricEntries { get; init; }

    public string? ContainerRegistryExpirationPoliciesCaching { get; init; }

    public string? ContainerRegistryTokenExpireDelay { get; init; }

    public string? OauthAccessTokenExpiresIn { get; init; }

    public string? DecompressArchiveFileTimeout { get; init; }

    public string? DefaultArtifactsExpireIn { get; init; }

    public string? DefaultBranchName { get; init; }

    public string? DefaultBranchProtection { get; init; }

    public string? DefaultBranchProtectionDefaults { get; init; }

    public string? DefaultCiConfigPath { get; init; }

    public string? DefaultGroupVisibility { get; init; }

    public string? DefaultPreferredLanguage { get; init; }

    public string? DefaultProjectCreation { get; init; }

    public string? DefaultProjectVisibility { get; init; }

    public string? DefaultProjectsLimit { get; init; }

    public string? DefaultSnippetVisibility { get; init; }

    public string? DefaultSyntaxHighlightingTheme { get; init; }
}