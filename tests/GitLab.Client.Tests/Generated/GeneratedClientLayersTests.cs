using GitLab.Client.Controllers;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Generated;

/// <summary>
///     Verifies GenerateClientLayersGenerator's output directly: ProjectsService and ProjectsController
///     are generated, not hand-written, so this is the test that actually exercises the generator
///     rather than just the hand-written Repository underneath it.
/// </summary>
public sealed class GeneratedClientLayersTests
{
    [Fact]
    public async Task ProjectsService_GetAsync_ForwardsToRepository_Unchanged()
    {
        GitLabProject expected = new()
        {
            Id = 1,
            Name = "GitLab",
            PathWithNamespace = "gitlab-org/gitlab",
            Visibility = GitLabVisibility.Public,
            WebUrl = new Uri("https://gitlab.com/gitlab-org/gitlab")
        };

        FakeProjectsRepository repository = new() { OnGetAsync = (_, _) => Task.FromResult(expected) };

        ProjectsService service = new(repository);

        GitLabProject actual = await service.GetAsync(1, TestContext.Current.CancellationToken);

        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task ProjectsController_GetAsync_ForwardsToService_Unchanged()
    {
        GitLabProject expected = new()
        {
            Id = 2,
            Name = "GitLab Runner",
            PathWithNamespace = "gitlab-org/gitlab-runner",
            Visibility = GitLabVisibility.Public,
            WebUrl = new Uri("https://gitlab.com/gitlab-org/gitlab-runner")
        };

        FakeProjectsService service = new() { OnGetAsync = (_, _) => Task.FromResult(expected) };

        ProjectsController controller = new(service);

        GitLabProject actual = await controller.GetAsync(2, TestContext.Current.CancellationToken);

        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task ProjectsService_ListAsync_ForwardsToRepository_Unchanged()
    {
        GitLabProject expected = new()
        {
            Id = 3,
            Name = "GitLab Shell",
            PathWithNamespace = "gitlab-org/gitlab-shell",
            Visibility = GitLabVisibility.Public,
            WebUrl = new Uri("https://gitlab.com/gitlab-org/gitlab-shell")
        };

        FakeProjectsRepository repository = new() { OnListAsync = (_, _) => AsAsyncEnumerable(expected) };

        ProjectsService service = new(repository);

        List<GitLabProject> actual = new();
        await foreach (GitLabProject project in service.ListAsync(
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            actual.Add(project);
        }

        Assert.Same(expected, Assert.Single(actual));
    }

    private static async IAsyncEnumerable<GitLabProject> AsAsyncEnumerable(params GitLabProject[] items)
    {
        foreach (GitLabProject item in items)
        {
            yield return item;
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }
}