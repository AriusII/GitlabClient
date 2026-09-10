using System.Text.Json.Serialization;

namespace GitLab.Client.GraphQL.WorkItems.Identifiers;

/// <summary>
///     An opaque GitLab GraphQL global ID, for example <c>gid://gitlab/WorkItem/123</c>.
/// </summary>
/// <remarks>
///     A global ID is deliberately not parsed into a database ID or a resource kind. GitLab uses it as an opaque
///     identifier, and both its prefix and representation can vary by resource, namespace, and GitLab version.
/// </remarks>
[JsonConverter(typeof(GitLabGraphQLGlobalIdJsonConverter))]
public sealed record GitLabGraphQLGlobalId
{
    /// <summary>Initializes an opaque non-empty GraphQL global ID.</summary>
    /// <param name="value">The exact string supplied by GitLab or accepted by a GraphQL input field.</param>
    /// <exception cref="ArgumentException"><paramref name="value" /> is null, empty, or whitespace.</exception>
    public GitLabGraphQLGlobalId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    /// <summary>The unmodified GraphQL global-ID string.</summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value;
    }
}