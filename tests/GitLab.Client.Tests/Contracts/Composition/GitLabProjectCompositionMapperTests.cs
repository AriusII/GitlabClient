using GitLab.Client.Composition;
using GitLab.Client.Models;
using GitLab.Client.Models.Responses;

namespace GitLab.Client.Tests.Contracts.Composition;

public sealed class GitLabProjectCompositionMapperTests
{
    [Fact]
    public void Map_CompletePlan_AssemblesExistingRouteDtosWithoutProjection()
    {
        GitLabProject project = new() { Id = 42, Name = "sdk" };
        GitLabPipeline pipeline = CreatePipeline();
        IReadOnlyList<GitLabJob> jobs = [CreateJob()];
        IReadOnlyList<GitLabMergeRequest> mergeRequests = [CreateMergeRequest()];
        IReadOnlyList<GitLabIssue> issues = [CreateIssue()];
        IReadOnlyList<GitLabEnvironment> environments = [new() { Id = 7, Name = "production" }];

        GitLabProjectComposition composition = GitLabProjectCompositionMapper.Map(
            new GitLabProjectCompositionInput
            {
                Project = project,
                LatestPipeline = pipeline,
                LatestPipelineJobs = jobs,
                MergeRequests = mergeRequests,
                Issues = issues,
                Environments = environments
            },
            GitLabProjectCompositionLoadPlan.Complete);

        Assert.Same(project, composition.Project);
        Assert.Same(pipeline, composition.LatestPipeline);
        Assert.Same(jobs, composition.LatestPipelineJobs);
        Assert.Same(mergeRequests, composition.MergeRequests);
        Assert.Same(issues, composition.Issues);
        Assert.Same(environments, composition.Environments);
        Assert.True(composition.IsLoaded(GitLabProjectCompositionSection.Project));
        Assert.True(composition.IsLoaded(GitLabProjectCompositionSection.LatestPipelineJobs));
        Assert.True(composition.IsLoaded(GitLabProjectCompositionSection.MergeRequests));
        Assert.True(composition.IsLoaded(GitLabProjectCompositionSection.Issues));
        Assert.True(composition.IsLoaded(GitLabProjectCompositionSection.Environments));
    }

    [Fact]
    public void Map_ProjectOnly_IgnoresUnselectedRouteResultsAndMarksThemNotLoaded()
    {
        GitLabProject project = new() { Id = 42, Name = "sdk" };

        GitLabProjectComposition composition = GitLabProjectCompositionMapper.Map(
            new GitLabProjectCompositionInput
            {
                Project = project,
                LatestPipeline = CreatePipeline(),
                LatestPipelineJobs = [CreateJob()],
                MergeRequests = [CreateMergeRequest()],
                Issues = [CreateIssue()],
                Environments = [new GitLabEnvironment { Id = 7, Name = "production" }]
            },
            GitLabProjectCompositionLoadPlan.ProjectOnly);

        Assert.Same(project, composition.Project);
        Assert.Null(composition.LatestPipeline);
        Assert.Empty(composition.LatestPipelineJobs);
        Assert.Empty(composition.MergeRequests);
        Assert.Empty(composition.Issues);
        Assert.Empty(composition.Environments);
        Assert.False(composition.IsLoaded(GitLabProjectCompositionSection.LatestPipeline));
        Assert.False(composition.IsLoaded(GitLabProjectCompositionSection.LatestPipelineJobs));
        Assert.False(composition.IsLoaded(GitLabProjectCompositionSection.MergeRequests));
        Assert.False(composition.IsLoaded(GitLabProjectCompositionSection.Issues));
        Assert.False(composition.IsLoaded(GitLabProjectCompositionSection.Environments));
    }

    [Fact]
    public void Constructor_LatestPipelineJobs_NormalizesItsRouteDependencies()
    {
        GitLabProjectCompositionLoadPlan plan = new(GitLabProjectCompositionSection.LatestPipelineJobs);

        Assert.Equal(
            GitLabProjectCompositionSection.Project |
            GitLabProjectCompositionSection.LatestPipeline |
            GitLabProjectCompositionSection.LatestPipelineJobs,
            plan.Sections);
        Assert.True(plan.Includes(GitLabProjectCompositionSection.Project));
        Assert.True(plan.Includes(GitLabProjectCompositionSection.LatestPipeline));
        Assert.True(plan.Includes(GitLabProjectCompositionSection.LatestPipelineJobs));
    }

    [Fact]
    public void Map_LoadedEmptyCollection_RemainsDistinctFromAnUnselectedRoute()
    {
        GitLabProjectCompositionLoadPlan plan = new(GitLabProjectCompositionSection.MergeRequests);

        GitLabProjectComposition composition = GitLabProjectCompositionMapper.Map(
            new GitLabProjectCompositionInput { Project = new GitLabProject { Id = 42 }, MergeRequests = [] },
            plan);

        Assert.Empty(composition.MergeRequests);
        Assert.True(composition.IsLoaded(GitLabProjectCompositionSection.MergeRequests));
        Assert.False(composition.IsLoaded(GitLabProjectCompositionSection.Issues));
    }

    [Fact]
    public void Map_SelectedCollectionWithoutRouteResult_RejectsAccidentalPartialComposition()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() => GitLabProjectCompositionMapper.Map(
            new GitLabProjectCompositionInput { Project = new GitLabProject { Id = 42 } },
            GitLabProjectCompositionLoadPlan.Collaboration));

        Assert.Equal("routeResult", exception.ParamName);
        Assert.Contains(nameof(GitLabProjectCompositionInput.MergeRequests), exception.Message,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Map_LoadedPipelineWithoutAnyPipeline_RetainsTheLoadedNullState()
    {
        GitLabProjectCompositionLoadPlan plan = new(GitLabProjectCompositionSection.LatestPipeline);

        GitLabProjectComposition composition = GitLabProjectCompositionMapper.Map(
            new GitLabProjectCompositionInput { Project = new GitLabProject { Id = 42 } },
            plan);

        Assert.Null(composition.LatestPipeline);
        Assert.True(composition.IsLoaded(GitLabProjectCompositionSection.LatestPipeline));
    }

    [Fact]
    public void Constructor_UnsupportedSection_Throws()
    {
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GitLabProjectCompositionLoadPlan((GitLabProjectCompositionSection)64));

        Assert.Equal("sections", exception.ParamName);
    }

    private static GitLabPipeline CreatePipeline()
    {
        return new GitLabPipeline
        {
            Id = 100,
            Sha = "0123456789abcdef",
            Ref = "main",
            Status = "success",
            WebUrl = new Uri("https://gitlab.example/group/sdk/-/pipelines/100")
        };
    }

    private static GitLabJob CreateJob()
    {
        return new GitLabJob
        {
            Id = 101,
            Status = "success",
            Name = "test",
            WebUrl = new Uri("https://gitlab.example/group/sdk/-/jobs/101")
        };
    }

    private static GitLabMergeRequest CreateMergeRequest()
    {
        return new GitLabMergeRequest
        {
            Id = 102,
            Iid = 4,
            Title = "Add composition contracts",
            State = "opened",
            SourceBranch = "feature/composition",
            TargetBranch = "main",
            WebUrl = new Uri("https://gitlab.example/group/sdk/-/merge_requests/4")
        };
    }

    private static GitLabIssue CreateIssue()
    {
        return new GitLabIssue
        {
            Id = 103,
            Iid = 5,
            Title = "Add a mapper",
            State = "opened",
            WebUrl = new Uri("https://gitlab.example/group/sdk/-/issues/5")
        };
    }
}