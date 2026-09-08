using System.Text.Json;

using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Tests.Infrastructure;

/// <summary>
///     Covers what <c>[GitLabQuery]</c> generates: the derived wire names, the projection onto
///     <see cref="GitLabRouteBuilder" />, and the guarantee that an enum's query value is the same
///     string System.Text.Json writes for it.
/// </summary>
public sealed class GeneratedQueryProjectionTests
{
    private static readonly string[] LabelsWithASpace = ["bug", "needs review"];

    private static readonly string[] ExcludedLabels = ["wontfix"];

    private static readonly string[] Topics = ["devops", "a,b"];

    private static readonly long[] Iids = [41, 42];

    /// <summary>
    ///     Every name here was checked against the vendored spec's declaration of the corresponding
    ///     <c>GET</c> operation. GitLab answers 200 and ignores a parameter it does not recognise, so
    ///     nothing else in the suite would catch a misspelling. Keyed by options type so that adding a
    ///     resource only ever appends a line, and two records that happen to project to the same string
    ///     do not collide.
    /// </summary>
    private static readonly Dictionary<string, string> ExpectedWireNames = new()
    {
        [nameof(AgentSessionListOptionsQueryExtensions)] =
            "agent_type,status,created_after,created_before,per_page",
        [nameof(AllSnippetListOptionsQueryExtensions)] = "created_after,created_before,repository_storage,per_page",
        [nameof(BranchListOptionsQueryExtensions)] = "search,regex,sort,page,page_token,per_page",
        [nameof(CommitListOptionsQueryExtensions)] = "ref_name,since,until,per_page",
        [nameof(GroupBillableMemberListOptionsQueryExtensions)] = "search,sort,per_page",
        [nameof(GroupLabelGetOptionsQueryExtensions)] =
            "include_ancestor_groups,include_descendant_groups,only_group_labels",
        [nameof(GroupLabelListOptionsQueryExtensions)] =
            "with_counts,include_ancestor_groups,include_descendant_groups,only_group_labels,search,archived,per_page",
        [nameof(GroupListOptionsQueryExtensions)] = "search,visibility,skip_groups,per_page",
        [nameof(GroupProtectedBranchListOptionsQueryExtensions)] = "search,per_page",
        [nameof(GroupReleaseListOptionsQueryExtensions)] = "sort,simple,per_page",
        [nameof(GroupTransferLocationListOptionsQueryExtensions)] = "search,per_page",
        [nameof(GroupUserListOptionsQueryExtensions)] =
            "username,search,active,blocked,created_after,created_before,per_page",
        [nameof(InvitedGroupListOptionsQueryExtensions)] =
            "relation,search,min_access_level,with_custom_attributes,per_page",
        [nameof(IssueListOptionsQueryExtensions)] =
            "state,labels,not[labels],iids,author_id,updated_after,per_page,with_labels_details,closed_by_id,order_by,sort,due_date,issue_type,milestone,milestone_id,search,in,author_username,assignee_id,assignee_username,created_after,created_before,updated_before,not[milestone],not[milestone_id],not[iids],not[author_id],not[author_username],not[assignee_id],not[assignee_username],not[weight],not[iteration_id],not[iteration_title],scope,my_reaction_emoji,confidential,weight,epic_id,health_status,iteration_id,iteration_title,non_archived",
        [nameof(JobArtifactDownloadOptionsQueryExtensions)] = "file_type,job_token",
        [nameof(JobArtifactRefDownloadOptionsQueryExtensions)] = "job_token,search_recent_successful_pipelines",
        [nameof(JobArtifactTreeListOptionsQueryExtensions)] = "path,recursive,job_token,per_page",
        [nameof(JobListOptionsQueryExtensions)] = "scope,ref,per_page",
        [nameof(JobTraceOptionsQueryExtensions)] = "byte_offset,byte_limit",
        [nameof(LabelGetOptionsQueryExtensions)] = "include_ancestor_groups",
        [nameof(LabelListOptionsQueryExtensions)] = "search,with_counts,include_ancestor_groups,archived,per_page",
        [nameof(MergeRequestListOptionsQueryExtensions)] =
            "state,labels,not[labels],source_branch,target_branch,source_project_id,author_id,author_username,not[author_id],assignee_id,assignee_username,reviewer_id,reviewer_username,milestone,not[milestone],my_reaction_emoji,scope,draft,with_labels_details,with_merge_status_recheck,search,in,created_after,created_before,updated_after,updated_before,merged_after,merged_before,deployed_after,deployed_before,environment,merge_user_id,merge_user_username,approved_by_ids,approved_by_usernames,approver_ids,iids,non_archived,order_by,sort,per_page",
        [nameof(MilestoneListOptionsQueryExtensions)] =
            "state,iids,title,search,include_parent_milestones,include_ancestors,updated_before,updated_after,per_page",
        [nameof(PipelineListOptionsQueryExtensions)] =
            "status,ref,sha,source,name,username,scope,updated_after,updated_before,created_after,created_before,yaml_errors,order_by,sort,per_page",
        [nameof(PipelineScheduleListOptionsQueryExtensions)] = "scope,per_page",
        [nameof(PipelineScheduleRunListOptionsQueryExtensions)] = "scope,status,sort,per_page",
        [nameof(ProjectListOptionsQueryExtensions)] =
            "search,visibility,topic,archived,last_activity_after,marked_for_deletion_on,per_page,order_by,sort,search_namespaces,owned,starred,imported,membership,with_issues_enabled,with_merge_requests_enabled,with_programming_language,min_access_level,id_after,id_before,last_activity_before,repository_storage,topic_id,updated_before,updated_after,include_pending_delete,active,wiki_checksum_failed,repository_checksum_failed,include_hidden,simple,statistics,with_custom_attributes",
        [nameof(ReleaseLinkListOptionsQueryExtensions)] = "per_page",
        [nameof(ReviewAppDeletionOptionsQueryExtensions)] = "before,limit,dry_run",
        [nameof(RunnerJobListOptionsQueryExtensions)] = "status,order_by,sort,system_id,per_page",
        [nameof(RunnerListOptionsQueryExtensions)] = "type,paused,status,tag_list,version_prefix,per_page",
        [nameof(ServiceAccountListOptionsQueryExtensions)] = "order_by,sort,per_page",
        [nameof(SharedGroupListOptionsQueryExtensions)] =
            "skip_groups,visibility,search,min_access_level,order_by,sort,with_custom_attributes,per_page",
        [nameof(SnippetListOptionsQueryExtensions)] = "created_after,created_before,per_page",
        [nameof(StorageMoveListOptionsQueryExtensions)] = "per_page",
        [nameof(TagListOptionsQueryExtensions)] = "order_by,sort,search,per_page",
        [nameof(UserActivityListOptionsQueryExtensions)] = "from,per_page",
        [nameof(UserMembershipListOptionsQueryExtensions)] = "type,per_page",
        [nameof(UserPipelineListOptionsQueryExtensions)] =
            "source,created_before,created_after,order_by,sort,cursor,per_page",
        [nameof(CommitDiffOptionsQueryExtensions)] = "unidiff,per_page",
        [nameof(CommitMergeRequestListOptionsQueryExtensions)] = "state,per_page",
        [nameof(CommitRefListOptionsQueryExtensions)] = "type,per_page",
        [nameof(RepositoryArchiveOptionsQueryExtensions)] =
            "sha,ref_type,format,path,include_lfs_blobs,exclude_paths",
        [nameof(UserListOptionsQueryExtensions)] =
            "username,extern_uid,public_email,provider,search,active,humans,external,blocked,admins,auditors,two_factor,created_after,created_before,without_projects,without_project_bots,exclude_active,exclude_external,exclude_humans,exclude_internal,skip_ldap,with_custom_attributes,order_by,sort,per_page",
        [nameof(ProjectAncestorGroupListOptionsQueryExtensions)] =
            "search,skip_groups,with_shared,shared_visible_only,shared_min_access_level,per_page",
        [nameof(ProjectAuditEventListOptionsQueryExtensions)] = "created_after,created_before,per_page",
        [nameof(ProjectInvitedGroupListOptionsQueryExtensions)] =
            "relation,search,min_access_level,with_custom_attributes,per_page",
        [nameof(ProjectShareLocationListOptionsQueryExtensions)] = "search",
        [nameof(ProjectStarrerListOptionsQueryExtensions)] = "search,per_page",
        [nameof(ProjectUserListOptionsQueryExtensions)] = "search,skip_users,per_page",
        [nameof(MergeRequestDiffListOptionsQueryExtensions)] = "unidiff,per_page",
        [nameof(DependencyListOptionsQueryExtensions)] = "package_manager,per_page",
        [nameof(ManagedLicenseListOptionsQueryExtensions)] = "per_page"
    };

