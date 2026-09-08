namespace GitLab.Client.Abstractions;

/// <summary>
///     Root aggregate exposing every GitLab REST API resource area as a typed client. This interface is
///     the library's table of contents and its public NuGet contract, so it stays hand-written: a
///     resource appearing on or vanishing from the public surface must show up in a reviewable diff.
///     <para>
///         Its implementation is generated (<c>GitLabClientWiringGenerator</c>) from the
///         <c>[GenerateClientLayers]</c> attribute on each <c>I&lt;Resource&gt;Repository</c>, as
///         explicit interface implementations. That makes the compiler enforce the pairing in both
///         directions: a property declared here with no attributed repository behind it is CS0535, and an
///         attributed repository with no property here is CS0539 naming the exact member. Adding a
///         resource therefore means adding one line to this file - and nothing to
///         <c>ServiceCollectionExtensions</c> or the root client.
///     </para>
///     <para>
///         Every resource client is also independently injectable by its own interface, so code that
///         needs one area can depend on <c>I&lt;Resource&gt;Client</c> directly instead of on this.
///     </para>
/// </summary>
public interface IGitLabClient
{
    /// <summary>Projects (<c>/projects</c>).</summary>
    IProjectsClient Projects { get; }

    /// <summary>Groups and subgroups (<c>/groups</c>).</summary>
    IGroupsClient Groups { get; }

    /// <summary>Users (<c>/users</c>).</summary>
    IUsersClient Users { get; }

    /// <summary>Repository branches (<c>/projects/:id/repository/branches</c>).</summary>
    IBranchesClient Branches { get; }

    /// <summary>Repository commits (<c>/projects/:id/repository/commits</c>).</summary>
    ICommitsClient Commits { get; }

    /// <summary>Issues (<c>/projects/:id/issues</c>).</summary>
    IIssuesClient Issues { get; }

    /// <summary>Merge requests (<c>/projects/:id/merge_requests</c>).</summary>
    IMergeRequestsClient MergeRequests { get; }

    /// <summary>Labels (<c>/projects/:id/labels</c>).</summary>
    ILabelsClient Labels { get; }

    /// <summary>Milestones (<c>/projects/:id/milestones</c>).</summary>
    IMilestonesClient Milestones { get; }

    /// <summary>Repository tags (<c>/projects/:id/repository/tags</c>).</summary>
    ITagsClient Tags { get; }

    /// <summary>CI/CD pipelines (<c>/projects/:id/pipelines</c>).</summary>
    IPipelinesClient Pipelines { get; }

    /// <summary>CI/CD jobs (<c>/projects/:id/jobs</c>).</summary>
    IJobsClient Jobs { get; }

    /// <summary>Repository files (<c>/projects/:id/repository/files</c>).</summary>
    IRepositoryFilesClient RepositoryFiles { get; }

    /// <summary>Releases (<c>/projects/:id/releases</c>).</summary>
    IReleasesClient Releases { get; }

    /// <summary>Protected branches (<c>/projects/:id/protected_branches</c>).</summary>
    IProtectedBranchesClient ProtectedBranches { get; }

    /// <summary>Environments (<c>/projects/:id/environments</c>).</summary>
    IEnvironmentsClient Environments { get; }

    /// <summary>Notes and comments (<c>/projects/:id/:noteable/:iid/notes</c>).</summary>
    INotesClient Notes { get; }

    /// <summary>Project members (<c>/projects/:id/members</c>).</summary>
    IMembersClient Members { get; }

    /// <summary>Project webhooks (<c>/projects/:id/hooks</c>).</summary>
    IProjectHooksClient ProjectHooks { get; }

    /// <summary>Threaded discussions (<c>/projects/:id/:noteable/:id/discussions</c>).</summary>
    IDiscussionsClient Discussions { get; }

    /// <summary>Pending review comments (<c>/projects/:id/merge_requests/:merge_request_iid/draft_notes</c>).</summary>
    IDraftNotesClient DraftNotes { get; }

    /// <summary>Applying code suggestions (<c>/suggestions</c>).</summary>
    ISuggestionsClient Suggestions { get; }

    /// <summary>Emoji reactions on issues, merge requests and their notes (<c>.../award_emoji</c>).</summary>
    IAwardEmojiClient AwardEmoji { get; }

    /// <summary>Subscribing to issues, merge requests and labels (<c>.../subscribe</c>).</summary>
    IResourceSubscriptionsClient ResourceSubscriptions { get; }

    /// <summary>The authenticated user's to-do items (<c>/todos</c>).</summary>
    ITodosClient Todos { get; }

