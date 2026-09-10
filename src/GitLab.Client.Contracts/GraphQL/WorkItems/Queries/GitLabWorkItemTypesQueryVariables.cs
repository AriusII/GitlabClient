using System.Text.Json.Serialization;

namespace GitLab.Client.GraphQL.WorkItems.Queries;

/// <summary>Variables for a curated cursor-paginated namespace work-item-types query.</summary>
public sealed record GitLabWorkItemTypesQueryVariables
{
    /// <summary>Initializes a namespace work-item-types page request.</summary>
    /// <param name="fullPath">The full path of the target project or group namespace.</param>
    /// <param name="first">The positive maximum number of nodes to request, or null to use the operation default.</param>
    /// <param name="after">The non-empty cursor after which to continue, or null for the first page.</param>
    /// <exception cref="ArgumentException"><paramref name="fullPath" /> is blank or <paramref name="after" /> is blank.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="first" /> is zero or negative.</exception>
    public GitLabWorkItemTypesQueryVariables(string fullPath, int? first = null, string? after = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullPath);

        if (first is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(first), first, "A GraphQL page size must be positive.");
        }

        if (after is not null && string.IsNullOrWhiteSpace(after))
        {
            throw new ArgumentException("A GraphQL cursor cannot be blank.", nameof(after));
        }

        FullPath = fullPath;
        First = first;
        After = after;
    }

    /// <summary>The full path of the target project or group namespace.</summary>
    [JsonPropertyName("fullPath")]
    public string FullPath { get; }

    /// <summary>The requested maximum node count, or null when the operation supplies its own default.</summary>
    [JsonPropertyName("first")]
    public int? First { get; }

    /// <summary>The cursor after which to return nodes, or null for the first page.</summary>
    [JsonPropertyName("after")]
    public string? After { get; }
}