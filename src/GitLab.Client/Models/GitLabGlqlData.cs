using System.Text.Json;
using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>The payload half of a <c>POST /glql</c> result: what was found, and where the page ends.</summary>
public sealed record GitLabGlqlData
{
    /// <summary>How many items the query matched.</summary>
    public int? Count { get; init; }

    /// <summary>
    ///     The matched items. Their shape is decided by the query's own <c>fields</c> declaration and by
    ///     which data source it targets (issues, merge requests, epics, work items), so GitLab declares no
    ///     schema for it and this stays a raw <see cref="JsonElement" /> rather than a shape the API does
    ///     not promise. Read it against the keys in <see cref="GitLabGlqlResult.Fields" />.
    /// </summary>
    public JsonElement? Nodes { get; init; }

    /// <summary>Cursor pagination state. camelCase on the wire, like the rest of GLQL's GraphQL-derived output.</summary>
    [JsonPropertyName("pageInfo")]
    public GitLabGlqlPageInfo? PageInfo { get; init; }
}