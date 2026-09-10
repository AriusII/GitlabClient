using System.Text.Json.Serialization;

namespace GitLab.Client.GraphQL.WorkItems.Mutations;

/// <summary>The <c>variables</c> object for a curated <c>workItemDelete</c> operation.</summary>
public sealed record GitLabWorkItemDeleteMutationVariables
{
    /// <summary>Initializes GraphQL variables from a delete input.</summary>
    /// <param name="input">The non-null work-item delete input.</param>
    /// <exception cref="ArgumentNullException"><paramref name="input" /> is null.</exception>
    public GitLabWorkItemDeleteMutationVariables(GitLabWorkItemDeleteInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        Input = input;
    }

    /// <summary>The input passed to the GraphQL mutation.</summary>
    [JsonPropertyName("input")]
    public GitLabWorkItemDeleteInput Input { get; }
}