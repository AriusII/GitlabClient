using System.Reflection;

using GitLab.Client.Abstractions;
using GitLab.Client.Composition;
using GitLab.Client.Composition.WorkItems;
using GitLab.Client.Models;

using Microsoft.Extensions.DependencyInjection;

namespace GitLab.Client.Tests.Composition;

/// <summary>
///     Verifies the ergonomic root-client mapper view stays a singleton façade over the existing pure contracts.
/// </summary>
public sealed class GitLabMapperFacadeTests
{
    [Fact]
    public void AddGitLabClient_ExposesOnePureMapperFacadeThroughTheRootClient()
    {
        using ServiceProvider provider = BuildProvider(out ServiceCollection services);

        IMappersClient directlyResolved = provider.GetRequiredService<IMappersClient>();
        IGitLabClient root = provider.GetRequiredService<IGitLabClient>();
        ServiceDescriptor descriptor = Assert.Single(services,
            candidate => candidate.ServiceType == typeof(IMappersClient));

        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        Assert.Same(directlyResolved, root.Mappers);
        Assert.Same(directlyResolved.Projects, root.Mappers.Projects);
        Assert.Same(directlyResolved.WorkItems, root.Mappers.WorkItems);
    }

    [Fact]
    public void WorkItems_Map_DelegatesToThePureContractWithoutAClientOperation()
    {
        using ServiceProvider provider = BuildProvider(out _);
        GitLabWorkItemCompositionInput input = new();
        GitLabWorkItemCompositionLoadPlan loadPlan = GitLabWorkItemCompositionLoadPlan.Empty;

        GitLabWorkItemComposition expected = GitLabWorkItemCompositionMapper.Map(input, loadPlan);
        GitLabWorkItemComposition actual = provider.GetRequiredService<IGitLabClient>()
            .Mappers
            .WorkItems
            .Map(input, loadPlan);

        Assert.Equal(expected, actual);
        Assert.Same(loadPlan, actual.LoadPlan);
        Assert.False(actual.Sources.GraphQLCore.WasRequested);
        Assert.False(actual.Sources.GraphQLCore.IsLoaded);
    }

    [Fact]
    public void MappersClient_HasNoTransportConstructorOrInstanceState()
    {
        Type endpointType = typeof(IMappersClient).Assembly.GetType("GitLab.Client.Endpoints.MappersClient", true)!;
        ConstructorInfo constructor = Assert.Single(endpointType.GetConstructors(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
        FieldInfo[] fields = endpointType.GetFields(BindingFlags.Instance | BindingFlags.Static |
                                                    BindingFlags.Public | BindingFlags.NonPublic);

        Assert.Empty(constructor.GetParameters());
        Assert.All(fields, field => Assert.True(field.IsStatic));
        Assert.Equal(
            [typeof(GitLabProjectMappers), typeof(GitLabWorkItemMappers)],
            fields.Select(field => field.FieldType).OrderBy(type => type.FullName, StringComparer.Ordinal));
    }

    [Fact]
    public void Projects_Map_DelegatesToThePureContractAndPreservesTheRouteDtoInstance()
    {
        using ServiceProvider provider = BuildProvider(out _);
        GitLabProject project = new() { Id = 42, Name = "sdk" };
        GitLabProjectCompositionInput input = new() { Project = project };

        GitLabProjectComposition composition = provider.GetRequiredService<IGitLabClient>()
            .Mappers
            .Projects
            .Map(input, GitLabProjectCompositionLoadPlan.ProjectOnly);

        Assert.Same(project, composition.Project);
        Assert.True(composition.IsLoaded(GitLabProjectCompositionSection.Project));
        Assert.Empty(composition.Issues);
    }

    private static ServiceProvider BuildProvider(out ServiceCollection services)
    {
        services = new ServiceCollection();
        services.AddGitLabClient(options => options.AccessToken = "glpat-test-token");
        return services.BuildServiceProvider();
    }
}