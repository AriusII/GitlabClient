using GitLab.Client.Models;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>Filters for <c>GET /databases/:database_name/dictionary/tables</c>.</summary>
[GitLabQuery]
public readonly record struct DictionaryTableListOptions
{
    public GitLabDictionaryTableSize? TableSize { get; init; }
}