    private static readonly Dictionary<string, string> GeneratedWireNames = new()
    {
        [nameof(AgentSessionListOptionsQueryExtensions)] =
            AgentSessionListOptionsQueryExtensions.QueryParameterNames,
        [nameof(AllSnippetListOptionsQueryExtensions)] = AllSnippetListOptionsQueryExtensions.QueryParameterNames,
        [nameof(BranchListOptionsQueryExtensions)] = BranchListOptionsQueryExtensions.QueryParameterNames,
        [nameof(CommitListOptionsQueryExtensions)] = CommitListOptionsQueryExtensions.QueryParameterNames,
        [nameof(GroupBillableMemberListOptionsQueryExtensions)] =
            GroupBillableMemberListOptionsQueryExtensions.QueryParameterNames,
        [nameof(GroupLabelGetOptionsQueryExtensions)] = GroupLabelGetOptionsQueryExtensions.QueryParameterNames,
        [nameof(GroupLabelListOptionsQueryExtensions)] = GroupLabelListOptionsQueryExtensions.QueryParameterNames,
        [nameof(GroupListOptionsQueryExtensions)] = GroupListOptionsQueryExtensions.QueryParameterNames,
        [nameof(GroupProtectedBranchListOptionsQueryExtensions)] =
            GroupProtectedBranchListOptionsQueryExtensions.QueryParameterNames,
        [nameof(GroupReleaseListOptionsQueryExtensions)] =
            GroupReleaseListOptionsQueryExtensions.QueryParameterNames,
        [nameof(GroupTransferLocationListOptionsQueryExtensions)] =
            GroupTransferLocationListOptionsQueryExtensions.QueryParameterNames,
        [nameof(GroupUserListOptionsQueryExtensions)] = GroupUserListOptionsQueryExtensions.QueryParameterNames,
        [nameof(InvitedGroupListOptionsQueryExtensions)] =
            InvitedGroupListOptionsQueryExtensions.QueryParameterNames,
        [nameof(IssueListOptionsQueryExtensions)] = IssueListOptionsQueryExtensions.QueryParameterNames,
        [nameof(JobArtifactDownloadOptionsQueryExtensions)] =
            JobArtifactDownloadOptionsQueryExtensions.QueryParameterNames,
        [nameof(JobArtifactRefDownloadOptionsQueryExtensions)] =
            JobArtifactRefDownloadOptionsQueryExtensions.QueryParameterNames,
        [nameof(JobArtifactTreeListOptionsQueryExtensions)] =
            JobArtifactTreeListOptionsQueryExtensions.QueryParameterNames,
        [nameof(JobListOptionsQueryExtensions)] = JobListOptionsQueryExtensions.QueryParameterNames,
        [nameof(JobTraceOptionsQueryExtensions)] = JobTraceOptionsQueryExtensions.QueryParameterNames,
        [nameof(LabelGetOptionsQueryExtensions)] = LabelGetOptionsQueryExtensions.QueryParameterNames,
        [nameof(LabelListOptionsQueryExtensions)] = LabelListOptionsQueryExtensions.QueryParameterNames,
        [nameof(MergeRequestListOptionsQueryExtensions)] =
            MergeRequestListOptionsQueryExtensions.QueryParameterNames,
        [nameof(MilestoneListOptionsQueryExtensions)] = MilestoneListOptionsQueryExtensions.QueryParameterNames,
        [nameof(PipelineListOptionsQueryExtensions)] = PipelineListOptionsQueryExtensions.QueryParameterNames,
        [nameof(PipelineScheduleListOptionsQueryExtensions)] =
            PipelineScheduleListOptionsQueryExtensions.QueryParameterNames,
        [nameof(PipelineScheduleRunListOptionsQueryExtensions)] =
            PipelineScheduleRunListOptionsQueryExtensions.QueryParameterNames,
        [nameof(ProjectListOptionsQueryExtensions)] = ProjectListOptionsQueryExtensions.QueryParameterNames,
        [nameof(ReleaseLinkListOptionsQueryExtensions)] = ReleaseLinkListOptionsQueryExtensions.QueryParameterNames,
        [nameof(ReviewAppDeletionOptionsQueryExtensions)] =
            ReviewAppDeletionOptionsQueryExtensions.QueryParameterNames,
        [nameof(RunnerJobListOptionsQueryExtensions)] = RunnerJobListOptionsQueryExtensions.QueryParameterNames,
        [nameof(RunnerListOptionsQueryExtensions)] = RunnerListOptionsQueryExtensions.QueryParameterNames,
        [nameof(ServiceAccountListOptionsQueryExtensions)] =
            ServiceAccountListOptionsQueryExtensions.QueryParameterNames,
        [nameof(SharedGroupListOptionsQueryExtensions)] = SharedGroupListOptionsQueryExtensions.QueryParameterNames,
        [nameof(SnippetListOptionsQueryExtensions)] = SnippetListOptionsQueryExtensions.QueryParameterNames,
        [nameof(StorageMoveListOptionsQueryExtensions)] = StorageMoveListOptionsQueryExtensions.QueryParameterNames,
        [nameof(TagListOptionsQueryExtensions)] = TagListOptionsQueryExtensions.QueryParameterNames,
        [nameof(UserActivityListOptionsQueryExtensions)] =
            UserActivityListOptionsQueryExtensions.QueryParameterNames,
        [nameof(UserMembershipListOptionsQueryExtensions)] =
            UserMembershipListOptionsQueryExtensions.QueryParameterNames,
        [nameof(UserPipelineListOptionsQueryExtensions)] =
            UserPipelineListOptionsQueryExtensions.QueryParameterNames,
        [nameof(CommitDiffOptionsQueryExtensions)] = CommitDiffOptionsQueryExtensions.QueryParameterNames,
        [nameof(CommitMergeRequestListOptionsQueryExtensions)] =
            CommitMergeRequestListOptionsQueryExtensions.QueryParameterNames,
        [nameof(CommitRefListOptionsQueryExtensions)] = CommitRefListOptionsQueryExtensions.QueryParameterNames,
        [nameof(RepositoryArchiveOptionsQueryExtensions)] =
            RepositoryArchiveOptionsQueryExtensions.QueryParameterNames,
        [nameof(UserListOptionsQueryExtensions)] = UserListOptionsQueryExtensions.QueryParameterNames,
        [nameof(ProjectAncestorGroupListOptionsQueryExtensions)] =
            ProjectAncestorGroupListOptionsQueryExtensions.QueryParameterNames,
        [nameof(ProjectAuditEventListOptionsQueryExtensions)] =
            ProjectAuditEventListOptionsQueryExtensions.QueryParameterNames,
        [nameof(ProjectInvitedGroupListOptionsQueryExtensions)] =
            ProjectInvitedGroupListOptionsQueryExtensions.QueryParameterNames,
        [nameof(ProjectShareLocationListOptionsQueryExtensions)] =
            ProjectShareLocationListOptionsQueryExtensions.QueryParameterNames,
        [nameof(ProjectStarrerListOptionsQueryExtensions)] =
            ProjectStarrerListOptionsQueryExtensions.QueryParameterNames,
        [nameof(ProjectUserListOptionsQueryExtensions)] = ProjectUserListOptionsQueryExtensions.QueryParameterNames,
        [nameof(MergeRequestDiffListOptionsQueryExtensions)] =
            MergeRequestDiffListOptionsQueryExtensions.QueryParameterNames,
        [nameof(DependencyListOptionsQueryExtensions)] = DependencyListOptionsQueryExtensions.QueryParameterNames,
        [nameof(ManagedLicenseListOptionsQueryExtensions)] =
            ManagedLicenseListOptionsQueryExtensions.QueryParameterNames
    };

