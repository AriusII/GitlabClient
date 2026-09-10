using GitLab.Client.Composition;
using GitLab.Client.Composition.WorkItems;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Pure, local composition operations for DTOs already returned by GitLab REST and GraphQL calls.
/// </summary>
/// <remarks>
///     This is an ergonomic root-client view, not an API endpoint. Its members do not perform I/O, resolve an
///     <see cref="HttpClient" />, cache data, or initiate a request; callers retain control of fetching, pagination,
///     retries, and scheduling before passing their DTOs to a mapper.
/// </remarks>
public interface IMappersClient
{
    /// <summary>Pure project composition operations.</summary>
    GitLabProjectMappers Projects { get; }

    /// <summary>Pure REST and GraphQL Work Item composition operations.</summary>
    GitLabWorkItemMappers WorkItems { get; }
}