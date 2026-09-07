using System.Reflection;

using GitLab.Client.Abstractions;

using Microsoft.Extensions.DependencyInjection;

namespace GitLab.Client.Tests.DependencyInjection;

/// <summary>
///     Covers everything <c>GitLabClientWiringGenerator</c> emits, through the real compiled assembly.
///     These tests enumerate the resource client interfaces rather than listing them, so every resource
///     added from here on is covered the moment its <c>I&lt;Resource&gt;Client</c> lands - which is
///     exactly the "you forgot to wire something" regression the generator exists to prevent.
///     <para>Reflection is fine here: the test assembly is neither trimmed nor AOT-published.</para>
/// </summary>
public sealed class GeneratedWiringTests
{
    private static readonly Type[] ResourceClientInterfaces = typeof(IGitLabClient).Assembly
        .GetExportedTypes()
        .Where(type => type.IsInterface
                       && string.Equals(type.Namespace, "GitLab.Client.Abstractions", StringComparison.Ordinal)
                       && type.Name.EndsWith("Client", StringComparison.Ordinal)
                       && type != typeof(IGitLabClient))
        .OrderBy(type => type.Name, StringComparer.Ordinal)
        .ToArray();

    [Fact]
    public void AddGitLabClient_RegistersEveryResourceClientInterface()
    {
        using ServiceProvider provider = BuildProvider(out _);

        Assert.NotEmpty(ResourceClientInterfaces);

        foreach (Type clientInterface in ResourceClientInterfaces)
        {
            Assert.NotNull(provider.GetService(clientInterface));
        }
    }

    [Fact]
    public void IGitLabClient_ExposesEveryResourceClientInterface()
    {
        HashSet<Type> exposed = typeof(IGitLabClient).GetProperties()
            .Select(property => property.PropertyType)
            .ToHashSet();

        Assert.Equal(ResourceClientInterfaces.ToHashSet(), exposed);
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
    public void ResourceClients_AreRegisteredAsSingletons()
    {
        using ServiceProvider provider = BuildProvider(out ServiceCollection services);

        foreach (Type clientInterface in ResourceClientInterfaces)
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

        foreach (Type clientInterface in ResourceClientInterfaces)
        {
            Assert.Single(services, candidate => candidate.ServiceType == clientInterface);
        }
    }

    /// <summary>
    ///     The generated registrations wire the whole Repository -&gt; Service -&gt; Controller chain, so
    ///     the internal layers must resolve too, not just the public client interface.
    /// </summary>
    [Fact]
    public void AddGitLabClient_RegistersTheWholeLayerChainForEveryResource()
    {
        using ServiceProvider provider = BuildProvider(out _);
        Assembly assembly = typeof(IGitLabClient).Assembly;

        foreach (Type clientInterface in ResourceClientInterfaces)
        {
            string resource = clientInterface.Name[1..^"Client".Length];

            Type? repositoryInterface = assembly.GetType($"GitLab.Client.Repositories.I{resource}Repository", false);
            Type? serviceInterface = assembly.GetType($"GitLab.Client.Services.I{resource}Service", false);

            Assert.NotNull(repositoryInterface);
            Assert.NotNull(serviceInterface);
            Assert.NotNull(provider.GetService(repositoryInterface));
            Assert.NotNull(provider.GetService(serviceInterface));
        }
    }

    private static ServiceProvider BuildProvider(out ServiceCollection services)
    {
        services = new ServiceCollection();
        services.AddGitLabClient(options => options.AccessToken = "glpat-test-token");
        return services.BuildServiceProvider();
    }
}