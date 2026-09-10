using System.Text.Json.Serialization;

namespace GitLab.Client.GraphQL.WorkItems.Mutations;

/// <summary>The <c>variables</c> object for a curated <c>workItemCreate</c> operation.</summary>
public sealed record GitLabWorkItemCreateMutationVariables
{
    /// <summary>Initializes GraphQL variables from a create input.</summary>
    /// <param name="input">The non-null work-item create input.</param>
    /// <exception cref="ArgumentNullException"><paramref name="input" /> is null.</exception>
    public GitLabWorkItemCreateMutationVariables(GitLabWorkItemCreateInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        Input = input;
    }

    /// <summary>The input passed to the GraphQL mutation.</summary>
    [JsonPropertyName("input")]
    public GitLabWorkItemCreateInput Input { get; }
}