    /// <summary>Iterations (<c>/groups/:id/iterations</c>, <c>/projects/:id/iterations</c>).</summary>
    IIterationsClient Iterations { get; }

    /// <summary>Issue boards and their lists (<c>/projects/:id/boards</c>).</summary>
    IBoardsClient Boards { get; }

    /// <summary>Issue and merge-request resource events (<c>/projects/:id/issues/:iid/resource_*_events</c>).</summary>
    IResourceEventsClient ResourceEvents { get; }

    /// <summary>CI/CD variables (<c>/projects/:id/variables</c>, <c>/groups/:id/variables</c>).</summary>
    IVariablesClient Variables { get; }

    /// <summary>Pipeline trigger tokens (<c>/projects/:id/triggers</c>).</summary>
    ITriggersClient Triggers { get; }

    /// <summary>CI/CD configuration validation (<c>/projects/:id/ci/lint</c>).</summary>
    ICiLintClient CiLint { get; }

    /// <summary>Pipeline schedules (<c>/projects/:id/pipeline_schedules</c>).</summary>
    IPipelineSchedulesClient PipelineSchedules { get; }

    /// <summary>CI/CD runners (<c>/runners</c>).</summary>
    IRunnersClient Runners { get; }

    /// <summary>Deployments (<c>/projects/:id/deployments</c>).</summary>
    IDeploymentsClient Deployments { get; }

    /// <summary>Deploy keys (<c>/projects/:id/deploy_keys</c>).</summary>
    IDeployKeysClient DeployKeys { get; }

    /// <summary>Commit statuses (<c>/projects/:id/repository/commits/:sha/statuses</c>).</summary>
    ICommitStatusesClient CommitStatuses { get; }

    /// <summary>Repository browsing, comparison and metrics (<c>/projects/:id/repository</c>).</summary>
    IRepositoriesClient Repositories { get; }

    /// <summary>Instance, group and project search (<c>/search</c>).</summary>
    ISearchClient Search { get; }

    /// <summary>Project and group wikis (<c>/projects/:id/wikis</c>, <c>/groups/:id/wikis</c>).</summary>
    IWikisClient Wikis { get; }

    /// <summary>Project and group badges (<c>/projects/:id/badges</c>, <c>/groups/:id/badges</c>).</summary>
    IBadgesClient Badges { get; }

    /// <summary>Activity feeds (<c>/events</c>, <c>/projects/:id/events</c>, <c>/users/:id/events</c>).</summary>
    IEventsClient Events { get; }

    /// <summary>Protected tags (<c>/projects/:id/protected_tags</c>).</summary>
    IProtectedTagsClient ProtectedTags { get; }

    /// <summary>Push mirrors (<c>/projects/:id/remote_mirrors</c>).</summary>
    IRemoteMirrorsClient RemoteMirrors { get; }

    /// <summary>
    ///     Project and group access tokens (<c>/projects/:id/access_tokens</c>,
    ///     <c>/groups/:id/access_tokens</c>).
    /// </summary>
    IAccessTokensClient AccessTokens { get; }

    /// <summary>Personal access tokens (<c>/personal_access_tokens</c>).</summary>
    IPersonalAccessTokensClient PersonalAccessTokens { get; }

    /// <summary>User SSH keys (<c>/user/keys</c>, <c>/users/:id/keys</c>).</summary>
    ISshKeysClient SshKeys { get; }

    /// <summary>Merge request approvals (<c>/projects/:id/merge_requests/:iid/approvals</c>).</summary>
    IMergeRequestApprovalsClient MergeRequestApprovals { get; }

    /// <summary>Project and group approval rules (<c>/projects/:id/approval_rules</c>, <c>/groups/:id/approval_rules</c>).</summary>
    IApprovalRulesClient ApprovalRules { get; }

    /// <summary>External status checks (<c>/projects/:id/external_status_checks</c>).</summary>
    IExternalStatusChecksClient ExternalStatusChecks { get; }

    /// <summary>Project and group access requests (<c>/projects/:id/access_requests</c>).</summary>
    IAccessRequestsClient AccessRequests { get; }

    /// <summary>Pending project and group invitations (<c>/projects/:id/invitations</c>).</summary>
    IInvitationsClient Invitations { get; }

    /// <summary>Protected environments (<c>/projects/:id/protected_environments</c>).</summary>
    IProtectedEnvironmentsClient ProtectedEnvironments { get; }

    /// <summary>User GPG keys (<c>/user/gpg_keys</c>, <c>/users/:id/gpg_keys</c>).</summary>
    IGpgKeysClient GpgKeys { get; }