    /// <summary>
    ///     Every name here was checked against the vendored spec's declaration of the corresponding
    ///     <c>GET</c> operation. GitLab answers 200 and ignores a parameter it does not recognise, so
    ///     nothing else in the suite would catch a misspelling.
    /// </summary>
    [Fact]
    public void GeneratedWireNames_MatchTheSpelling_TheGitLabSpecDeclares()
    {
        Dictionary<string, (string Generated, string Expected)> actual = new(ExpectedWireNames.Count);
        foreach ((string type, string generated) in GeneratedWireNames)
        {
            actual[type] = (generated, ExpectedWireNames[type]);
        }

        Assert.Equal(ExpectedWireNames.Count, actual.Count);
        foreach ((string type, (string generated, string expected)) in actual)
        {
            Assert.Equal(expected, generated);
            Assert.NotNull(type);
        }
    }


    /// <summary>
    ///     <c>TriggerJobListOptions</c> projects onto the same two wire names as
    ///     <c>PipelineScheduleListOptions</c>, so it cannot be another <c>[InlineData]</c> row on the theory
    ///     above - two identical rows are an xUnit1025 build warning. Pinned here instead.
    /// </summary>
    [Fact]
    public void TriggerJobListOptions_GeneratesTheWireNames_TheGitLabSpecDeclares()
    {
        Assert.Equal("scope,per_page", TriggerJobListOptionsQueryExtensions.QueryParameterNames);
    }

