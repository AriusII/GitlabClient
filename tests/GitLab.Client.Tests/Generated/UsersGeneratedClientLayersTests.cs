using GitLab.Client.Controllers;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Generated;

/// <summary>
///     Verifies GenerateClientLayersGenerator's output for the Users resource directly: UsersService
///     and UsersController are generated, not hand-written, so this exercises the generator rather
///     than just the hand-written Repository underneath it.
/// </summary>
public sealed class UsersGeneratedClientLayersTests
{
    [Fact]
    public async Task UsersService_GetAsync_ForwardsToRepository_Unchanged()
    {
        GitLabUser expected = new()
        {
            Id = 1, Username = "octocat", Name = "Octo Cat", WebUrl = new Uri("https://gitlab.example/octocat")
        };

        FakeUsersRepository repository = new() { OnGetAsync = (_, _) => Task.FromResult(expected) };

        UsersService service = new(repository);

        GitLabUser actual = await service.GetAsync(1, TestContext.Current.CancellationToken);

        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task UsersController_GetCurrentAsync_ForwardsToService_Unchanged()
    {
        GitLabUser expected = new()
        {
            Id = 2, Username = "self", Name = "Current User", WebUrl = new Uri("https://gitlab.example/self")
        };

        FakeUsersService service = new() { OnGetCurrentAsync = _ => Task.FromResult(expected) };

        UsersController controller = new(service);

        GitLabUser actual = await controller.GetCurrentAsync(TestContext.Current.CancellationToken);

        Assert.Same(expected, actual);
    }
}