    /// <summary>Deploy freeze periods (<c>/projects/:id/freeze_periods</c>).</summary>
    IFreezePeriodsClient FreezePeriods { get; }

    /// <summary>CI resource groups (<c>/projects/:id/resource_groups</c>).</summary>
    IResourceGroupsClient ResourceGroups { get; }

    /// <summary>Merge trains (<c>/projects/:id/merge_trains</c>).</summary>
    IMergeTrainsClient MergeTrains { get; }

    /// <summary>CI secure files (<c>/projects/:id/secure_files</c>).</summary>
    ISecureFilesClient SecureFiles { get; }

    /// <summary>
    ///     Runner controllers and their tokens (<c>/runner_controllers</c>,
    ///     <c>/runner_controllers/:id/scopes</c>, <c>/runner_controllers/:id/tokens</c>).
    /// </summary>
    IRunnerControllersClient RunnerControllers { get; }

    /// <summary>Group webhooks (<c>/groups/:id/hooks</c>).</summary>
    IGroupHooksClient GroupHooks { get; }

    /// <summary>Instance-level system hooks (<c>/hooks</c>). Requires instance administrator rights.</summary>
    ISystemHooksClient SystemHooks { get; }

    /// <summary>
    ///     Repository storage moves for projects, groups and snippets
    ///     (<c>/project_repository_storage_moves</c>, <c>/group_repository_storage_moves</c>,
    ///     <c>/snippet_repository_storage_moves</c> and their entity-scoped forms). Requires instance
    ///     administrator rights.
    /// </summary>
    IStorageMovesClient StorageMoves { get; }

    /// <summary>
    ///     Service accounts (<c>/service_accounts</c>, <c>/groups/:id/service_accounts</c>,
    ///     <c>/projects/:id/service_accounts</c>).
    /// </summary>
    IServiceAccountsClient ServiceAccounts { get; }

    /// <summary>
    ///     Custom member and admin roles (<c>/member_roles</c>, <c>/groups/:id/member_roles</c>,
    ///     <c>/admin_member_roles</c>).
    /// </summary>
    IMemberRolesClient MemberRoles { get; }

    /// <summary>
    ///     The authenticated user's own account (<c>/user/activities</c>, <c>/user/emails</c>,
    ///     <c>/user/preferences</c>, <c>/user/status</c>, <c>/user/support_pin</c>, <c>/user/runners</c>,
    ///     <c>/user/avatar</c>).
    /// </summary>
    ICurrentUserClient CurrentUser { get; }

    /// <summary>
    ///     Avatar lookup and download (<c>/avatar</c>, <c>/projects/:id/avatar</c>,
    ///     <c>/groups/:id/avatar</c>).
    /// </summary>
    IAvatarsClient Avatars { get; }

    /// <summary>
    ///     Personal and project snippets (<c>/snippets</c>, <c>/projects/:id/snippets</c>).
    /// </summary>
    ISnippetsClient Snippets { get; }

    /// <summary>Project markdown attachment uploads (<c>/projects/:id/uploads</c>).</summary>
    IProjectUploadsClient ProjectUploads { get; }

    /// <summary>
    ///     Terraform remote state (<c>/projects/:id/terraform/state</c>) and its protection rules
    ///     (<c>/projects/:id/terraform/state_protection_rules</c>).
    /// </summary>
    ITerraformStatesClient TerraformStates { get; }

    /// <summary>External AI coding agent identities, sessions and audit events (<c>/projects/:id/ai_agent</c>).</summary>
    IProjectAiAgentsClient ProjectAiAgents { get; }

    /// <summary>
    ///     Job artifacts (<c>/projects/:id/jobs/:job_id/artifacts</c>,
    ///     <c>/projects/:id/jobs/artifacts/:ref_name</c>, <c>/projects/:id/artifacts</c>).
    /// </summary>
    IJobArtifactsClient JobArtifacts { get; }

    /// <summary>
    ///     Instance licence activation (<c>/license</c>, <c>/licenses</c>) and project software licence
    ///     policies (<c>/projects/:id/managed_licenses</c>).
    /// </summary>
    ILicensesClient Licenses { get; }

    /// <summary>
    ///     The software supply chain: dependency lists and exports (<c>/projects/:id/dependencies</c>,
    ///     <c>/dependency_list_exports</c>), SBOM scans (<c>/jobs/:id/sbom_scans</c>), provenance
    ///     attestations (<c>/projects/:id/attestations</c>) and the real-time SAST file scan
    ///     (<c>/projects/:id/security_scans/sast</c>).
    /// </summary>
    IDependenciesClient Dependencies { get; }

