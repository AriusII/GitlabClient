using GitLab.Client.Abstractions;
using GitLab.Client.Batching;
using GitLab.Client.Composition;
using GitLab.Client.Models;

using Microsoft.Extensions.DependencyInjection;

namespace GitLab.Client.Tests.Batching;

public sealed class GitLabBatchClientTests
{
    [Fact]
    public async Task AddGitLabClient_ExposesOneStatelessBatchFacadeAndComposesResultsWithMappers()
    {
        ServiceCollection services = new();
        services.AddGitLabClient(options => options.AccessToken = "glpat-test-token");
        using ServiceProvider provider = services.BuildServiceProvider(
            new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });

        IBatchesClient directlyResolved = provider.GetRequiredService<IBatchesClient>();
        IGitLabClient root = provider.GetRequiredService<IGitLabClient>();
        ServiceDescriptor descriptor = Assert.Single(services,
            candidate => candidate.ServiceType == typeof(IBatchesClient));

        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        Assert.Same(directlyResolved, root.Batches);

        GitLabProject project = new() { Id = 42, Name = "sdk" };
        GitLabBatchPlan plan = root.Batches.Create(new GitLabBatchOptions { MaxConcurrency = 2 });
        GitLabBatchOperation<GitLabProject> projectOperation = plan.Add<GitLabProject>(_ => Task.FromResult(project));
        GitLabBatchExecution execution = await plan.ExecuteAsync(TestContext.Current.CancellationToken);

        GitLabProjectComposition composition = root.Mappers.Projects.Map(
            new GitLabProjectCompositionInput { Project = projectOperation.GetResult(execution) },
            GitLabProjectCompositionLoadPlan.ProjectOnly);

        Assert.Same(project, composition.Project);
        Assert.True(execution.IsSuccess);
    }
}