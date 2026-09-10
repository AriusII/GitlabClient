using System.Globalization;
using System.Text.Json.Serialization;

using GitLab.Client.GraphQL.WorkItems.Identifiers;

namespace GitLab.Client.GraphQL.WorkItems.Queries;

/// <summary>Variables for a curated <c>namespace { workItem(iid: …) }</c> GraphQL query.</summary>
public sealed record GitLabWorkItemByLocatorQueryVariables
{
    /// <summary>Initializes query variables from a validated work-item locator.</summary>
    /// <param name="locator">The namespace/IID locator to serialize for the query.</param>
    /// <exception cref="ArgumentNullException"><paramref name="locator" /> is null.</exception>
    public GitLabWorkItemByLocatorQueryVariables(GitLabWorkItemLocator locator)
    {
        ArgumentNullException.ThrowIfNull(locator);
        FullPath = locator.NamespacePath;
        Iid = locator.Iid.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>The containing namespace's full path.</summary>
    [JsonPropertyName("fullPath")]
    public string FullPath { get; }

    /// <summary>The namespace-local IID, encoded as the GraphQL <c>String</c> scalar GitLab declares.</summary>
    [JsonPropertyName("iid")]
    public string Iid { get; }
}