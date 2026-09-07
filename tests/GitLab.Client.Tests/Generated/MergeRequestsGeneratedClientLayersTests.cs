using GitLab.Client.Controllers;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Generated;

/// <summary>
///     Verifies GenerateClientLayersGenerator's output for the Merge Requests resource directly:
///     MergeRequestsService and MergeRequestsController are generated, not hand-written, so this is the
///     test that actually exercises the generator rather than just the hand-written Repository underneath it.
/// </summary>
public sealed class MergeRequestsGeneratedClientLayersTests
{
    [Fact]
    public async Task MergeRequestsService_GetAsync_ForwardsToRepository_Unchanged()
    {
        GitLabMergeRequest expected = new()
        {
            Id = 1,
            Iid = 7,
            Title = "Add feature",
            State = "opened",
            SourceBranch = "feature",
            TargetBranch = "main",
            WebUrl = new Uri("https://gitlab.example/gitlab-org/gitlab/-/merge_requests/7")
        };

        FakeMergeRequestsRepository repository = new() { OnGetAsync = (_, _, _) => Task.FromResult(expected) };

        MergeRequestsService service = new(repository);

        GitLabMergeRequest actual = await service.GetAsync(1, 7, TestContext.Current.CancellationToken);

        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task MergeRequestsController_CreateAsync_ForwardsToService_Unchanged()
    {
        GitLabMergeRequest expected = new()
        {
            Id = 2,
            Iid = 9,
            Title = "New MR",
            State = "opened",
            SourceBranch = "feature/x",
            TargetBranch = "main",
            WebUrl = new Uri("https://gitlab.example/gitlab-org/gitlab/-/merge_requests/9")
        };

        FakeMergeRequestsService service = new() { OnCreateAsync = (_, _, _) => Task.FromResult(expected) };

        MergeRequestsController controller = new(service);

        CreateMergeRequestRequest request = new()
        {
            Title = "New MR", SourceBranch = "feature/x", TargetBranch = "main"
        };

        GitLabMergeRequest actual = await controller.CreateAsync(1, request, TestContext.Current.CancellationToken);

        Assert.Same(expected, actual);
    }
}