    /// <summary>A group's dependency proxy cache (<c>/groups/:id/dependency_proxy/cache</c>).</summary>
    IDependencyProxyClient DependencyProxy { get; }

    /// <summary>
    ///     The GitLab app for Jira Cloud - namespace subscriptions and Forge installation
    ///     (<c>/integrations/jira_connect/subscriptions</c>, <c>/integrations/jira_forge/subscriptions</c>,
    ///     <c>/integrations/jira_forge/installation</c>). Mostly called by the Atlassian app itself, not by
    ///     an ordinary API consumer; this is not the <c>jira</c> project integration.
    /// </summary>
    IJiraConnectClient JiraConnect { get; }

    /// <summary>
    ///     The registered-model half of the GitLab model registry
    ///     (<c>/projects/:id/ml/mlflow/api/2.0/mlflow/registered-models</c>).
    /// </summary>
    IMlModelsClient MlModels { get; }

    /// <summary>
    ///     GitLab Pages settings, custom domains and access checks (<c>/projects/:id/pages</c>,
    ///     <c>/projects/:id/pages/domains</c>, <c>/pages/domains</c>).
    /// </summary>
    IPagesClient Pages { get; }

    /// <summary>
    ///     Administrator-defined key/value metadata on users, groups and projects
    ///     (<c>/users/:id/custom_attributes</c>, <c>/groups/:id/custom_attributes</c>,
    ///     <c>/projects/:id/custom_attributes</c>).
    /// </summary>
    ICustomAttributesClient CustomAttributes { get; }

    /// <summary>The instance-wide project topic vocabulary, including merge and avatar (<c>/topics</c>).</summary>
    ITopicsClient Topics { get; }

    /// <summary>Alternative Git-facing names for a project (<c>/project_aliases</c>).</summary>
    IProjectAliasesClient ProjectAliases { get; }

    /// <summary>
    ///     Deploy tokens - the username/password credentials for registry and repository access
    ///     (<c>/deploy_tokens</c>, <c>/projects/:id/deploy_tokens</c>, <c>/groups/:id/deploy_tokens</c>).
    ///     Not deploy keys: those are on <see cref="IDeployKeysClient" />.
    /// </summary>
    IDeployTokensClient DeployTokens { get; }

    /// <summary>
    ///     Group SAML and SCIM provider identities (<c>/groups/:id/saml/identities</c>,
    ///     <c>/groups/:id/saml/:uid</c>, <c>/groups/:id/scim/identities</c>, <c>/groups/:id/scim/:uid</c>).
    /// </summary>
    IProviderIdentitiesClient ProviderIdentities { get; }

    /// <summary>
    ///     SAML group links, mapping a SAML provider group onto a role in a GitLab group
    ///     (<c>/groups/:id/saml_group_links</c>).
    /// </summary>
    ISamlGroupLinksClient SamlGroupLinks { get; }

    /// <summary>
    ///     Third-party integrations on a project or group
    ///     (<c>/projects/:id/integrations</c>, <c>/groups/:id/integrations</c>), plus the Slack and
    ///     Mattermost callback endpoints (<c>/integrations/slack/*</c>, <c>/slack/trigger</c>).
    /// </summary>
    IIntegrationsClient Integrations { get; }

    /// <summary>
    ///     Instance file templates - Dockerfiles, <c>.gitignore</c>s, CI/CD YAML and open source licenses
    ///     (<c>/templates/dockerfiles</c>, <c>/templates/gitignores</c>, <c>/templates/gitlab_ci_ymls</c>,
    ///     <c>/templates/licenses</c>).
    /// </summary>
    ITemplatesClient Templates { get; }

    /// <summary>
    ///     Visual Studio Code Settings Sync, the server half of the editor's settings-sync protocol
    ///     (<c>/vscode/settings_sync/...</c>). For editor integrations; see <see cref="IVsCodeClient" />.
    /// </summary>
    IVsCodeClient VsCode { get; }

    /// <summary>
    ///     GitLab Duo and the rest of the AI surface - Duo Chat (<c>/chat/completions</c>), Git command
    ///     generation (<c>/ai/llm/git_command</c>), third-party agent access
    ///     (<c>/ai/third_party_agents/direct_access</c>), Duo Code Review evaluations
    ///     (<c>/duo_code_review/evaluations</c>), Code Suggestions (<c>/code_suggestions</c>), GitLab Query
    ///     Language (<c>/glql</c>) and the Model Context Protocol server (<c>/mcp</c>).
    /// </summary>
    IDuoClient Duo { get; }

    /// <summary>Markdown rendering (<c>/markdown</c>).</summary>
    IMarkdownClient Markdown { get; }