    [Fact]
    public void QueryFrom_WithNullOptions_AddsNothing()
    {
        Uri route = GitLabRouteBuilder.Create("projects").QueryFrom((ProjectListOptions?)null).Build();

        Assert.Equal("projects", route.OriginalString);
    }

    [Fact]
    public void QueryFrom_WithAnEmptyOptionsInstance_AddsNothing()
    {
        Uri route = GitLabRouteBuilder.Create("projects").QueryFrom(new ProjectListOptions()).Build();

        Assert.Equal("projects", route.OriginalString);
    }

    [Fact]
    public void QueryFrom_ProjectsEveryValueShape_InPropertyDeclarationOrder()
    {
        ProjectListOptions options = new()
        {
            Search = "gitlab",
            Visibility = GitLabVisibility.Internal,
            Topic = Topics,
            Archived = false,
            LastActivityAfter = new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero),
            MarkedForDeletionOn = new DateOnly(2024, 3, 4),
            PerPage = 20
        };

        Uri route = GitLabRouteBuilder.Create("projects").QueryFrom(options).Build();

        Assert.Equal(
            "projects?search=gitlab&visibility=internal&topic=devops,a%2Cb&archived=false" +
            "&last_activity_after=2024-01-02T03:04:05Z&marked_for_deletion_on=2024-03-04&per_page=20",
            route.OriginalString);
    }

    [Fact]
    public void QueryFrom_UsesTheExplicitWireName_WhereGitLabsSpellingIsNotDerivable()
    {
        IssueListOptions options = new()
        {
            State = GitLabIssueStateFilter.Opened,
            Labels = LabelsWithASpace,
            NotLabels = ExcludedLabels,
            Iids = Iids,
            AuthorId = 7,
            PerPage = 20
        };

        Uri route = GitLabRouteBuilder.Create("projects").Segment(ProjectId.FromId(1)).Literal("issues")
            .QueryFrom(options)
            .Build();

        Assert.Equal(
            "projects/1/issues?state=opened&labels=bug,needs%20review&not[labels]=wontfix&iids=41,42" +
            "&author_id=7&per_page=20",
            route.OriginalString);
    }

    [Fact]
    public void QueryFrom_SkipsUnsetProperties_SoOnlySuppliedFiltersReachTheServer()
    {
        Uri route = GitLabRouteBuilder.Create("projects").Segment(ProjectId.FromId(1)).Literal("pipelines")
            .QueryFrom(new PipelineListOptions { Ref = "main", PerPage = 20 })
            .Build();

        Assert.Equal("projects/1/pipelines?ref=main&per_page=20", route.OriginalString);
    }

    [Theory]
    [InlineData(GitLabVisibility.Private)]
    [InlineData(GitLabVisibility.Internal)]
    [InlineData(GitLabVisibility.Public)]
    public void EnumQueryValue_IsTheSameStringSystemTextJsonWrites(GitLabVisibility visibility)
    {
        GitLabProject project = new()
        {
            Id = 1,
            Name = "gitlab",
            PathWithNamespace = "gitlab-org/gitlab",
            Visibility = visibility,
            WebUrl = new Uri("https://gitlab.example/gitlab-org/gitlab")
        };

        string json = JsonSerializer.Serialize(project, GitLabJsonContext.Default.GitLabProject);

        // Both sides come from the same [JsonStringEnumMemberName], which is the point of generating
        // the query helper from that attribute rather than hand-writing a second mapping.
        Assert.Contains($"\"visibility\":\"{visibility.ToApiValue()}\"", json, StringComparison.Ordinal);
    }
}