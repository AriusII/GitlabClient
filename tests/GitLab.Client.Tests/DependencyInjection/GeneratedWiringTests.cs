using System.Reflection;

using GitLab.Client.Abstractions;

using Microsoft.Extensions.DependencyInjection;

namespace GitLab.Client.Tests.DependencyInjection;

/// <summary>
///     Covers everything <c>GitLabClientWiringGenerator</c> emits, through the real compiled assembly.
///     These tests enumerate public resource-client interfaces rather than listing them. They derive
///     the direct composition-root clients by excluding interfaces owned by another client facade, so
///     every new root resource is covered without flattening intentional nested APIs.
///     <para>Reflection is fine here: the test assembly is neither trimmed nor AOT-published.</para>
/// </summary>
public sealed class GeneratedWiringTests
{
    private static readonly Type[] PublicClientInterfaces = typeof(IGitLabClient).Assembly
        .GetExportedTypes()
        .Where(type => type.IsInterface
                       && string.Equals(type.Namespace, "GitLab.Client.Abstractions", StringComparison.Ordinal)
                       && type.Name.EndsWith("Client", StringComparison.Ordinal)
                       && type != typeof(IGitLabClient))
        .OrderBy(type => type.Name, StringComparer.Ordinal)
        .ToArray();

    // A client exposed from another client is intentionally a nested facade, not an additional
    // top-level IGitLabClient property. For example, GraphQL owns WorkItems so callers retain a
    // clear API boundary: gitLab.GraphQL.WorkItems rather than gitLab.WorkItems.
    private static readonly Type[] NestedClientInterfaces = PublicClientInterfaces
        .Where(candidate => PublicClientInterfaces
            .Where(parent => parent != candidate)
            .SelectMany(parent => parent.GetProperties())
            .Any(property => property.PropertyType == candidate))
        .OrderBy(type => type.Name, StringComparer.Ordinal)
        .ToArray();

    private static readonly Type[] RootClientInterfaces = PublicClientInterfaces
        .Except(NestedClientInterfaces)
        .OrderBy(type => type.Name, StringComparer.Ordinal)
        .ToArray();

    [Fact]
    public void AddGitLabClient_RegistersEveryRootResourceClientInterface()
    {
        using ServiceProvider provider = BuildProvider(out _);

        Assert.NotEmpty(RootClientInterfaces);

        foreach (Type clientInterface in RootClientInterfaces)
        {
            Assert.NotNull(provider.GetService(clientInterface));
        }
    }

    [Fact]
    public void IGitLabClient_ExposesExactlyEveryRootResourceClientInterface()
    {
        HashSet<Type> exposed = typeof(IGitLabClient).GetProperties()
            .Select(property => property.PropertyType)
            .ToHashSet();

        Assert.Equal(RootClientInterfaces.ToHashSet(), exposed);
    }

    [Fact]
    public void IGraphQLClient_ExposesWorkItemsAsItsNestedResourceClient()
    {
        using ServiceProvider provider = BuildProvider(out _);

        IGraphQLClient graphQL = provider.GetRequiredService<IGraphQLClient>();
        IGraphQLWorkItemsClient workItems = provider.GetRequiredService<IGraphQLWorkItemsClient>();

        Assert.Contains(typeof(IGraphQLWorkItemsClient), NestedClientInterfaces);
        Assert.DoesNotContain(typeof(IGraphQLWorkItemsClient), RootClientInterfaces);
        Assert.Same(workItems, graphQL.WorkItems);
    }

    [Fact]
    public void GitLabClient_PropertiesReturnTheContainerRegisteredInstances()
    {
        using ServiceProvider provider = BuildProvider(out _);
        IGitLabClient client = provider.GetRequiredService<IGitLabClient>();

        foreach (PropertyInfo property in typeof(IGitLabClient).GetProperties())
        {
            Assert.Same(provider.GetRequiredService(property.PropertyType), property.GetValue(client));
        }
    }

    [Fact]
    public void RootResourceClients_AreRegisteredAsSingletons()
    {
        using ServiceProvider provider = BuildProvider(out ServiceCollection services);

        foreach (Type clientInterface in RootClientInterfaces)
        {
            ServiceDescriptor descriptor =
                Assert.Single(services, candidate => candidate.ServiceType == clientInterface);
            Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        }
    }

    [Fact]
    public void AddGitLabClient_CalledTwice_ThrowsAndLeavesTheFirstRegistrationIntact()
    {
        ServiceCollection services = new();

        services.AddGitLabClient(options => options.AccessToken = "glpat-test-token");

        // The resource registrations themselves are TryAdd and would happily no-op, but the options and
        // HTTP-client configuration delegates accumulate, so a second call would silently become
        // last-writer-wins across two different configurations. Failing loudly is the contract.
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddGitLabClient(options => options.AccessToken = "glpat-other-token"));

        Assert.Contains("already been called", exception.Message, StringComparison.Ordinal);

        foreach (Type clientInterface in RootClientInterfaces)
        {
            Assert.Single(services, candidate => candidate.ServiceType == clientInterface);
        }
    }

    private static ServiceProvider BuildProvider(out ServiceCollection services)
    {
        services = new ServiceCollection();
        services.AddGitLabClient(options => options.AccessToken = "glpat-test-token");
        return services.BuildServiceProvider();
    }
}