    /// <summary>
    ///     A project's own feature flags (<c>/projects/:id/feature_flags</c>), their minimum role
    ///     (<c>/projects/:id/feature_flags_settings</c>) and their user lists
    ///     (<c>/projects/:id/feature_flags_user_lists</c>).
    /// </summary>
    IFeatureFlagsClient FeatureFlags { get; }

    /// <summary>
    ///     Instance-wide Flipper feature flags (<c>/features</c>, <c>/features/definitions</c>) and the
    ///     Unleash client endpoints (<c>/feature_flags/unleash/:project_id</c>).
    /// </summary>
    IFeaturesClient Features { get; }

    /// <summary>
    ///     GitLab Duo Agent Platform flows, their checkpoints, events and callback hooks
    ///     (<c>/ai/duo_workflows</c>). Experimental: GitLab may change this area without deprecation.
    /// </summary>
    IDuoWorkflowsClient DuoWorkflows { get; }

    /// <summary>
    ///     MLflow-compatible experiment tracking - experiments, runs (GitLab candidates), metrics,
    ///     parameters, model versions and artifacts
    ///     (<c>/projects/:id/ml/mlflow/api/2.0/...</c>).
    /// </summary>
    IMlExperimentsClient MlExperiments { get; }

    /// <summary>
    ///     Group file export and import (<c>/groups/import</c>, <c>/groups/:id/export</c>) and the
    ///     per-relation export (<c>/groups/:id/export_relations</c>).
    /// </summary>
    IGroupImportClient GroupImport { get; }

    /// <summary>
    ///     Direct-transfer and offline-transfer migrations (<c>/bulk_imports</c>,
    ///     <c>/bulk_imports/:id/entities</c>, <c>/offline_exports</c>, <c>/offline_imports</c>) plus the
    ///     GitHub gist import (<c>/import/github/gists</c>).
    /// </summary>
    IBulkImportsClient BulkImports { get; }

    /// <summary>
    ///     Project export and import (<c>/projects/:id/export</c>, <c>/projects/:id/export_relations</c>,
    ///     <c>/projects/:id/import</c>, <c>/projects/import</c>, <c>/projects/remote-import</c>,
    ///     <c>/projects/remote-import-s3</c>, <c>/import/github</c>, <c>/import/bitbucket</c>), plus the
    ///     file templates a project exposes (<c>/projects/:id/templates/:type</c>).
    /// </summary>
    IProjectImportClient ProjectImport { get; }

    /// <summary>
    ///     GitLab's Debian package registry - distributions and the APT-compatible file tree under them
    ///     (<c>/projects/:id/debian_distributions</c>, <c>/groups/:id/-/debian_distributions</c>,
    ///     <c>/projects/:id/packages/debian</c>, <c>/groups/:id/-/packages/debian</c>).
    /// </summary>
    IPackagesDebianClient PackagesDebian { get; }

    /// <summary>
    ///     GitLab's Cargo crate registry (<c>/projects/:id/packages/cargo/...</c>) - the sparse-index
    ///     protocol Cargo itself speaks, plus <c>.crate</c> downloads.
    /// </summary>
    IPackagesCargoClient PackagesCargo { get; }

    /// <summary>
    ///     GitLab's Composer package registry (<c>/group/:id/-/packages/composer/...</c>,
    ///     <c>/projects/:id/packages/composer/...</c>) - the group-scoped metadata endpoints Composer
    ///     itself reads, plus publishing and archive downloads.
    /// </summary>
    IPackagesComposerClient PackagesComposer { get; }

    /// <summary>
    ///     GitLab's Helm chart registry (<c>/projects/:id/packages/helm/...</c>) - uploading a chart to a
    ///     named channel and downloading a channel's charts and index.
    /// </summary>
    IPackagesHelmClient PackagesHelm { get; }

    /// <summary>
    ///     GitLab's PyPI package registry (<c>/groups/:id/-/packages/pypi/...</c>,
    ///     <c>/projects/:id/packages/pypi/...</c>) - the Simple-index endpoints <c>pip</c> reads, plus
    ///     upload and file download.
    /// </summary>
    IPackagesPyPiClient PackagesPyPi { get; }

    /// <summary>
    ///     GitLab's RPM package registry (<c>/projects/:id/packages/rpm/...</c>) - uploading a package
    ///     and downloading its files and the generated repository metadata.
    /// </summary>
    IPackagesRpmClient PackagesRpm { get; }

