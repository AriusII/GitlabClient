namespace GitLab.Client.Models;

/// <summary>
///     The shard and replica configuration of one Elasticsearch index, as returned in
///     <see cref="GitLabApplicationSettings.ElasticsearchIndexSettings" />.
/// </summary>
public sealed record GitLabElasticsearchIndexSetting
{
    /// <summary>Name of the Elasticsearch index alias.</summary>
    public string? AliasName { get; init; }

    /// <summary>Number of shards in the index.</summary>
    public int? NumberOfShards { get; init; }

    /// <summary>Number of replicas per shard.</summary>
    public int? NumberOfReplicas { get; init; }
}