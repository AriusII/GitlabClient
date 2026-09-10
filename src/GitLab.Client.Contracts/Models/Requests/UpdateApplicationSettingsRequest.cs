using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

using GitLab.Client.Domain;

namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>PUT /application/settings</c>. Every member is nullable: unset properties
///     are omitted from the payload rather than sent as null, so an update leaves settings it does not
///     mention unchanged. Administrators only.
///     <para>
///         Covers every parameter in GitLab 19.4's <c>RequestBody_ddb13e33e88a</c> schema.
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

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this setting as an unformatted string, so it must round-trip verbatim.")]
    public string? GrafanaUrl { get; init; }

    public bool? GravatarEnabled { get; init; }

    public bool? HelpPageHideCommercialContent { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this setting as an unformatted string, so it must round-trip verbatim.")]
    public string? HelpPageSupportUrl { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this setting as an unformatted string, so it must round-trip verbatim.")]
    public string? HelpPageDocumentationBaseUrl { get; init; }

    public string? HelpPageText { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this setting as an unformatted string, so it must round-trip verbatim.")]
    public string? HomePageUrl { get; init; }

    public bool? HousekeepingEnabled { get; init; }

    public int? HousekeepingFullRepackPeriod { get; init; }

    public int? HousekeepingGcPeriod { get; init; }

    public int? HousekeepingIncrementalRepackPeriod { get; init; }

    public int? HousekeepingOptimizeRepositoryPeriod { get; init; }

    public bool? HtmlEmailsEnabled { get; init; }

    public IReadOnlyList<string>? ImportSources { get; init; }

    public bool? InvisibleCaptchaEnabled { get; init; }

    public int? MaxArtifactsSize { get; init; }

    public int? MaxAttachmentSize { get; init; }

    public int? MaxExportSize { get; init; }

    public int? MaxGithubResponseSizeLimit { get; init; }

    public int? MaxGithubResponseJsonValueCount { get; init; }

    public int? MaxImportSize { get; init; }

    public int? MaxImportRemoteFileSize { get; init; }

    public int? MaxDecompressedArchiveSize { get; init; }

    public int? MaxPagesSize { get; init; }

    public int? MaxPagesCustomDomainsPerProject { get; init; }

    public int? MaxTerraformStateSizeBytes { get; init; }

    public int? MetricsMethodCallThreshold { get; init; }

    public bool? PasswordAuthenticationEnabled { get; init; }

    public bool? PasswordAuthenticationEnabledForWeb { get; init; }

    public bool? SigninEnabled { get; init; }

    public bool? PasswordAuthenticationEnabledForGit { get; init; }

    public string? PerformanceBarAllowedGroupId { get; init; }

    public string? PerformanceBarAllowedGroupPath { get; init; }

    public string? PerformanceBarEnabled { get; init; }

    public string? PersonalAccessTokenPrefix { get; init; }

    public bool? RequirePersonalAccessTokenExpiry { get; init; }

    public bool? KrokiEnabled { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this setting as an unformatted string, so it must round-trip verbatim.")]
    public string? KrokiUrl { get; init; }

    public bool? PlantumlEnabled { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this setting as an unformatted string, so it must round-trip verbatim.")]
    public string? PlantumlUrl { get; init; }

    public bool? DiagramsnetEnabled { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this setting as an unformatted string, so it must round-trip verbatim.")]
    public string? DiagramsnetUrl { get; init; }

    public double? PollingIntervalMultiplier { get; init; }

    public bool? ProjectExportEnabled { get; init; }

    public bool? PrometheusMetricsEnabled { get; init; }

    public int? PushEventHooksLimit { get; init; }

    public int? PushEventActivitiesLimit { get; init; }

    public bool? RecaptchaEnabled { get; init; }

    public string? RecaptchaSiteKey { get; init; }

    public string? RecaptchaPrivateKey { get; init; }

    public bool? LoginRecaptchaProtectionEnabled { get; init; }

    public bool? RepositoryChecksEnabled { get; init; }

    public IReadOnlyDictionary<string, int>? RepositoryStoragesWeighted { get; init; }

    public bool? RequireTwoFactorAuthentication { get; init; }

    public int? TwoFactorGracePeriod { get; init; }

    public IReadOnlyList<string>? RestrictedVisibilityLevels { get; init; }

    public int? SessionExpireDelay { get; init; }

    public bool? SessionExpireFromInit { get; init; }

    public bool? SharedRunnersEnabled { get; init; }

    public string? SharedRunnersText { get; init; }

    public IReadOnlyList<string>? ValidRunnerRegistrars { get; init; }

    public bool? SignupEnabled { get; init; }

    public bool? SourcegraphEnabled { get; init; }

    public bool? SourcegraphPublicOnly { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this setting as an unformatted string, so it must round-trip verbatim.")]
    public string? SourcegraphUrl { get; init; }

    public bool? SpamCheckEndpointEnabled { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this setting as an unformatted string, so it must round-trip verbatim.")]
    public string? SpamCheckEndpointUrl { get; init; }

    public int? TerminalMaxSessionTime { get; init; }

    public bool? UsagePingEnabled { get; init; }

    public int? LocalMarkdownVersion { get; init; }

    public bool? AllowLocalRequestsFromHooksAndServices { get; init; }

    public bool? MailgunEventsEnabled { get; init; }

    public string? MailgunSigningKey { get; init; }

    public bool? SnowplowEnabled { get; init; }

    public string? SnowplowCollectorHostname { get; init; }

    public string? SnowplowCookieDomain { get; init; }

    public string? SnowplowAppId { get; init; }

    public int? IssuesCreateLimit { get; init; }

    public int? RawBlobRequestLimit { get; init; }

    public int? RawBlobRequestLimitUnauthenticated { get; init; }

    public int? WikiPageMaxContentBytes { get; init; }

    public int? DescriptionAndNoteMaxSize { get; init; }

    public bool? WikiAsciidocAllowUriIncludes { get; init; }

    public bool? RequireAdminApprovalAfterUserSignup { get; init; }

    public string? WhatsNewVariant { get; init; }

    public bool? FlocEnabled { get; init; }

    public bool? UserDeactivationEmailsEnabled { get; init; }

    public bool? ShowMigrateFromJenkinsBanner { get; init; }

    public bool? EnableArtifactExternalRedirectWarningPage { get; init; }

    public int? UsersGetByIdLimit { get; init; }

    public int? RunnerTokenExpirationInterval { get; init; }

    public int? GroupRunnerTokenExpirationInterval { get; init; }

    public int? ProjectRunnerTokenExpirationInterval { get; init; }

    public int? PipelineLimitPerProjectUserSha { get; init; }

    public int? PipelineLimitPerUser { get; init; }

    public int? CiLintLimitPerUser { get; init; }

    public string? JiraConnectApplicationKey { get; init; }

    public bool? JiraConnectPublicKeyStorageEnabled { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this setting as an unformatted string, so it must round-trip verbatim.")]
    public string? JiraConnectProxyUrl { get; init; }

    public string? JiraForgeAppId { get; init; }

    public int? BulkImportConcurrentPipelineBatchLimit { get; init; }

    public int? ConcurrentRelationBatchExportLimit { get; init; }

    public bool? BulkImportEnabled { get; init; }

    public int? BulkImportMaxDownloadFile { get; init; }

    public int? AutocompleteUsersLimit { get; init; }

    public int? AutocompleteUsersUnauthenticatedLimit { get; init; }

    public int? ConcurrentGithubImportJobsLimit { get; init; }

    public int? ConcurrentBitbucketImportJobsLimit { get; init; }

    public int? ConcurrentBitbucketServerImportJobsLimit { get; init; }

    public int? ConcurrentPullRequestImportJobsLimit { get; init; }

    public bool? AllowRunnerRegistrationToken { get; init; }

    public int? CiMaxIncludes { get; init; }

    public int? CiMaxCachesPerJob { get; init; }

    public bool? CiJobLiveTraceEnabled { get; init; }

    public int? GitPushPipelineLimit { get; init; }

    public bool? SecurityPolicyGlobalGroupApproversEnabled { get; init; }

    public bool? SlackAppEnabled { get; init; }

    public string? SlackAppId { get; init; }

    public string? SlackAppSecret { get; init; }

    public string? SlackAppSigningSecret { get; init; }

    public string? SlackAppVerificationToken { get; init; }

    public int? NamespaceAggregationScheduleLeaseDurationInSeconds { get; init; }

    public int? ProjectJobsApiRateLimit { get; init; }

    public string? SecurityTxtContent { get; init; }

    public int? DownstreamPipelineTriggerLimitPerProjectUserSha { get; init; }

    public int? AiActionApiRateLimit { get; init; }

    public int? CodeSuggestionsApiRateLimit { get; init; }

    public JsonElement? ResourceUsageLimits { get; init; }

    public GitLabVscodeExtensionMarketplaceSettings? VscodeExtensionMarketplace { get; init; }

    public bool? EnableLanguageServerRestrictions { get; init; }

    public string? MinimumLanguageServerVersion { get; init; }

    public bool? TerraformStateEncryptionEnabled { get; init; }

    public int? LoggingFieldSchemaVersion { get; init; }

    public int? LoggingFieldDualEmitTarget { get; init; }

    public int? RsaKeyRestriction { get; init; }

    public int? DsaKeyRestriction { get; init; }

    public int? EcdsaKeyRestriction { get; init; }

    public int? Ed25519KeyRestriction { get; init; }

    public int? EcdsaSkKeyRestriction { get; init; }

    public int? Ed25519SkKeyRestriction { get; init; }

    public string? AllowAccountDeletion { get; init; }

    public string? AllowApplicationDefaultCredentialsForOfflineTransfer { get; init; }

    public string? AllowBypassPlaceholderConfirmation { get; init; }

    public string? AllowLocalRequestsFromSystemHooks { get; init; }

    public string? AllowLocalRequestsFromWebHooksAndServices { get; init; }

    public string? AllowProjectCreationForGuestAndBelow { get; init; }

    public string? AllowS3CompatibleStorageForOfflineTransfer { get; init; }

    public string? ArchiveBuildsInHumanReadable { get; init; }

    public string? AsciidocMaxIncludes { get; init; }

    public string? AuthorizedKeysEnabled { get; init; }

    public string? AutoAcceptAwardedAchievements { get; init; }

    public string? AutoDevopsDomain { get; init; }

    public string? AutoDevopsEnabled { get; init; }

    public string? BulkImportMaxDownloadFileSize { get; init; }

    public string? CanCreateGroup { get; init; }

    public string? CiDeletePipelinesInSecondsLimitHumanReadable { get; init; }

    public string? CiMaxTotalYamlSizeBytes { get; init; }

    public string? CiPartitionsInSecondsLimit { get; init; }

    public string? CiPartitionsInSecondsLimitHumanReadable { get; init; }

    public string? CommitEmailHostname { get; init; }

    public string? ContainerExpirationPoliciesEnableHistoricEntries { get; init; }

    public string? ContainerRegistryCleanupTagsServiceMaxListSize { get; init; }

    public string? ContainerRegistryDeleteTagsServiceTimeout { get; init; }

    public string? ContainerRegistryExpirationPoliciesCaching { get; init; }

    public string? ContainerRegistryExpirationPoliciesWorkerCapacity { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification =
            "GitLab 19.4 declares custom_http_clone_url_root as an unconstrained string, not a URI-formatted value.")]
    public string? CustomHttpCloneUrlRoot { get; init; }

    public string? DefaultBranchName { get; init; }

    public string? DefaultDarkSyntaxHighlightingTheme { get; init; }

    public string? DefaultPreferredLanguage { get; init; }

    public string? DefaultSyntaxHighlightingTheme { get; init; }

    public string? DeleteInactiveProjects { get; init; }

    public string? DeletionAdjournedPeriod { get; init; }

    public string? DiffMaxCommits { get; init; }

    public string? DiffMaxFiles { get; init; }

    public string? DiffMaxLines { get; init; }

    public string? DiffMaxPatchBytes { get; init; }

    public string? DiffMaxVersions { get; init; }

    public string? DisablePasswordAuthenticationForUsersWithSsoIdentities { get; init; }

    public string? DnsRebindingProtectionEnabled { get; init; }

    public string? EmailRestrictions { get; init; }

    public string? EmailRestrictionsEnabled { get; init; }

    public string? EnforceTerms { get; init; }

    public string? ExternalAuthClientCert { get; init; }

    public string? ExternalAuthClientKey { get; init; }

    public string? ExternalAuthClientKeyPass { get; init; }

    public string? ExternalAuthorizationServiceDefaultLabel { get; init; }

    public string? ExternalAuthorizationServiceEnabled { get; init; }

    public string? ExternalAuthorizationServiceTimeout { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this setting as an unformatted string, so it must round-trip verbatim.")]
    public string? ExternalAuthorizationServiceUrl { get; init; }

    public string? ExternalPipelineValidationServiceTimeout { get; init; }

    public string? ExternalPipelineValidationServiceToken { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this setting as an unformatted string, so it must round-trip verbatim.")]
    public string? ExternalPipelineValidationServiceUrl { get; init; }

    public string? FailedLoginAttemptsUnlockPeriodInMinutes { get; init; }

    public string? FirstDayOfWeek { get; init; }

    public string? GitlabDedicatedInstance { get; init; }

    public string? GitlabEnvironmentToolkitInstance { get; init; }

    public string? GitlabProductUsageDataEnabled { get; init; }

    public string? GitlabShellOperationLimit { get; init; }

    public string? HashedStorageEnabled { get; init; }

    public string? HideThirdPartyOffers { get; init; }

    public string? InactiveProjectsDeleteAfterMonths { get; init; }

    public string? InactiveProjectsMinSizeMb { get; init; }

    public string? InactiveProjectsSendWarningEmailAfterMonths { get; init; }

    public string? InactiveResourceAccessTokensDeleteAfterDays { get; init; }

    public string? IncludeOptionalMetricsInServicePing { get; init; }

    public string? KeepLatestArtifact { get; init; }

    public string? KrokiDiagramProxyEnabled { get; init; }

    public string? KrokiFormats { get; init; }

    public string? LockRequireShaForMerge { get; init; }

    public string? MaxHttpDecompressedSize { get; init; }

    public string? MaxHttpResponseCsvStructuralChars { get; init; }

    public string? MaxHttpResponseJsonDepth { get; init; }

    public string? MaxHttpResponseJsonStructuralChars { get; init; }

    public string? MaxHttpResponseSizeLimit { get; init; }

    public string? MaxHttpResponseXmlStructuralChars { get; init; }

    public string? MaxLoginAttempts { get; init; }

    public string? MaxYamlDepth { get; init; }

    public string? MaxYamlSizeBytes { get; init; }

    public string? MinimumPasswordLength { get; init; }

    public string? MirrorAvailable { get; init; }

    public string? NotifyOnUnknownSignIn { get; init; }

    public string? OfflineTransferExportsEnabled { get; init; }

    public string? OfflineTransferImportsEnabled { get; init; }

    public string? PackageRegistryAllowAnyoneToPullOption { get; init; }

    public string? PackageRegistryCleanupPoliciesWorkerCapacity { get; init; }

    public string? PagesDomainVerificationEnabled { get; init; }

    public string? PagesUniqueDomainDefaultEnabled { get; init; }

    public string? PlantumlDiagramProxyEnabled { get; init; }

    public string? ProjectsApiRateLimitUnauthenticated { get; init; }

    public string? ProtectedCiVariables { get; init; }

    public string? RateLimitingResponseText { get; init; }

    public string? ReceiveMaxInputSize { get; init; }

    public string? RelationExportBatchSize { get; init; }

    public string? RememberMeEnabled { get; init; }

    public string? RequireAdminTwoFactorAuthentication { get; init; }

    public string? RequireEmailVerificationOnAccountLocked { get; init; }

    public string? RequireShaForMerge { get; init; }

    public string? RunnerJobsEndpointsApiLimit { get; init; }

    public string? RunnerJobsPatchTraceApiLimit { get; init; }

    public string? RunnerJobsRequestApiLimit { get; init; }

    public string? SearchRateLimit { get; init; }

    public string? SearchRateLimitUnauthenticated { get; init; }

    public string? SidekiqJobLimiterCompressionThresholdBytes { get; init; }

    public string? SidekiqJobLimiterLimitBytes { get; init; }

    public string? SidekiqJobLimiterMode { get; init; }

    public string? SidekiqTimezoneOverride { get; init; }

    public string? SignInRestrictions { get; init; }

    public string? SilentAdminExportsEnabled { get; init; }

    public string? SilentModeEnabled { get; init; }

    public string? SnippetSizeLimit { get; init; }

    public string? SnowplowDatabaseCollectorHostname { get; init; }

    public string? SpamCheckApiKey { get; init; }

    public string? StaticObjectsExternalStorageAuthToken { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this setting as an unformatted string, so it must round-trip verbatim.")]
    public string? StaticObjectsExternalStorageUrl { get; init; }

    public string? Terms { get; init; }

    public string? ThrottleAuthenticatedApiEnabled { get; init; }

    public string? ThrottleAuthenticatedApiPeriodInSeconds { get; init; }

    public string? ThrottleAuthenticatedApiRequestsPerPeriod { get; init; }

    public string? ThrottleAuthenticatedGitHttpEnabled { get; init; }

    public string? ThrottleAuthenticatedGitHttpPeriodInSeconds { get; init; }

    public string? ThrottleAuthenticatedGitHttpRequestsPerPeriod { get; init; }

    public string? ThrottleAuthenticatedPackagesApiEnabled { get; init; }

    public string? ThrottleAuthenticatedPackagesApiPeriodInSeconds { get; init; }

    public string? ThrottleAuthenticatedPackagesApiRequestsPerPeriod { get; init; }

    public string? ThrottleAuthenticatedWebEnabled { get; init; }

    public string? ThrottleAuthenticatedWebPeriodInSeconds { get; init; }

    public string? ThrottleAuthenticatedWebRequestsPerPeriod { get; init; }

    public string? ThrottleUnauthenticatedApiEnabled { get; init; }

    public string? ThrottleUnauthenticatedApiPeriodInSeconds { get; init; }

    public string? ThrottleUnauthenticatedApiRequestsPerPeriod { get; init; }

    public string? ThrottleUnauthenticatedEnabled { get; init; }

    public string? ThrottleUnauthenticatedGitHttpEnabled { get; init; }

    public string? ThrottleUnauthenticatedGitHttpPeriodInSeconds { get; init; }

    public string? ThrottleUnauthenticatedGitHttpRequestsPerPeriod { get; init; }

    public string? ThrottleUnauthenticatedPackagesApiEnabled { get; init; }

    public string? ThrottleUnauthenticatedPackagesApiPeriodInSeconds { get; init; }

    public string? ThrottleUnauthenticatedPackagesApiRequestsPerPeriod { get; init; }

    public string? ThrottleUnauthenticatedPeriodInSeconds { get; init; }

    public string? ThrottleUnauthenticatedRequestsPerPeriod { get; init; }

    public string? ThrottleUnauthenticatedWebEnabled { get; init; }

    public string? ThrottleUnauthenticatedWebPeriodInSeconds { get; init; }

    public string? ThrottleUnauthenticatedWebRequestsPerPeriod { get; init; }

    public string? TimeTrackingLimitToHours { get; init; }

    public string? TopLevelGroupCreationEnabled { get; init; }

    public string? UniqueIpsLimitEnabled { get; init; }

    public string? UniqueIpsLimitPerUser { get; init; }

    public string? UniqueIpsLimitTimeWindow { get; init; }

    public string? UpdateRunnerVersionsEnabled { get; init; }

    public string? UseClickhouseForAnalytics { get; init; }

    public string? UserDefaultExternal { get; init; }

    public string? UserDefaultInternalRegex { get; init; }

    public string? UserDefaultsToPrivateProfile { get; init; }

    public string? UserOauthApplications { get; init; }

    public string? UserShowAddSshKeyMessage { get; init; }

    public string? UsersApiLimitFollowers { get; init; }

    public string? UsersApiLimitFollowing { get; init; }

    public string? UsersApiLimitGpgKey { get; init; }

    public string? UsersApiLimitGpgKeys { get; init; }

    public string? UsersApiLimitStatus { get; init; }

    public string? VersionCheckEnabled { get; init; }

    public string? WebHookEventResendLimit { get; init; }

    public string? WebHookTestLimit { get; init; }

    public bool? ElasticsearchAws { get; init; }

    public string? ElasticsearchAwsAccessKey { get; init; }

    public string? ElasticsearchAwsRegion { get; init; }

    public string? ElasticsearchAwsSecretAccessKey { get; init; }

    public bool? ElasticsearchIndexing { get; init; }

    public bool? ElasticsearchSearch { get; init; }

    public bool? ElasticsearchPauseIndexing { get; init; }

    public bool? ElasticsearchAdvancedSearchPauseIndexing { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 permits a comma-separated cluster endpoint list, which is not one System.Uri.")]
    public string? ElasticsearchUrl { get; init; }

    public string? ElasticsearchUsername { get; init; }

    public string? ElasticsearchPassword { get; init; }

    public bool? ElasticsearchLimitIndexing { get; init; }

    public bool? ActiveContextPauseIndexing { get; init; }

    public IReadOnlyList<int>? ElasticsearchNamespaceIds { get; init; }

    public IReadOnlyList<int>? ElasticsearchProjectIds { get; init; }

    public bool? SecretDetectionTokenRevocationEnabled { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this setting as an unformatted string, so it must round-trip verbatim.")]
    public string? SecretDetectionTokenRevocationUrl { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this setting as an unformatted string, so it must round-trip verbatim.")]
    public string? SecretDetectionRevocationTokenTypesUrl { get; init; }

    public string? EmailAdditionalText { get; init; }

    public bool? DefaultProjectDeletionProtection { get; init; }

    public bool? DisablePersonalAccessTokens { get; init; }

    public int? RepositorySizeLimit { get; init; }

    public int? FileTemplateProjectId { get; init; }

    public bool? UpdatingNameDisabledForUsers { get; init; }

    public bool? DisableOverridingApproversPerMergeRequest { get; init; }

    public bool? PreventMergeRequestsAuthorApproval { get; init; }

    public bool? PreventMergeRequestsCommittersApproval { get; init; }

    public bool? MavenPackageRequestsForwarding { get; init; }

    public bool? NpmPackageRequestsForwarding { get; init; }

    public bool? PypiPackageRequestsForwarding { get; init; }

    public bool? RubygemsPackageRequestsForwarding { get; init; }

    public int? VirtualRegistriesEndpointsApiLimit { get; init; }

    public int? AuditEventsApiLimit { get; init; }

    public bool? GroupOwnersCanManageDefaultBranchProtection { get; init; }

    public bool? MaintenanceMode { get; init; }

    public string? MaintenanceModeMessage { get; init; }

    public int? GitTwoFactorSessionExpiry { get; init; }

    public int? MaxNumberOfRepositoryDownloads { get; init; }

    public int? MaxNumberOfRepositoryDownloadsWithinTimePeriod { get; init; }

    public IReadOnlyList<string>? GitRateLimitUsersAllowlist { get; init; }

    public IReadOnlyList<int>? GitRateLimitUsersAlertlist { get; init; }

    public bool? AutoBanUserOnExcessiveProjectsDownload { get; init; }

    public bool? MakeProfilePrivate { get; init; }

    public bool? ServiceAccessTokensExpirationEnforced { get; init; }

    public bool? DuoFeaturesEnabled { get; init; }

    public bool? LockDuoFeaturesEnabled { get; init; }

    public bool? DisabledDirectCodeSuggestions { get; init; }

    public bool? ReceptiveClusterAgentsEnabled { get; init; }

    public bool? AutoDuoCodeReviewEnabled { get; init; }

    public int? SecurityScanStaleAfterDays { get; init; }

    public bool? DuoCustomAgentsEnabled { get; init; }

    public bool? LockDuoCustomAgentsEnabled { get; init; }

    public bool? DuoCustomFlowsEnabled { get; init; }

    public bool? LockDuoCustomFlowsEnabled { get; init; }

    public bool? DuoExternalAgentsEnabled { get; init; }

    public bool? LockDuoExternalAgentsEnabled { get; init; }

    public bool? DuoRemoteFlowsEnabled { get; init; }

    public bool? LockDuoRemoteFlowsEnabled { get; init; }

    public string? DuoWorkflowsDefaultImageRegistry { get; init; }

    public string? CiTelemetryOtelEndpoint { get; init; }

    public double? CiJobTelemetrySamplingRate { get; init; }

    public IReadOnlyList<GitLabDuoNamespaceAccessRule>? DuoNamespaceAccessRules { get; init; }

    public bool? BuiltInProjectTemplatesEnabled { get; init; }

    public bool? LockBuiltInProjectTemplatesEnabled { get; init; }

    public int? DuoTemplateProjectId { get; init; }

    public bool? UseNatsForAuditStreaming { get; init; }

    public string? AllowAllIntegrations { get; init; }

    public string? AllowGroupOwnersToManageLdap { get; init; }

    public string? AllowedIntegrations { get; init; }

    public string? AutomaticPurchasedStorageAllocation { get; init; }

    public string? CheckNamespacePlan { get; init; }

    public string? DeleteUnconfirmedUsers { get; init; }

    public string? DisableInviteMembers { get; init; }

    public string? ElasticsearchClientAdapter { get; init; }

    public string? ElasticsearchIndexedFieldLengthLimit { get; init; }

    public string? ElasticsearchIndexedFileSizeLimitKb { get; init; }

    public string? ElasticsearchMaxBulkConcurrency { get; init; }

    public string? ElasticsearchMaxBulkSizeMb { get; init; }

    public string? ElasticsearchMaxCodeIndexingConcurrency { get; init; }

    public string? ElasticsearchReplicas { get; init; }

    public string? ElasticsearchRequeueWorkers { get; init; }

    public string? ElasticsearchRetryOnFailure { get; init; }

    public string? ElasticsearchShards { get; init; }

    public string? ElasticsearchWorkerNumberOfShards { get; init; }

    public string? EnforceNamespaceStorageLimit { get; init; }

    public string? EnforcePiplCompliance { get; init; }

    public string? GeoNodeAllowedIps { get; init; }

    public string? GeoStatusTimeout { get; init; }

    public string? GloballyAllowedIps { get; init; }

    public string? GroupSecretsLimit { get; init; }

    public string? LockMembershipsToSaml { get; init; }

    public string? MaxPersonalAccessTokenLifetime { get; init; }

    public string? MaxSshKeyLifetime { get; init; }

    public string? MirrorCapacityThreshold { get; init; }

    public string? MirrorMaxCapacity { get; init; }

    public string? MirrorMaxDelay { get; init; }

    public string? PackageMetadataPurlTypes { get; init; }

    public string? PasswordLowercaseRequired { get; init; }

    public string? PasswordNumberRequired { get; init; }

    public string? PasswordSymbolRequired { get; init; }

    public string? PasswordUppercaseRequired { get; init; }

    public string? ProjectSecretsLimit { get; init; }

    public string? ScanExecutionPoliciesActionLimit { get; init; }

    public string? ScanExecutionPoliciesScheduleLimit { get; init; }

    public string? SecretPushProtectionAvailable { get; init; }

    public string? SecurityApprovalPoliciesLimit { get; init; }

    public string? SecurityMrReportCacheLifetimeMinutes { get; init; }

    public string? SharedRunnersMinutes { get; init; }

    public string? UnconfirmedUsersDeleteAfterDays { get; init; }

    public string? AllowPossibleSpam { get; init; }

    public string? ImportJobsConcurrencyLimit { get; init; }

    public string? DenyAllRequestsExceptAllowed { get; init; }

    public string? RootMovedPermanentlyRedirection { get; init; }

    public string? DomainDenylistRaw { get; init; }

    public string? DomainAllowlistRaw { get; init; }

    public string? OutboundLocalRequestsAllowlistRaw { get; init; }

    public string? EnforceCiInboundJobTokenScopeEnabled { get; init; }

    public string? EnforceEmailSubaddressRestrictions { get; init; }

    public string? ErrorTrackingEnabled { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this setting as an unformatted string, so it must round-trip verbatim.")]
    public string? ErrorTrackingApiUrl { get; init; }

    public string? ForcePagesAccessControl { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this setting as an unformatted string, so it must round-trip verbatim.")]
    public string? JiraConnectAdditionalAudienceUrl { get; init; }

    public string? MathRenderingLimitsEnabled { get; init; }

    public string? MaxArtifactsContentIncludeSize { get; init; }

    public string? OrganizationClusterAgentAuthorizationEnabled { get; init; }

    public string? InstanceTokenPrefix { get; init; }

    public string? PagesExtraDeploymentsDefaultExpirySeconds { get; init; }

    public string? ThrottleAuthenticatedGitLfsEnabled { get; init; }

    public string? ThrottleAuthenticatedGitLfsPeriodInSeconds { get; init; }

    public string? ThrottleAuthenticatedGitLfsRequestsPerPeriod { get; init; }

    public string? ThrottleAuthenticatedFilesApiEnabled { get; init; }

    public string? ThrottleAuthenticatedFilesApiPeriodInSeconds { get; init; }

    public string? ThrottleAuthenticatedFilesApiRequestsPerPeriod { get; init; }

    public string? ThrottleAuthenticatedDeprecatedApiEnabled { get; init; }

    public string? ThrottleAuthenticatedDeprecatedApiPeriodInSeconds { get; init; }

    public string? ThrottleAuthenticatedDeprecatedApiRequestsPerPeriod { get; init; }

    public string? ThrottleUnauthenticatedFilesApiEnabled { get; init; }

    public string? ThrottleUnauthenticatedFilesApiPeriodInSeconds { get; init; }

    public string? ThrottleUnauthenticatedFilesApiRequestsPerPeriod { get; init; }

    public string? ThrottleUnauthenticatedDeprecatedApiEnabled { get; init; }

    public string? ThrottleUnauthenticatedDeprecatedApiPeriodInSeconds { get; init; }

    public string? ThrottleUnauthenticatedDeprecatedApiRequestsPerPeriod { get; init; }

    public string? ThrottleProtectedPathsEnabled { get; init; }

    public string? ThrottleProtectedPathsPeriodInSeconds { get; init; }

    public string? ThrottleProtectedPathsRequestsPerPeriod { get; init; }

    public string? ProtectedPathsRaw { get; init; }

    public string? ProtectedPathsForGetRequestRaw { get; init; }

    public string? UsagePingGenerationEnabled { get; init; }

    public string? UsagePingFeaturesEnabled { get; init; }

    public string? ProjectCreateLimit { get; init; }

    public string? GroupCreateLimit { get; init; }

    public string? NotesCreateLimit { get; init; }

    public string? NotesCreateLimitAllowlistRaw { get; init; }

    public string? MembersDeleteLimit { get; init; }

    public string? ProjectImportLimit { get; init; }

    public string? ProjectExportLimit { get; init; }

    public string? ProjectDownloadExportLimit { get; init; }

    public string? GroupImportLimit { get; init; }

    public string? GroupExportLimit { get; init; }

    public string? GroupDownloadExportLimit { get; init; }

    public string? ResourceAccessTokenNotifyInherited { get; init; }

    public string? LockResourceAccessTokenNotifyInherited { get; init; }

    public string? SentryEnabled { get; init; }

    public string? SentryDsn { get; init; }

    public string? SentryClientsideDsn { get; init; }

    public string? SentryEnvironment { get; init; }

    public string? SentryClientsideTracesSampleRate { get; init; }

    public string? SearchRateLimitAllowlistRaw { get; init; }

    public string? UsersGetByIdLimitAllowlistRaw { get; init; }

    public string? InvitationFlowEnforcement { get; init; }

    public string? CanCreateOrganization { get; init; }

    public string? ConcurrentRelationExportLimit { get; init; }

    public string? AllowContributionMappingToAdmins { get; init; }

    public string? DeactivationEmailAdditionalText { get; init; }

    public string? GroupApiLimit { get; init; }

    public string? GroupArchiveUnarchiveApiLimit { get; init; }

    public string? GroupInvitedGroupsApiLimit { get; init; }

    public string? GroupSharedGroupsApiLimit { get; init; }

    public string? GroupProjectsApiLimit { get; init; }

    public string? GroupsApiLimit { get; init; }

    public string? ProjectApiLimit { get; init; }

    public string? ProjectInvitedGroupsApiLimit { get; init; }

    public string? ProjectRepositoriesBlobsBatchLimit { get; init; }

    public string? ProjectsApiLimit { get; init; }

    public string? ProjectMembersApiLimit { get; init; }

    public string? CreateOrganizationApiLimit { get; init; }

    public string? UserContributedProjectsApiLimit { get; init; }

    public string? UserProjectsApiLimit { get; init; }

    public string? UserStarredProjectsApiLimit { get; init; }

    public string? UsersApiLimitSshKeys { get; init; }

    public string? UsersApiLimitSshKey { get; init; }

    public string? TagsCreateLimit { get; init; }

    public string? ObservabilityBackendSslVerificationEnabled { get; init; }

    public string? GlobalSearchSnippetTitlesEnabled { get; init; }

    public string? GlobalSearchUsersEnabled { get; init; }

    public string? GlobalSearchGroupsEnabled { get; init; }

    public string? GlobalSearchWorkItemsEnabled { get; init; }

    public string? GlobalSearchMergeRequestsEnabled { get; init; }

    public string? GlobalSearchBlockAnonymousSearchesEnabled { get; init; }

    public string? VscodeExtensionMarketplaceEnabled { get; init; }

    public string? VscodeExtensionMarketplaceExtensionHostDomain { get; init; }

    public string? VscodeExtensionMarketplaceSingleOriginFallbackEnabled { get; init; }

    public string? ReindexingMinimumIndexSize { get; init; }

    public string? ReindexingMinimumRelativeBloatSize { get; init; }

    public string? AnonymousSearchesAllowed { get; init; }

    public string? DefaultSearchScope { get; init; }

    public string? DelayUserAccountSelfDeletion { get; init; }

    public string? BackgroundOperationsMaxJobs { get; init; }

    public string? EnforceGranularTokens { get; init; }

    public string? GranularTokensEnforcedAfter { get; init; }

    public string? DeactivateDormantUsers { get; init; }

    public string? DeactivateDormantUsersPeriod { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification =
            "GitLab 19.4 declares nuget_skip_metadata_url_validation as an unconstrained string setting, not a URI-formatted value.")]
    public string? NugetSkipMetadataUrlValidation { get; init; }

    public string? HelmMaxPackagesCount { get; init; }

    public string? McpServerEnabled { get; init; }

    public string? DynamicClientRegistrationEnabled { get; init; }

    public string? CiCdCatalogProjectsAllowlist { get; init; }

    public string? CiCdCatalogProjectsAllowlistRaw { get; init; }

    public string? DuoChatExpirationColumn { get; init; }

    public string? DuoChatExpirationDays { get; init; }

    public string? ElasticsearchAwsRoleArn { get; init; }

    public string? ElasticsearchClientRequestTimeout { get; init; }

    public string? ElasticsearchIndexingTimeoutMinutes { get; init; }

    public string? ElasticsearchAnalyzersSmartcnEnabled { get; init; }

    public string? ElasticsearchAnalyzersSmartcnSearch { get; init; }

    public string? ElasticsearchAnalyzersKuromojiEnabled { get; init; }

    public string? ElasticsearchAnalyzersKuromojiSearch { get; init; }

    public string? ElasticsearchCodeScope { get; init; }

    public string? InstanceLevelAiBetaFeaturesEnabled { get; init; }

    public string? LockMembershipsToLdap { get; init; }

    public string? LockModelPromptCacheEnabled { get; init; }

    public string? ModelPromptCacheEnabled { get; init; }

    public string? SearchMaxShardSizeGb { get; init; }

    public string? SearchMaxDocsDenominator { get; init; }

    public string? SearchMinDocsBeforeRollover { get; init; }

    public string? SecretDetectionTokenRevocationToken { get; init; }

    public string? ThrottleIncidentManagementNotificationEnabled { get; init; }

    public string? ThrottleIncidentManagementNotificationPerPeriod { get; init; }

    public string? ThrottleIncidentManagementNotificationPeriodInSeconds { get; init; }

    public string? ProductAnalyticsEnabled { get; init; }

    public string? ProductAnalyticsDataCollectorHost { get; init; }

    public string? ProductAnalyticsConfiguratorConnectionString { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this setting as an unformatted string, so it must round-trip verbatim.")]
    public string? CubeApiBaseUrl { get; init; }

    public string? CubeApiKey { get; init; }

    public string? DuoAvailability { get; init; }

    public string? DuoAgentPlatformEnabled { get; init; }

    public string? DuoCliEnabled { get; init; }

    public string? AiAuditEventsStreamingEnabled { get; init; }

    public string? DuoRemoteFlowsAvailability { get; init; }

    public string? DuoFoundationalFlowsEnabled { get; init; }

    public string? DuoFoundationalFlowsAvailability { get; init; }

    public string? DuoCustomAgentsAvailability { get; init; }

    public string? DuoCustomFlowsAvailability { get; init; }

    public string? DuoExternalAgentsAvailability { get; init; }

    public string? ToolApprovalForSessionEnabled { get; init; }

    public string? ToolApprovalForSessionAvailability { get; init; }

    public string? EnabledExpandedLogging { get; init; }

    public string? FoundationalAgentsDefaultEnabled { get; init; }

    public string? FoundationalAgentsStatuses { get; init; }

    public string? ZoektIndexingEnabled { get; init; }

    public string? ZoektSearchEnabled { get; init; }

    public string? ZoektIndexingPaused { get; init; }

    public string? ZoektAutoIndexRootNamespace { get; init; }

    public string? ZoektCacheResponse { get; init; }

    public string? ZoektCpuToTasksRatio { get; init; }

    public string? ZoektForceReindexingPercentage { get; init; }

    public string? ZoektIndexingParallelism { get; init; }

    public string? ZoektRolloutBatchSize { get; init; }

    public string? ZoektLostNodeThreshold { get; init; }

    public string? ZoektIndexingTimeout { get; init; }

    public string? ZoektMaximumFiles { get; init; }

    public string? ZoektIndexedFileSizeLimit { get; init; }

    public string? ZoektTrigramMax { get; init; }

    public string? ZoektRolloutRetryInterval { get; init; }

    public string? ZoektDefaultNumberOfReplicas { get; init; }

    public string? ZoektMaxProjectsForLegacySearch { get; init; }

    [JsonPropertyName("zoekt_max_restarts_15m")]
    public string? ZoektMaxRestarts15m { get; init; }

    public string? DuoWorkflowOauthApplicationId { get; init; }

    public string? PipelineExecutionPoliciesPerConfigurationLimit { get; init; }

    public string? ScanExecutionPoliciesPerConfigurationLimit { get; init; }

    public string? VulnerabilityManagementPoliciesPerConfigurationLimit { get; init; }

    public string? DependencyFirewallPoliciesPerConfigurationLimit { get; init; }

    public string? PolicyStoreExperimentEnabled { get; init; }

    public string? SecretDetectionServiceAuthToken { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this setting as an unformatted string, so it must round-trip verbatim.")]
    public string? SecretDetectionServiceUrl { get; init; }

    public string? FetchObservabilityAlertsFromCloud { get; init; }

    public string? GlobalSearchCodeEnabled { get; init; }

    public string? GlobalSearchCommitsEnabled { get; init; }

    public string? GlobalSearchWikiEnabled { get; init; }

    public string? GlobalSearchLimitedIndexingEnabled { get; init; }

    public string? ElasticMigrationWorkerEnabled { get; init; }

    public string? DisplayGitlabCreditsUserData { get; init; }

    public string? VacProjectIdsRaw { get; init; }

    public string? EnableMemberPromotionManagement { get; init; }

    public string? AllowDeployTokensAndKeysWithExternalAuthn { get; init; }

    public string? AllowTopLevelGroupOwnersToCreateServiceAccounts { get; init; }

    public string? DependencyScanningSbomScanApiUploadLimit { get; init; }

    public string? DependencyScanningSbomScanApiDownloadLimit { get; init; }
}

/// <summary>GitLab 19.4 settings for the VS Code Extension Marketplace.</summary>
public sealed record GitLabVscodeExtensionMarketplaceSettings
{
    public bool? Enabled { get; init; }

    public string? Preset { get; init; }

    /// <summary>
    ///     Custom marketplace endpoint values. GitLab's schema intentionally leaves this object open.
    /// </summary>
    public JsonElement? CustomValues { get; init; }
}