    /// <summary>
    ///     GitLab's RubyGems registry (<c>/projects/:id/packages/rubygems/...</c>) - the endpoints
    ///     <c>gem</c> and <c>bundler</c> speak directly.
    /// </summary>
    IPackagesRubyGemsClient PackagesRubyGems { get; }

    /// <summary>
    ///     Instance-administrator-only ActiveContext admin API (<c>/admin/active_context/...</c>) - the
    ///     semantic-search and knowledge-graph indexing pipeline's backend connections, collections and
    ///     dead-letter queue.
    /// </summary>
    IActiveContextClient ActiveContext { get; }

    /// <summary>
    ///     Instance-administrator-only Zoekt-backed instance code search admin API
    ///     (<c>/admin/zoekt/...</c>) - search nodes ("shards") and which namespaces are indexed on each.
    /// </summary>
    ICodeSearchClient CodeSearch { get; }

    /// <summary>
    ///     Instance-administrator-only Knowledge Graph admin API
    ///     (<c>/admin/knowledge_graph/namespaces</c>) - which namespaces have it enabled.
    /// </summary>
    IKnowledgeGraphClient KnowledgeGraph { get; }

    /// <summary>
    ///     Reporting endpoints: group activity counts (<c>/analytics/group_activity/*</c>), a project's
    ///     deployment frequency (<c>/projects/:id/analytics/deployment_frequency</c>), code review
    ///     analytics (<c>/analytics/code_review</c>) and DORA metrics (<c>/groups/:id/dora/metrics</c>,
    ///     <c>/projects/:id/dora/metrics</c>).
    /// </summary>
    IAnalyticsClient Analytics { get; }

    /// <summary>
    ///     Service Ping / usage telemetry (<c>/usage_data/*</c>) and the internal event-tracking API it
    ///     shares a route prefix with.
    /// </summary>
    IUsageDataClient UsageData { get; }

    /// <summary>
    ///     The instance-wide audit log (<c>/audit_events</c>, administrators only) and a single group's
    ///     audit log (<c>/groups/:id/audit_events</c>).
    /// </summary>
    IAuditEventsClient AuditEvents { get; }

    /// <summary>
    ///     Instance-wide compliance policy settings (<c>/admin/security/compliance_policy_settings</c>)
    ///     and a project's external compliance control status
    ///     (<c>/projects/:id/compliance_external_controls/:control_id/status</c>).
    /// </summary>
    IComplianceSettingsClient ComplianceSettings { get; }

    /// <summary>
    ///     A group's credentials inventory (<c>/groups/:id/manage/...</c>) - the personal access tokens,
    ///     group/project access tokens and SSH keys belonging to its enterprise users, for compliance
    ///     review, revocation and rotation.
    /// </summary>
    IGroupCredentialsInventoryClient GroupCredentialsInventory { get; }

    /// <summary>
    ///     Instance-wide broadcast banners and notifications (<c>/broadcast_messages</c>).
    ///     Administrator-only.
    /// </summary>
    IBroadcastMessagesClient BroadcastMessages { get; }

    /// <summary>
    ///     A project's error tracking integration settings and client keys
    ///     (<c>/projects/:id/error_tracking</c>).
    /// </summary>
    IErrorTrackingClient ErrorTracking { get; }

    /// <summary>The calling user's registered mobile push notification devices (<c>/user/push_subscriptions</c>).</summary>
    IMobilePushSubscriptionsClient MobilePushSubscriptions { get; }

    /// <summary>
    ///     The webhook a CD orchestrator (a Starlark workflow run by AutoFlow) calls back into to report
    ///     flow-graph progress for a CD rollout (<c>/rollouts/:id</c>).
    /// </summary>
    IRolloutsClient Rollouts { get; }

    /// <summary>
    ///     Publishing a component project release as a version to the CI/CD catalog (<c>/projects/:id/catalog/publish</c>
    ///     ).
    /// </summary>
    ICiCatalogClient CiCatalog { get; }

    /// <summary>
    ///     The webhook the container registry itself calls back into GitLab on push, delete and other
    ///     registry operations (<c>/container_registry_event/events</c>).
    /// </summary>
    IContainerRegistryClient ContainerRegistry { get; }

    /// <summary>
    ///     A handful of GitLab REST operations that carry no OpenAPI tag and share no natural home with any
    ///     other resource client: the Gitaly-internal object pool RPC (<c>/internal/gitaly/object_pool_members</c>)
    ///     and the Swagger compatible API description endpoints (<c>/swagger_doc</c>).
    /// </summary>
    IInternalClient Internal { get; }

