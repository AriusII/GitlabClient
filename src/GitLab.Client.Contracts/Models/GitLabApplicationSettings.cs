using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using GitLab.Client.Models.Requests;

namespace GitLab.Client.Models;

/// <summary>
///     The current instance-wide application settings, as returned by both
///     <c>GET /application/settings</c> and <c>PUT /application/settings</c>. Administrators only.
///     <para>
///         GitLab's schema types most members as a bare <c>string</c>, including many settings that
///         have richer request-side types. Those string members remain strings here. The thirteen
///         members the GitLab 19.4 response schema explicitly declares as Boolean, integer, or an
///         Elasticsearch-index collection retain those types, so source-generated deserialization
///         accepts a schema-conformant JSON token instead of failing on a Boolean or array represented
///         as a <see cref="string" />. Update requests should go through the correctly-typed
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
    public string? DefaultDarkSyntaxHighlightingTheme { get; init; }

    public string? DeleteInactiveProjects { get; init; }

    public string? DeletionAdjournedPeriod { get; init; }

    public string? DenyAllRequestsExceptAllowed { get; init; }

    public string? DependencyManagementSettings { get; init; }

    public string? DisableAdminOauthScopes { get; init; }

    public string? DisableFeedToken { get; init; }

    public string? DisablePasswordAuthenticationForUsersWithSsoIdentities { get; init; }

    public string? RootMovedPermanentlyRedirection { get; init; }

    public string? DisabledOauthSignInSources { get; init; }

    public string? DomainDenylist { get; init; }

    public string? DomainDenylistEnabled { get; init; }

    public string? DomainDenylistRaw { get; init; }

    public string? DomainAllowlist { get; init; }

    public string? DomainAllowlistRaw { get; init; }

    public string? OutboundLocalRequestsAllowlistRaw { get; init; }

    public string? OutboundLocalRequestsWhitelist { get; init; }

    public string? DsaKeyRestriction { get; init; }

    public string? EcdsaKeyRestriction { get; init; }

    public string? EcdsaSkKeyRestriction { get; init; }

    public string? Ed25519KeyRestriction { get; init; }

    public string? Ed25519SkKeyRestriction { get; init; }

    public string? EksIntegrationEnabled { get; init; }

    public string? EksAccountId { get; init; }

    public string? EksAccessKeyId { get; init; }

    public string? EmailAuthorInBody { get; init; }

    public string? EmailConfirmationSetting { get; init; }

    public string? EmailOtpEnabled { get; init; }

    public string? EnabledGitAccessProtocol { get; init; }

    public string? EnforceCiInboundJobTokenScopeEnabled { get; init; }

    public string? EnforceEmailSubaddressRestrictions { get; init; }

    public string? RequireEmailVerificationOnAccountLocked { get; init; }

    public string? EnforceTerms { get; init; }

    public string? ErrorTrackingEnabled { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this response member as a string; see the type-level remarks.")]
    public string? ErrorTrackingApiUrl { get; init; }

    public string? ExternalPipelineValidationServiceTimeout { get; init; }

    public string? ExternalPipelineValidationServiceToken { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this response member as a string; see the type-level remarks.")]
    public string? ExternalPipelineValidationServiceUrl { get; init; }

    public string? FailedLoginAttemptsUnlockPeriodInMinutes { get; init; }

    public string? FirstDayOfWeek { get; init; }

    public string? FlocEnabled { get; init; }

    public string? ForcePagesAccessControl { get; init; }

    public string? GitalyTimeoutDefault { get; init; }

    public string? GitalyTimeoutMedium { get; init; }

    public string? GitalyTimeoutFast { get; init; }

    public string? GitpodEnabled { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this response member as a string; see the type-level remarks.")]
    public string? GitpodUrl { get; init; }

    public string? GrafanaEnabled { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this response member as a string; see the type-level remarks.")]
    public string? GrafanaUrl { get; init; }

    public string? GravatarEnabled { get; init; }

    public string? HashedStorageEnabled { get; init; }

    public string? HelpPageHideCommercialContent { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this response member as a string; see the type-level remarks.")]
    public string? HelpPageSupportUrl { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this response member as a string; see the type-level remarks.")]
    public string? HelpPageDocumentationBaseUrl { get; init; }

    public string? HelpPageText { get; init; }

    public string? HideThirdPartyOffers { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this response member as a string; see the type-level remarks.")]
    public string? HomePageUrl { get; init; }

    public string? HousekeepingEnabled { get; init; }

    public string? HousekeepingFullRepackPeriod { get; init; }

    public string? HousekeepingGcPeriod { get; init; }

    public string? HousekeepingIncrementalRepackPeriod { get; init; }

    public string? HousekeepingOptimizeRepositoryPeriod { get; init; }

    public string? HtmlEmailsEnabled { get; init; }

    public string? IframeRenderingEnabled { get; init; }

    public string? IframeRenderingAllowlist { get; init; }

    public string? IframeRenderingAllowlistRaw { get; init; }

    public string? ImportSources { get; init; }

    public string? InactiveResourceAccessTokensDeleteAfterDays { get; init; }

    public string? InactiveProjectsDeleteAfterMonths { get; init; }

    public string? InactiveProjectsMinSizeMb { get; init; }

    public string? InactiveProjectsSendWarningEmailAfterMonths { get; init; }

    public string? IncludeOptionalMetricsInServicePing { get; init; }

    public string? InvisibleCaptchaEnabled { get; init; }

    public string? JiraConnectApplicationKey { get; init; }

    public string? JiraConnectPublicKeyStorageEnabled { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this response member as a string; see the type-level remarks.")]
    public string? JiraConnectProxyUrl { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this response member as a string; see the type-level remarks.")]
    public string? JiraConnectAdditionalAudienceUrl { get; init; }

    public string? JiraForgeAppId { get; init; }

    public string? MathRenderingLimitsEnabled { get; init; }

    public string? RequireShaForMerge { get; init; }

    public string? LockRequireShaForMerge { get; init; }

    public string? MaxArtifactsContentIncludeSize { get; init; }

    public string? MaxArtifactsSize { get; init; }

    public string? MaxAttachmentSize { get; init; }

    public string? MaxDecompressedArchiveSize { get; init; }

    public string? MaxExportSize { get; init; }

    public string? MaxGithubResponseSizeLimit { get; init; }

    public string? MaxGithubResponseJsonValueCount { get; init; }

    public string? MaxHttpDecompressedSize { get; init; }

    public string? MaxHttpResponseSizeLimit { get; init; }

    public string? MaxHttpResponseJsonDepth { get; init; }

    public string? MaxHttpResponseJsonStructuralChars { get; init; }

    public string? MaxHttpResponseXmlStructuralChars { get; init; }

    public string? MaxHttpResponseCsvStructuralChars { get; init; }

    public string? MaxImportSize { get; init; }

    public string? MaxImportRemoteFileSize { get; init; }

    public string? MaxLoginAttempts { get; init; }

    public string? MaxPagesSize { get; init; }

    public string? MaxPagesCustomDomainsPerProject { get; init; }

    public string? MaxTerraformStateSizeBytes { get; init; }

    public string? MaxYamlSizeBytes { get; init; }

    public string? MaxYamlDepth { get; init; }

    public string? MetricsMethodCallThreshold { get; init; }

    public string? MinimumPasswordLength { get; init; }

    public string? MirrorAvailable { get; init; }

    public string? NotifyOnUnknownSignIn { get; init; }

    public string? OrganizationClusterAgentAuthorizationEnabled { get; init; }

    public string? PagesDomainVerificationEnabled { get; init; }

    public string? PagesUniqueDomainDefaultEnabled { get; init; }

    public string? PasswordAuthenticationEnabledForWeb { get; init; }

    public string? PasswordAuthenticationEnabledForGit { get; init; }

    public string? PersonalAccessTokenPrefix { get; init; }

    public string? InstanceTokenPrefix { get; init; }

    public string? KrokiEnabled { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this response member as a string; see the type-level remarks.")]
    public string? KrokiUrl { get; init; }

    public string? KrokiFormats { get; init; }

    public string? KrokiDiagramProxyEnabled { get; init; }

    public string? PlantumlEnabled { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this response member as a string; see the type-level remarks.")]
    public string? PlantumlUrl { get; init; }

    public string? PlantumlDiagramProxyEnabled { get; init; }

    public string? DiagramsnetEnabled { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this response member as a string; see the type-level remarks.")]
    public string? DiagramsnetUrl { get; init; }

    public string? PagesExtraDeploymentsDefaultExpirySeconds { get; init; }

    public string? PollingIntervalMultiplier { get; init; }

    public string? ProjectExportEnabled { get; init; }

    public string? PrometheusMetricsEnabled { get; init; }

    public string? RecaptchaEnabled { get; init; }

    public string? RecaptchaPrivateKey { get; init; }

    public string? RecaptchaSiteKey { get; init; }

    public string? LoginRecaptchaProtectionEnabled { get; init; }

    public string? ReceiveMaxInputSize { get; init; }

    public string? RepositoryChecksEnabled { get; init; }

    public string? RequireAdminApprovalAfterUserSignup { get; init; }

    public string? RequireAdminTwoFactorAuthentication { get; init; }

    public string? RequireTwoFactorAuthentication { get; init; }

    public string? RememberMeEnabled { get; init; }

    public string? RestrictedVisibilityLevels { get; init; }

    public string? RsaKeyRestriction { get; init; }

    public string? SessionExpireDelay { get; init; }

    public string? SessionExpireFromInit { get; init; }

    public string? SharedRunnersEnabled { get; init; }

    public string? SharedRunnersText { get; init; }

    public string? SignInRestrictions { get; init; }

    public string? SignupEnabled { get; init; }

    public string? SilentModeEnabled { get; init; }

    public string? SlackAppEnabled { get; init; }

    public string? SlackAppId { get; init; }

    public string? SlackAppSecret { get; init; }

    public string? SlackAppSigningSecret { get; init; }

    public string? SlackAppVerificationToken { get; init; }

    public string? SourcegraphEnabled { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this response member as a string; see the type-level remarks.")]
    public string? SourcegraphUrl { get; init; }

    public string? SourcegraphPublicOnly { get; init; }

    public string? SpamCheckEndpointEnabled { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this response member as a string; see the type-level remarks.")]
    public string? SpamCheckEndpointUrl { get; init; }

    public string? SpamCheckApiKey { get; init; }

    public string? TerminalMaxSessionTime { get; init; }

    public string? Terms { get; init; }

    public string? TerraformStateEncryptionEnabled { get; init; }

    public string? ThrottleAuthenticatedApiEnabled { get; init; }

    public string? ThrottleAuthenticatedApiPeriodInSeconds { get; init; }

    public string? ThrottleAuthenticatedApiRequestsPerPeriod { get; init; }

    public string? ThrottleAuthenticatedGitHttpEnabled { get; init; }

    public string? ThrottleAuthenticatedGitHttpPeriodInSeconds { get; init; }

    public string? ThrottleAuthenticatedGitHttpRequestsPerPeriod { get; init; }

    public string? ThrottleAuthenticatedGitLfsEnabled { get; init; }

    public string? ThrottleAuthenticatedGitLfsPeriodInSeconds { get; init; }

    public string? ThrottleAuthenticatedGitLfsRequestsPerPeriod { get; init; }

    public string? ThrottleAuthenticatedWebEnabled { get; init; }

    public string? ThrottleAuthenticatedWebPeriodInSeconds { get; init; }

    public string? ThrottleAuthenticatedWebRequestsPerPeriod { get; init; }

    public string? ThrottleAuthenticatedPackagesApiEnabled { get; init; }

    public string? ThrottleAuthenticatedPackagesApiPeriodInSeconds { get; init; }

    public string? ThrottleAuthenticatedPackagesApiRequestsPerPeriod { get; init; }

    public string? ThrottleAuthenticatedFilesApiEnabled { get; init; }

    public string? ThrottleAuthenticatedFilesApiPeriodInSeconds { get; init; }

    public string? ThrottleAuthenticatedFilesApiRequestsPerPeriod { get; init; }

    public string? ThrottleAuthenticatedDeprecatedApiEnabled { get; init; }

    public string? ThrottleAuthenticatedDeprecatedApiPeriodInSeconds { get; init; }

    public string? ThrottleAuthenticatedDeprecatedApiRequestsPerPeriod { get; init; }

    public string? ThrottleUnauthenticatedApiEnabled { get; init; }

    public string? ThrottleUnauthenticatedApiPeriodInSeconds { get; init; }

    public string? ThrottleUnauthenticatedApiRequestsPerPeriod { get; init; }

    public string? ThrottleUnauthenticatedEnabled { get; init; }

    public string? ThrottleUnauthenticatedPeriodInSeconds { get; init; }

    public string? ThrottleUnauthenticatedRequestsPerPeriod { get; init; }

    public string? ThrottleUnauthenticatedPackagesApiEnabled { get; init; }

    public string? ThrottleUnauthenticatedPackagesApiPeriodInSeconds { get; init; }

    public string? ThrottleUnauthenticatedPackagesApiRequestsPerPeriod { get; init; }

    public string? ThrottleUnauthenticatedFilesApiEnabled { get; init; }

    public string? ThrottleUnauthenticatedFilesApiPeriodInSeconds { get; init; }

    public string? ThrottleUnauthenticatedFilesApiRequestsPerPeriod { get; init; }

    public string? ThrottleUnauthenticatedGitHttpEnabled { get; init; }

    public string? ThrottleUnauthenticatedGitHttpPeriodInSeconds { get; init; }

    public string? ThrottleUnauthenticatedGitHttpRequestsPerPeriod { get; init; }

    public string? ThrottleUnauthenticatedDeprecatedApiEnabled { get; init; }

    public string? ThrottleUnauthenticatedDeprecatedApiPeriodInSeconds { get; init; }

    public string? ThrottleUnauthenticatedDeprecatedApiRequestsPerPeriod { get; init; }

    public string? ThrottleProtectedPathsEnabled { get; init; }

    public string? ThrottleProtectedPathsPeriodInSeconds { get; init; }

    public string? ThrottleProtectedPathsRequestsPerPeriod { get; init; }

    public string? TopLevelGroupCreationEnabled { get; init; }

    public string? ProtectedPathsRaw { get; init; }

    public string? ProtectedPathsForGetRequestRaw { get; init; }

    public string? TimeTrackingLimitToHours { get; init; }

    public string? TwoFactorGracePeriod { get; init; }

    public string? UpdateRunnerVersionsEnabled { get; init; }

    public string? UniqueIpsLimitEnabled { get; init; }

    public string? UniqueIpsLimitPerUser { get; init; }

    public string? UniqueIpsLimitTimeWindow { get; init; }

    public string? UsagePingEnabled { get; init; }

    public string? UsagePingGenerationEnabled { get; init; }

    public string? UsagePingFeaturesEnabled { get; init; }

    public string? UseClickhouseForAnalytics { get; init; }

    public string? UserDefaultExternal { get; init; }

    public string? UserShowAddSshKeyMessage { get; init; }

    public string? UserDefaultInternalRegex { get; init; }

    public string? UserOauthApplications { get; init; }

    public string? VersionCheckEnabled { get; init; }

    public string? DiffMaxPatchBytes { get; init; }

    public string? DiffMaxFiles { get; init; }

    public string? DiffMaxLines { get; init; }

    public string? DiffMaxVersions { get; init; }

    public string? DiffMaxCommits { get; init; }

    public string? CommitEmailHostname { get; init; }

    public string? ProtectedCiVariables { get; init; }

    public string? LocalMarkdownVersion { get; init; }

    public string? MailgunSigningKey { get; init; }

    public string? MailgunEventsEnabled { get; init; }

    public string? SnowplowCollectorHostname { get; init; }

    public string? SnowplowCookieDomain { get; init; }

    public string? SnowplowDatabaseCollectorHostname { get; init; }

    public string? SnowplowEnabled { get; init; }

    public string? SnowplowAppId { get; init; }

    public string? GitlabProductUsageDataEnabled { get; init; }

    public string? PushEventHooksLimit { get; init; }

    public string? PushEventActivitiesLimit { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this response member as a string; see the type-level remarks.")]
    public string? CustomHttpCloneUrlRoot { get; init; }

    public string? SnippetSizeLimit { get; init; }

    public string? DescriptionAndNoteMaxSize { get; init; }

    public string? EmailRestrictionsEnabled { get; init; }

    public string? EmailRestrictions { get; init; }

    public string? IssuesCreateLimit { get; init; }

    public string? ProjectCreateLimit { get; init; }

    public string? GroupCreateLimit { get; init; }

    public string? NotesCreateLimit { get; init; }

    public string? NotesCreateLimitAllowlistRaw { get; init; }

    public string? MembersDeleteLimit { get; init; }

    public string? RawBlobRequestLimit { get; init; }

    public string? RawBlobRequestLimitUnauthenticated { get; init; }

    public string? ProjectImportLimit { get; init; }

    public string? ProjectExportLimit { get; init; }

    public string? ProjectDownloadExportLimit { get; init; }

    public string? GroupImportLimit { get; init; }

    public string? GroupExportLimit { get; init; }

    public string? GroupDownloadExportLimit { get; init; }

    public string? WikiPageMaxContentBytes { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this response member as a string; see the type-level remarks.")]
    public string? WikiAsciidocAllowUriIncludes { get; init; }

    public string? ContainerRegistryDeleteTagsServiceTimeout { get; init; }

    public string? RateLimitingResponseText { get; init; }

    public string? PackageRegistryAllowAnyoneToPullOption { get; init; }

    public string? PackageRegistryCleanupPoliciesWorkerCapacity { get; init; }

    public string? ContainerRegistryExpirationPoliciesWorkerCapacity { get; init; }

    public string? ContainerRegistryCleanupTagsServiceMaxListSize { get; init; }

    public string? KeepLatestArtifact { get; init; }

    public string? WhatsNewVariant { get; init; }

    public string? UserDeactivationEmailsEnabled { get; init; }

    public string? ResourceAccessTokenNotifyInherited { get; init; }

    public string? LockResourceAccessTokenNotifyInherited { get; init; }

    public string? SentryEnabled { get; init; }

    public string? SentryDsn { get; init; }

    public string? SentryClientsideDsn { get; init; }

    public string? SentryEnvironment { get; init; }

    public string? SentryClientsideTracesSampleRate { get; init; }

    public string? SidekiqJobLimiterMode { get; init; }

    public string? SidekiqJobLimiterCompressionThresholdBytes { get; init; }

    public string? SidekiqJobLimiterLimitBytes { get; init; }

    public string? SidekiqTimezoneOverride { get; init; }

    public string? EnableArtifactExternalRedirectWarningPage { get; init; }

    public string? SearchRateLimit { get; init; }

    public string? SearchRateLimitUnauthenticated { get; init; }

    public string? SearchRateLimitAllowlistRaw { get; init; }

    public string? UsersGetByIdLimit { get; init; }

    public string? UsersGetByIdLimitAllowlistRaw { get; init; }

    public string? RunnerTokenExpirationInterval { get; init; }

    public string? GroupRunnerTokenExpirationInterval { get; init; }

    public string? ProjectRunnerTokenExpirationInterval { get; init; }

    public string? PipelineLimitPerProjectUserSha { get; init; }

    public string? PipelineLimitPerUser { get; init; }

    public string? CiLintLimitPerUser { get; init; }

    public string? InvitationFlowEnforcement { get; init; }

    public string? CanCreateGroup { get; init; }

    public string? CanCreateOrganization { get; init; }

    public string? BulkImportConcurrentPipelineBatchLimit { get; init; }

    public string? ConcurrentRelationBatchExportLimit { get; init; }

    public string? ConcurrentRelationExportLimit { get; init; }

    public string? RelationExportBatchSize { get; init; }

    public string? BulkImportEnabled { get; init; }

    public string? BulkImportMaxDownloadFileSize { get; init; }

    public string? SilentAdminExportsEnabled { get; init; }

    public string? AllowContributionMappingToAdmins { get; init; }

    public string? AllowS3CompatibleStorageForOfflineTransfer { get; init; }

    public string? AllowApplicationDefaultCredentialsForOfflineTransfer { get; init; }

    public string? OfflineTransferExportsEnabled { get; init; }

    public string? OfflineTransferImportsEnabled { get; init; }

    public string? AllowRunnerRegistrationToken { get; init; }

    public string? ValidRunnerRegistrars { get; init; }

    public string? UserDefaultsToPrivateProfile { get; init; }

    public string? DeactivationEmailAdditionalText { get; init; }

    public string? ProjectsApiRateLimitUnauthenticated { get; init; }

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

    public string? UsersApiLimitFollowers { get; init; }

    public string? UsersApiLimitFollowing { get; init; }

    public string? UsersApiLimitStatus { get; init; }

    public string? UsersApiLimitSshKeys { get; init; }

    public string? UsersApiLimitSshKey { get; init; }

    public string? UsersApiLimitGpgKeys { get; init; }

    public string? UsersApiLimitGpgKey { get; init; }

    public string? TagsCreateLimit { get; init; }

    public string? WebHookEventResendLimit { get; init; }

    public string? WebHookTestLimit { get; init; }

    public string? GitlabDedicatedInstance { get; init; }

    public string? GitlabEnvironmentToolkitInstance { get; init; }

    public string? CiMaxIncludes { get; init; }

    public string? CiMaxCachesPerJob { get; init; }

    public string? AllowAccountDeletion { get; init; }

    public string? GitlabShellOperationLimit { get; init; }

    public string? NamespaceAggregationScheduleLeaseDurationInSeconds { get; init; }

    public string? CiMaxTotalYamlSizeBytes { get; init; }

    public string? ProjectJobsApiRateLimit { get; init; }

    public string? SecurityTxtContent { get; init; }

    public string? AllowProjectCreationForGuestAndBelow { get; init; }

    public string? DownstreamPipelineTriggerLimitPerProjectUserSha { get; init; }

    public string? AsciidocMaxIncludes { get; init; }

    public string? AiActionApiRateLimit { get; init; }

    public string? CodeSuggestionsApiRateLimit { get; init; }

    public string? RequirePersonalAccessTokenExpiry { get; init; }

    public string? ObservabilityBackendSslVerificationEnabled { get; init; }

    public string? ShowMigrateFromJenkinsBanner { get; init; }

    public string? GlobalSearchSnippetTitlesEnabled { get; init; }

    public string? GlobalSearchUsersEnabled { get; init; }

    public string? GlobalSearchGroupsEnabled { get; init; }

    public string? GlobalSearchWorkItemsEnabled { get; init; }

    public string? GlobalSearchMergeRequestsEnabled { get; init; }

    public string? GlobalSearchBlockAnonymousSearchesEnabled { get; init; }

    public string? EnableLanguageServerRestrictions { get; init; }

    public string? MinimumLanguageServerVersion { get; init; }

    public string? VscodeExtensionMarketplace { get; init; }

    public string? VscodeExtensionMarketplaceEnabled { get; init; }

    public string? VscodeExtensionMarketplaceExtensionHostDomain { get; init; }

    public string? VscodeExtensionMarketplaceSingleOriginFallbackEnabled { get; init; }

    public string? ReindexingMinimumIndexSize { get; init; }

    public string? ReindexingMinimumRelativeBloatSize { get; init; }

    public string? AnonymousSearchesAllowed { get; init; }

    public string? DefaultSearchScope { get; init; }

    public string? GitPushPipelineLimit { get; init; }

    public string? DelayUserAccountSelfDeletion { get; init; }

    public string? ResourceUsageLimits { get; init; }

    public string? RunnerJobsRequestApiLimit { get; init; }

    public string? RunnerJobsPatchTraceApiLimit { get; init; }

    public string? RunnerJobsEndpointsApiLimit { get; init; }

    public string? BackgroundOperationsMaxJobs { get; init; }

    public string? EnforceGranularTokens { get; init; }

    public string? GranularTokensEnforcedAfter { get; init; }

    public int? LoggingFieldSchemaVersion { get; init; }

    public int? LoggingFieldDualEmitTarget { get; init; }

    public string? AutoAcceptAwardedAchievements { get; init; }

    public string? DeactivateDormantUsers { get; init; }

    public string? DeactivateDormantUsersPeriod { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this response member as a string; see the type-level remarks.")]
    public string? NugetSkipMetadataUrlValidation { get; init; }

    public string? HelmMaxPackagesCount { get; init; }

    public string? McpServerEnabled { get; init; }

    public string? DynamicClientRegistrationEnabled { get; init; }

    public string? ActiveContextPauseIndexing { get; init; }

    public string? AllowAllIntegrations { get; init; }

    public string? AllowedIntegrations { get; init; }

    public string? AllowGroupOwnersToManageLdap { get; init; }

    public string? AutomaticPurchasedStorageAllocation { get; init; }

    public string? CheckNamespacePlan { get; init; }

    public string? CiCdCatalogProjectsAllowlist { get; init; }

    public string? CiCdCatalogProjectsAllowlistRaw { get; init; }

    public string? DuoChatExpirationColumn { get; init; }

    public string? DuoChatExpirationDays { get; init; }

    public string? ElasticsearchAwsAccessKey { get; init; }

    public string? ElasticsearchAwsRegion { get; init; }

    public string? ElasticsearchAwsRoleArn { get; init; }

    public string? ElasticsearchAwsSecretAccessKey { get; init; }

    public string? ElasticsearchAws { get; init; }

    public string? ElasticsearchClientAdapter { get; init; }

    public string? ElasticsearchClientRequestTimeout { get; init; }

    public string? ElasticsearchIndexedFieldLengthLimit { get; init; }

    public string? ElasticsearchIndexedFileSizeLimitKb { get; init; }

    public string? ElasticsearchIndexingTimeoutMinutes { get; init; }

    public string? ElasticsearchIndexing { get; init; }

    public string? ElasticsearchRequeueWorkers { get; init; }

    public string? ElasticsearchLimitIndexing { get; init; }

    public string? ElasticsearchWorkerNumberOfShards { get; init; }

    public string? ElasticsearchMaxBulkConcurrency { get; init; }

    public string? ElasticsearchMaxBulkSizeMb { get; init; }

    public string? ElasticsearchMaxCodeIndexingConcurrency { get; init; }

    public string? ElasticsearchNamespaceIds { get; init; }

    public string? ElasticsearchPauseIndexing { get; init; }

    public string? ElasticsearchAdvancedSearchPauseIndexing { get; init; }

    public string? ElasticsearchProjectIds { get; init; }

    public string? ElasticsearchRetryOnFailure { get; init; }

    public string? ElasticsearchReplicas { get; init; }

    public string? ElasticsearchSearch { get; init; }

    public string? ElasticsearchShards { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this response member as a string; see the type-level remarks.")]
    public string? ElasticsearchUrl { get; init; }

    public string? ElasticsearchUsername { get; init; }

    public string? ElasticsearchPassword { get; init; }

    public string? ElasticsearchAnalyzersSmartcnEnabled { get; init; }

    public string? ElasticsearchAnalyzersSmartcnSearch { get; init; }

    public string? ElasticsearchAnalyzersKuromojiEnabled { get; init; }

    public string? ElasticsearchAnalyzersKuromojiSearch { get; init; }

    public string? ElasticsearchCodeScope { get; init; }

    public string? EnforceNamespaceStorageLimit { get; init; }

    public string? GeoNodeAllowedIps { get; init; }

    public string? GeoStatusTimeout { get; init; }

    public string? InstanceLevelAiBetaFeaturesEnabled { get; init; }

    public string? LockMembershipsToLdap { get; init; }

    public string? LockMembershipsToSaml { get; init; }

    public string? LockModelPromptCacheEnabled { get; init; }

    public string? MaxPersonalAccessTokenLifetime { get; init; }

    public string? MaxSshKeyLifetime { get; init; }

    public string? ModelPromptCacheEnabled { get; init; }

    public string? ReceptiveClusterAgentsEnabled { get; init; }

    public string? RepositorySizeLimit { get; init; }

    public string? SearchMaxShardSizeGb { get; init; }

    public string? SearchMaxDocsDenominator { get; init; }

    public string? SearchMinDocsBeforeRollover { get; init; }

    public string? SecretDetectionTokenRevocationEnabled { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this response member as a string; see the type-level remarks.")]
    public string? SecretDetectionTokenRevocationUrl { get; init; }

    public string? SecretDetectionTokenRevocationToken { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this response member as a string; see the type-level remarks.")]
    public string? SecretDetectionRevocationTokenTypesUrl { get; init; }

    public string? SharedRunnersMinutes { get; init; }

    public string? ThrottleIncidentManagementNotificationEnabled { get; init; }

    public string? ThrottleIncidentManagementNotificationPerPeriod { get; init; }

    public string? ThrottleIncidentManagementNotificationPeriodInSeconds { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this response member as a string; see the type-level remarks.")]
    public string? PackageMetadataPurlTypes { get; init; }

    public string? ProductAnalyticsEnabled { get; init; }

    public string? ProductAnalyticsDataCollectorHost { get; init; }

    public string? ProductAnalyticsConfiguratorConnectionString { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this response member as a string; see the type-level remarks.")]
    public string? CubeApiBaseUrl { get; init; }

    public string? CubeApiKey { get; init; }

    public string? SecurityPolicyGlobalGroupApproversEnabled { get; init; }

    public string? SecurityApprovalPoliciesLimit { get; init; }

    public string? DuoFeaturesEnabled { get; init; }

    public string? LockDuoFeaturesEnabled { get; init; }

    public string? DuoAvailability { get; init; }

    public string? DuoAgentPlatformEnabled { get; init; }

    public string? DuoCliEnabled { get; init; }

    public bool? AiAuditEventsStreamingEnabled { get; init; }

    public string? DuoNamespaceAccessRules { get; init; }

    public string? DuoRemoteFlowsEnabled { get; init; }

    public string? DuoRemoteFlowsAvailability { get; init; }

    public string? DuoFoundationalFlowsEnabled { get; init; }

    public string? DuoFoundationalFlowsAvailability { get; init; }

    public bool? DuoCustomAgentsEnabled { get; init; }

    public string? DuoCustomAgentsAvailability { get; init; }

    public bool? DuoCustomFlowsEnabled { get; init; }

    public string? DuoCustomFlowsAvailability { get; init; }

    public bool? DuoExternalAgentsEnabled { get; init; }

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

    public string? ScanExecutionPoliciesActionLimit { get; init; }

    public string? ScanExecutionPoliciesScheduleLimit { get; init; }

    public string? PipelineExecutionPoliciesPerConfigurationLimit { get; init; }

    public string? ScanExecutionPoliciesPerConfigurationLimit { get; init; }

    public string? VulnerabilityManagementPoliciesPerConfigurationLimit { get; init; }

    public string? DependencyFirewallPoliciesPerConfigurationLimit { get; init; }

    public string? PolicyStoreExperimentEnabled { get; init; }

    public string? SecretDetectionServiceAuthToken { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this response member as a string; see the type-level remarks.")]
    public string? SecretDetectionServiceUrl { get; init; }

    public string? FetchObservabilityAlertsFromCloud { get; init; }

    public string? GlobalSearchCodeEnabled { get; init; }

    public string? GlobalSearchCommitsEnabled { get; init; }

    public string? GlobalSearchWikiEnabled { get; init; }

    public string? GlobalSearchLimitedIndexingEnabled { get; init; }

    public string? ElasticMigrationWorkerEnabled { get; init; }

    public string? EnforcePiplCompliance { get; init; }

    public string? DisplayGitlabCreditsUserData { get; init; }

    public bool? UseNatsForAuditStreaming { get; init; }

    public string? ProjectSecretsLimit { get; init; }

    public string? GroupSecretsLimit { get; init; }

    public string? SecurityMrReportCacheLifetimeMinutes { get; init; }

    public string? SecurityScanStaleAfterDays { get; init; }

    public string? VacProjectIdsRaw { get; init; }

    public string? EnableMemberPromotionManagement { get; init; }

    public string? ExternalAuthClientCert { get; init; }

    public string? ExternalAuthClientKey { get; init; }

    public string? ExternalAuthClientKeyPass { get; init; }

    public string? ExternalAuthorizationServiceDefaultLabel { get; init; }

    public string? ExternalAuthorizationServiceEnabled { get; init; }

    public string? ExternalAuthorizationServiceTimeout { get; init; }

    [SuppressMessage("Design", "CA1056",
        Justification = "GitLab 19.4 declares this response member as a string; see the type-level remarks.")]
    public string? ExternalAuthorizationServiceUrl { get; init; }

    public string? AllowDeployTokensAndKeysWithExternalAuthn { get; init; }

    public string? ThrottleUnauthenticatedWebEnabled { get; init; }

    public string? ThrottleUnauthenticatedWebPeriodInSeconds { get; init; }

    public string? ThrottleUnauthenticatedWebRequestsPerPeriod { get; init; }

    public string? PasswordAuthenticationEnabled { get; init; }

    public string? SigninEnabled { get; init; }

    public string? AllowLocalRequestsFromHooksAndServices { get; init; }

    public string? AssetProxyWhitelist { get; init; }

    public string? HousekeepingBitmapsEnabled { get; init; }

    public string? RepositoryStoragesWeighted { get; init; }

    public string? GitlabProductUsageDataSource { get; init; }

    public string? ContainerRegistryImportMaxTagsCount { get; init; }

    public string? ContainerRegistryImportMaxRetries { get; init; }

    public string? ContainerRegistryImportStartMaxRetries { get; init; }

    public string? ContainerRegistryImportMaxStepDuration { get; init; }

    public string? ContainerRegistryPreImportTagsRate { get; init; }

    public string? ContainerRegistryPreImportTimeout { get; init; }

    public string? ContainerRegistryImportTimeout { get; init; }

    public string? ContainerRegistryImportTargetPlan { get; init; }

    public string? ContainerRegistryImportCreatedBefore { get; init; }

    public string? MirrorMaxCapacity { get; init; }

    public string? MirrorMaxDelay { get; init; }

    public string? MirrorCapacityThreshold { get; init; }

    public string? DisableOverridingApproversPerMergeRequest { get; init; }

    public string? PreventMergeRequestsAuthorApproval { get; init; }

    public string? PreventMergeRequestsCommittersApproval { get; init; }

    public string? PasswordNumberRequired { get; init; }

    public string? PasswordSymbolRequired { get; init; }

    public string? PasswordUppercaseRequired { get; init; }

    public string? PasswordLowercaseRequired { get; init; }

    public string? EmailAdditionalText { get; init; }

    public string? FileTemplateProjectId { get; init; }

    public string? DefaultProjectDeletionProtection { get; init; }

    public string? DisablePersonalAccessTokens { get; init; }

    public string? UpdatingNameDisabledForUsers { get; init; }

    public string? MavenPackageRequestsForwarding { get; init; }

    public string? NpmPackageRequestsForwarding { get; init; }

    public string? VirtualRegistriesEndpointsApiLimit { get; init; }

    public string? AuditEventsApiLimit { get; init; }

    public string? DependencyScanningSbomScanApiUploadLimit { get; init; }

    public string? DependencyScanningSbomScanApiDownloadLimit { get; init; }

    public string? SecretPushProtectionAvailable { get; init; }

    public string? PreReceiveSecretDetectionEnabled { get; init; }

    public string? PypiPackageRequestsForwarding { get; init; }

    public string? RubygemsPackageRequestsForwarding { get; init; }

    public string? GroupOwnersCanManageDefaultBranchProtection { get; init; }

    public string? MaintenanceMode { get; init; }

    public string? MaintenanceModeMessage { get; init; }

    public string? ServiceAccessTokensExpirationEnforced { get; init; }

    public string? GitTwoFactorSessionExpiry { get; init; }

    public string? MaxNumberOfRepositoryDownloads { get; init; }

    public string? MaxNumberOfRepositoryDownloadsWithinTimePeriod { get; init; }

    public string? GitRateLimitUsersAllowlist { get; init; }

    public string? GitRateLimitUsersAlertlist { get; init; }

    public string? AutoBanUserOnExcessiveProjectsDownload { get; init; }

    public string? DeleteUnconfirmedUsers { get; init; }

    public string? UnconfirmedUsersDeleteAfterDays { get; init; }

    public string? MakeProfilePrivate { get; init; }

    public bool? LockDuoCustomAgentsEnabled { get; init; }

    public bool? LockDuoCustomFlowsEnabled { get; init; }

    public bool? LockDuoExternalAgentsEnabled { get; init; }

    public string? DuoWorkflowsDefaultImageRegistry { get; init; }

    public string? DuoTemplateProjectId { get; init; }

    public string? CiTelemetryOtelEndpoint { get; init; }

    public string? CiJobTelemetrySamplingRate { get; init; }

    public string? DisabledDirectCodeSuggestions { get; init; }

    public string? AllowTopLevelGroupOwnersToCreateServiceAccounts { get; init; }

    public string? AutoDuoCodeReviewEnabled { get; init; }

    public IReadOnlyList<GitLabElasticsearchIndexSetting>? ElasticsearchIndexSettings { get; init; }

    public bool? BuiltInProjectTemplatesEnabled { get; init; }

    public bool? LockBuiltInProjectTemplatesEnabled { get; init; }
}