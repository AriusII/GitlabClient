using GitLab.Client.Domain;

namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>PUT /application/settings</c>. Every member is nullable: unset properties
///     are omitted from the payload rather than sent as null, so an update leaves settings it does not
///     mention unchanged. Administrators only.
///     <para>
///         Covers the subset of GitLab's ~200-member application-settings surface most commonly
///         changed through automation; anything not listed here can still be reached through
///         <see cref="Abstractions.IGitLabApiConnection" /> directly.
///     </para>
/// </summary>
public sealed record UpdateApplicationSettingsRequest
{
    /// <summary>Requires admin users to re-authenticate before administrative (potentially dangerous) operations.</summary>
    public bool? AdminMode { get; init; }

    /// <summary>Deprecated: use <see cref="AbuseNotificationEmail" /> instead.</summary>
    public string? AdminNotificationEmail { get; init; }

    /// <summary>Abuse reports are sent to this address if it is set.</summary>
    public string? AbuseNotificationEmail { get; init; }

    /// <summary>Text shown after sign-up.</summary>
    public string? AfterSignUpText { get; init; }

    /// <summary>Page users are redirected to after they sign out.</summary>
    public string? AfterSignOutPath { get; init; }

    /// <summary>Helps prevent bots from creating issues.</summary>
    public bool? AkismetEnabled { get; init; }

    public string? AkismetApiKey { get; init; }

    /// <summary>Enables proxying of assets.</summary>
    public bool? AssetProxyEnabled { get; init; }

    /// <summary>URL of the asset proxy server.</summary>
    public Uri? AssetProxyUrl { get; init; }

    /// <summary>Shared secret with the asset proxy server.</summary>
    public string? AssetProxySecretKey { get; init; }

    /// <summary>Deprecated: use <see cref="AssetProxyAllowlist" /> instead.</summary>
    public IReadOnlyList<string>? AssetProxyWhitelist { get; init; }

    /// <summary>Domains that are not proxied even when <see cref="AssetProxyEnabled" /> is set.</summary>
    public IReadOnlyList<string>? AssetProxyAllowlist { get; init; }

    /// <summary>Enables authentication data retention cleanup workers.</summary>
    public bool? AuthnDataRetentionCleanupEnabled { get; init; }

    /// <summary>Container registry authorization token duration, in minutes.</summary>
    public int? ContainerRegistryTokenExpireDelay { get; init; }

    /// <summary>Lifetime of OAuth access tokens, in seconds.</summary>
    public int? OauthAccessTokenExpiresIn { get; init; }

    /// <summary>Default timeout for decompressing archived files, in seconds. <c>0</c> disables the timeout.</summary>
    public int? DecompressArchiveFileTimeout { get; init; }

    /// <summary>Default expiration time for each job's artifacts, for example <c>"30 days"</c>.</summary>
    public string? DefaultArtifactsExpireIn { get; init; }

    /// <summary>The instance default CI/CD configuration file and path for new projects.</summary>
    public string? DefaultCiConfigPath { get; init; }

    /// <summary>
    ///     Who may create projects in a group, on GitLab's numeric ladder: <c>0</c> (no one), <c>1</c>
    ///     (Maintainers), <c>2</c> (Developers and Maintainers), <c>3</c> (Administrators), <c>4</c>
    ///     (Owners).
    /// </summary>
    public int? DefaultProjectCreation { get; init; }

    /// <summary>
    ///     Who may push to the default branch, on GitLab's numeric ladder: <c>0</c> (not protected),
    ///     <c>1</c> (partially protected), <c>2</c> (fully protected), <c>3</c> (protected against
    ///     force push), <c>4</c> (full protection after initial push).
    /// </summary>
    public int? DefaultBranchProtection { get; init; }

    /// <summary>
    ///     The structured replacement for <see cref="DefaultBranchProtection" /> - who may push to and
    ///     merge into new projects' default branch.
    /// </summary>
    public GitLabBranchProtectionDefaults? DefaultBranchProtectionDefaults { get; init; }

    public GitLabVisibility? DefaultGroupVisibility { get; init; }

    public GitLabVisibility? DefaultProjectVisibility { get; init; }

    /// <summary>The maximum number of personal projects a user may create.</summary>
    public int? DefaultProjectsLimit { get; init; }

    public GitLabVisibility? DefaultSnippetVisibility { get; init; }

    /// <summary>Instance-wide dependency management settings.</summary>
    public GitLabDependencyManagementSettings? DependencyManagementSettings { get; init; }

    /// <summary>Stops administrators from connecting to non-trusted OAuth applications.</summary>
    public bool? DisableAdminOauthScopes { get; init; }

    /// <summary>Disables the display of RSS/Atom and calendar feed tokens.</summary>
    public bool? DisableFeedToken { get; init; }

    /// <summary>OAuth sign-in sources to disable.</summary>
    public IReadOnlyList<string>? DisabledOauthSignInSources { get; init; }

    /// <summary>Enables the sign-up domain denylist.</summary>
    public bool? DomainDenylistEnabled { get; init; }

    /// <summary>Email domains that may not sign up. Wildcards allowed.</summary>
    public IReadOnlyList<string>? DomainDenylist { get; init; }

    /// <summary>When set, only these email domains may sign up. Wildcards allowed.</summary>
    public IReadOnlyList<string>? DomainAllowlist { get; init; }

    /// <summary>
    ///     Trusted domains or IP addresses local requests are allowed to reach when local requests for
    ///     webhooks and integrations are otherwise disabled.
    /// </summary>
    public IReadOnlyList<string>? OutboundLocalRequestsWhitelist { get; init; }

    /// <summary>Enables email-based one-time passwords as a multi-factor authentication method.</summary>
    public bool? EmailOtpEnabled { get; init; }

    /// <summary>Allows rendering of iframes in Markdown.</summary>
    public bool? IframeRenderingEnabled { get; init; }

    public IReadOnlyList<string>? IframeRenderingAllowlist { get; init; }

    /// <summary>The raw, unparsed form of <see cref="IframeRenderingAllowlist" />.</summary>
    public string? IframeRenderingAllowlistRaw { get; init; }

    public bool? EksIntegrationEnabled { get; init; }

    public string? EksAccountId { get; init; }

    public string? EksAccessKeyId { get; init; }

    public string? EksSecretAccessKey { get; init; }

    /// <summary>Includes the commit author's name in the body of notification emails.</summary>
    public bool? EmailAuthorInBody { get; init; }

    public GitLabEmailConfirmationSetting? EmailConfirmationSetting { get; init; }

    public GitLabGitAccessProtocol? EnabledGitAccessProtocol { get; init; }

    public bool? GitpodEnabled { get; init; }

    public Uri? GitpodUrl { get; init; }

    /// <summary>Default Gitaly RPC timeout, in seconds.</summary>
    public int? GitalyTimeoutDefault { get; init; }

    /// <summary>Gitaly fast-operation RPC timeout, in seconds.</summary>
    public int? GitalyTimeoutFast { get; init; }

    /// <summary>Gitaly medium-operation RPC timeout, in seconds.</summary>
    public int? GitalyTimeoutMedium { get; init; }

    public bool? GrafanaEnabled { get; init; }
}