    /// <summary>
    ///     The authenticated user's notification preferences - globally (<c>/notification_settings</c>) or
    ///     scoped to one group (<c>/groups/:id/notification_settings</c>) or project
    ///     (<c>/projects/:id/notification_settings</c>).
    /// </summary>
    INotificationSettingsClient NotificationSettings { get; }

    /// <summary>A/B experiments running on the instance, and cached variant assignments (<c>/experiments</c>).</summary>
    IExperimentsClient Experiments { get; }

    /// <summary>
    ///     Four small, otherwise-unrelated API areas combined into one client - Kubernetes agents allowed
    ///     for a CI/CD job token (<c>/job/allowed_agents</c>), IAM OIDC claims (<c>/iam/userinfo</c>),
    ///     modular-service token exchange (<c>/token_exchange</c>) and a project's Google Cloud
    ///     integration setup scripts (<c>/projects/:id/google_cloud/setup/*</c>).
    /// </summary>
    IPlatformIntegrationsClient PlatformIntegrations { get; }

    /// <summary>
    ///     The Conan package registry (<c>/packages/conan/v1</c> instance-wide,
    ///     <c>/projects/:id/packages/conan/v1</c> and the revision-aware
    ///     <c>/projects/:id/packages/conan/v2</c>) - the endpoints a Conan client talks to when its remote
    ///     points at GitLab.
    /// </summary>
    IPackagesConanClient PackagesConan { get; }

    /// <summary>
    ///     The ecosystem-agnostic "Packages" API area: the Maven and Go package proxies
    ///     (<c>/packages/maven</c>, <c>/projects/:id/packages/go</c>), the generic package format
    ///     (<c>/projects/:id/packages/generic</c>), and format-agnostic package/package-file/pipeline
    ///     management (<c>/projects/:id/packages/:package_id</c>).
    /// </summary>
    IPackagesGenericClient PackagesGeneric { get; }

    /// <summary>
    ///     The NuGet package registry (<c>/projects/:id/packages/nuget</c>,
    ///     <c>/groups/:id/-/packages/nuget</c>) - the V3 protocol's service index, metadata and search
    ///     services, plus the legacy V2 OData feed and the symbol server endpoint.
    /// </summary>
    IPackagesNuGetClient PackagesNuGet { get; }

    /// <summary>
    ///     The npm registry protocol at project, group and instance scope
    ///     (<c>/projects/:id/packages/npm/...</c>, <c>/groups/:id/-/packages/npm/...</c>,
    ///     <c>/packages/npm/...</c>) - package metadata, dist-tags, tarball publish/download and the npm
    ///     security-audit proxy endpoints.
    /// </summary>
    IPackagesNpmClient PackagesNpm { get; }

    /// <summary>
    ///     Batched background migrations and batched background operations
    ///     (<c>/admin/batched_background_migrations</c>, <c>/admin/batched_background_operations</c>) -
    ///     the mechanisms Rails uses to run large data or schema changes in small batches over time
    ///     rather than as one blocking database transaction. Instance administrators only.
    /// </summary>
    IBackgroundMigrationsClient BackgroundMigrations { get; }

    /// <summary>
    ///     Data management (<c>/admin/data_management/:model_name</c>) and Database dictionary
    ///     (<c>/databases/:database_name/dictionary/tables</c>,
    ///     <c>/admin/databases/:database_name/dictionary/tables/:table_name</c>) - per-record checksum
    ///     inspection for instance data models, and the generated map of which feature category owns each
    ///     database table. Instance administrators only.
    /// </summary>
    IDataManagementClient DataManagement { get; }

    /// <summary>
    ///     Sidekiq queue draining and background job processing metrics
    ///     (<c>/admin/sidekiq/queues/:queue_name</c>, <c>/sidekiq/queue_metrics</c>,
    ///     <c>/sidekiq/process_metrics</c>, <c>/sidekiq/job_stats</c>, <c>/sidekiq/compound_metrics</c>).
    ///     Instance administrators only.
    /// </summary>
    ISidekiqClient Sidekiq { get; }

    /// <summary>
    ///     Rails schema migration status (<c>/admin/migrations/pending</c>,
    ///     <c>/admin/migrations/:timestamp/mark</c>) - listing migrations the instance has not yet run,
    ///     and marking one applied without running it. Instance administrators only.
    /// </summary>
    IAdminMigrationsClient AdminMigrations { get; }

    /// <summary>
    ///     Instance-admin-managed OAuth application registration, both instance-wide
    ///     (<c>/applications</c>) and per-user (<c>/user/applications</c>).
    /// </summary>
    IApplicationsClient Applications { get; }

