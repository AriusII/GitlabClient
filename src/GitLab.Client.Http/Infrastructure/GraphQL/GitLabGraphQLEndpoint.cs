using GitLab.Client.Configuration;

namespace GitLab.Client.Infrastructure.GraphQL;

/// <summary>
///     Resolves GitLab's versionless GraphQL endpoint without treating it as a REST v4 route.
/// </summary>
internal static class GitLabGraphQLEndpoint
{
    private const string RestApiPathSuffix = "/api/v4/";

    /// <summary>
    ///     Resolves the explicit safe override, if supplied, otherwise REST's sibling
    ///     <c>../graphql</c>. For example, <c>/gitlab/api/v4/</c> becomes
    ///     <c>/gitlab/api/graphql</c> while retaining the configured origin and path prefix.
    /// </summary>
    public static Uri Resolve(GitLabClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return options.GraphQLEndpoint ?? Derive(options.BaseAddress);
    }

    private static Uri Derive(Uri? restBaseAddress)
    {
        if (restBaseAddress is null)
        {
            throw new InvalidOperationException(
                $"{nameof(GitLabClientOptions.BaseAddress)} must be configured before resolving the GraphQL endpoint.");
        }

        if (!restBaseAddress.IsAbsoluteUri
            || !restBaseAddress.AbsolutePath.EndsWith(RestApiPathSuffix, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"{nameof(GitLabClientOptions.BaseAddress)} must end with '{RestApiPathSuffix}' to derive the GraphQL endpoint. Configure {nameof(GitLabClientOptions.GraphQLEndpoint)} explicitly for a non-standard GitLab route.");
        }

        // Resolve one directory above the REST v4 root. This preserves a self-managed instance's path
        // prefix (for example /gitlab/) and cannot cross the configured origin.
        return new Uri(restBaseAddress, "../graphql");
    }
}