using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Filters for <c>GET /databases/:database_name/dictionary/tables</c>.</summary>
[GitLabQuery]
public sealed record DictionaryTableListOptions
{
    public GitLabDictionaryTableSize? TableSize { get; init; }
}