    /// <summary>
    ///     Instance-level informational and administrative endpoints: branding
    ///     (<c>/application/appearance</c>), application settings (<c>/application/settings</c>),
    ///     entity statistics (<c>/application/statistics</c>), version metadata (<c>/metadata</c>) and
    ///     billing-plan limits (<c>/application/plan_limits</c>).
    /// </summary>
    IInstanceClient Instance { get; }

    /// <summary>
    ///     Namespaces (<c>/namespaces</c>) - the umbrella GitLab exposes over both groups and personal
    ///     namespaces, plus the internal, license-aware project listing under
    ///     <c>/internal/gitlab_subscriptions/namespaces/:id/projects</c>.
    /// </summary>
    INamespacesClient Namespaces { get; }

    /// <summary>
    ///     Organizations (<c>/organizations</c>) - the newer, still-experimental top-level container
    ///     GitLab is building above groups.
    /// </summary>
    IOrganizationsClient Organizations { get; }

    /// <summary>
    ///     GitLab Workspaces' internal remote-development agent endpoints
    ///     (<c>/internal/agents/agentw/...</c>).
    /// </summary>
    IWorkspacesClient Workspaces { get; }

    /// <summary>
    ///     Alert management metric images - screenshots and dashboard links attached to an alert
    ///     (<c>/projects/:id/alert_management_alerts/:alert_iid/metric_images</c>).
    /// </summary>
    IAlertManagementClient AlertManagement { get; }

    /// <summary>
    ///     The ML Model Registry's file storage (<c>/projects/:id/packages/ml_models/...</c>) - uploading
    ///     and downloading the files attached to a model version. Model and model-version metadata itself
    ///     is managed through <see cref="MlModels" /> instead.
    /// </summary>
    IMlModelPackageFilesClient MlModelPackageFiles { get; }

    /// <summary>A project's pull mirror configuration and status (<c>/projects/:id/mirror/pull</c>).</summary>
    IProjectMirrorsClient ProjectMirrors { get; }

    /// <summary>
    ///     Project package protection rules (<c>/projects/:id/packages/protection/rules</c>) - which
    ///     packages, by name pattern and package format, only members at or above a given role may push
    ///     or delete.
    /// </summary>
    IProjectPackageProtectionRulesClient ProjectPackageProtectionRules { get; }

    /// <summary>
    ///     Project container repository protection rules
    ///     (<c>/projects/:id/registry/protection/repository/rules</c>) - which container image
    ///     repositories, by path pattern, only members at or above a given role may push images to or
    ///     delete images from.
    /// </summary>
    IProjectContainerRegistryProtectionRulesClient ProjectContainerRegistryProtectionRules { get; }

    /// <summary>
    ///     Project container registry protection tag rules
    ///     (<c>/projects/:id/registry/protection/tag/rules</c>) - which container image tags, by name
    ///     pattern, only members at or above a given role may push or delete.
    /// </summary>
    IProjectContainerRegistryProtectionTagRulesClient ProjectContainerRegistryProtectionTagRules { get; }

    /// <summary>
    ///     CI/CD job token access settings for a project (<c>/projects/:id/job_token_scope</c>) - whether
    ///     job tokens are restricted, and the project/group allowlists that restriction exempts.
    /// </summary>
    IJobTokenScopeClient JobTokenScope { get; }

    /// <summary>
    ///     Push rules (<c>/projects/:id/push_rule</c>, <c>/groups/:id/push_rule</c>) - server-side checks
    ///     run against every push, and the group-level defaults applied to new projects.
    /// </summary>
    IPushRulesClient PushRules { get; }

    /// <summary>
    ///     The Terraform Module Registry (<c>/packages/terraform/modules/v1/...</c>,
    ///     <c>/projects/:id/packages/terraform/modules/...</c>) - a different API area from
    ///     <see cref="ITerraformStatesClient" />, which is the Terraform remote-state backend rather than a
    ///     module registry.
    /// </summary>
    IPackagesTerraformModulesClient PackagesTerraformModules { get; }

    /// <summary>
    ///     Artifact attestations (<c>/projects/:id/attestations</c>) - build provenance bundles produced
    ///     for a project's CI/CD pipelines, addressed by their project-scoped internal id (<c>iid</c>).
    /// </summary>
    IAttestationsClient Attestations { get; }

    /// <summary>
    ///     Cluster agents (<c>/projects/:id/cluster_agents</c>) - registrations for the GitLab agent for
    ///     Kubernetes, their authentication tokens, and receptive agents' URL configurations. Distinct
    ///     from the deprecated, certificate-based Kubernetes cluster integration, which this library does
    ///     not implement.
    /// </summary>
    IClusterAgentsClient ClusterAgents { get; }
}