using GitLab.Client.Abstractions;
using GitLab.Client.Composition;
using GitLab.Client.Composition.WorkItems;

namespace GitLab.Client.Endpoints;

/// <summary>
///     Singleton root-client view over stateless DTO composition operations.
/// </summary>
/// <remarks>
///     The source generator registers this type because <see cref="IGitLabClient.Mappers" /> follows the standard
///     <c>I&lt;Resource&gt;Client</c> root convention. Unlike REST or GraphQL endpoint clients, it deliberately has no
///     transport dependency and allocates neither a request nor a response wrapper.
/// </remarks>
internal sealed class MappersClient : IMappersClient
{
    private static readonly GitLabProjectMappers ProjectMappers = new();
    private static readonly GitLabWorkItemMappers WorkItemMappers = new();

    public GitLabProjectMappers Projects => ProjectMappers;

    public GitLabWorkItemMappers WorkItems => WorkItemMappers;
}