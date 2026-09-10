using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.GraphQL;

using Microsoft.Extensions.DependencyInjection;

namespace GitLab.Client.Tests.DependencyInjection.GraphQL;

/// <summary>
///     Verifies that GraphQL joins the existing client composition root without creating a second HTTP
///     pipeline or a separately scoped endpoint client.
/// </summary>
public sealed class GraphQLServiceCollectionRegistrationTests
{
    [Fact]
    public void AddGitLabClient_RegistersTheSharedGraphQLTransportAsASingleton()
    {
        using ServiceProvider provider = BuildProvider(out ServiceCollection services);

        ServiceDescriptor descriptor = Assert.Single(services,
            candidate => candidate.ServiceType == typeof(IGitLabGraphQLConnection));

        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        Assert.IsType<GitLabGraphQLConnection>(provider.GetRequiredService<IGitLabGraphQLConnection>());
        Assert.Same(
            provider.GetRequiredService<IGitLabGraphQLConnection>(),
            provider.GetRequiredService<IGitLabGraphQLConnection>());
    }

    [Fact]
    public void AddGitLabClient_ResolvesOneGraphQLClientThroughTheRootAggregate()
    {
        using ServiceProvider provider = BuildProvider(out ServiceCollection services);

        IGraphQLClient directlyResolved = provider.GetRequiredService<IGraphQLClient>();
        IGitLabClient root = provider.GetRequiredService<IGitLabClient>();

        ServiceDescriptor descriptor = Assert.Single(services,
            candidate => candidate.ServiceType == typeof(IGraphQLClient));

        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        Assert.Same(directlyResolved, root.GraphQL);
        Assert.Same(directlyResolved, provider.GetRequiredService<IGraphQLClient>());
    }

    [Fact]
    public void AddGitLabClient_RegistersWorkItemsAsTheGraphQLClientView()
    {
        using ServiceProvider provider = BuildProvider(out ServiceCollection services);

        IGraphQLClient graphQL = provider.GetRequiredService<IGraphQLClient>();
        IGraphQLWorkItemsClient directlyResolved = provider.GetRequiredService<IGraphQLWorkItemsClient>();

        ServiceDescriptor descriptor = Assert.Single(services,
            candidate => candidate.ServiceType == typeof(IGraphQLWorkItemsClient));

        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        Assert.Same(graphQL.WorkItems, directlyResolved);
        Assert.Same(directlyResolved, provider.GetRequiredService<IGraphQLWorkItemsClient>());
    }

    private static ServiceProvider BuildProvider(out ServiceCollection services)
    {
        services = new ServiceCollection();
        services.AddGitLabClient(options => options.AccessToken = "glpat-test-token");
        return services.BuildServiceProvider();
    }
}