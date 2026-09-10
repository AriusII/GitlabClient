using System.Text.Json.Serialization;

namespace GitLab.Client.GraphQL.WorkItems.Mutations;

/// <summary>The <c>variables</c> object for a curated <c>workItemUpdate</c> operation.</summary>
public sealed record GitLabWorkItemUpdateMutationVariables
{
    /// <summary>Initializes GraphQL variables from an update input.</summary>
    /// <param name="input">The non-null work-item update input.</param>
    /// <exception cref="ArgumentNullException"><paramref name="input" /> is null.</exception>
    public GitLabWorkItemUpdateMutationVariables(GitLabWorkItemUpdateInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        Input = input;
    }

    /// <summary>The input passed to the GraphQL mutation.</summary>
    [JsonPropertyName("input")]
    public GitLabWorkItemUpdateInput Input { get; }
}