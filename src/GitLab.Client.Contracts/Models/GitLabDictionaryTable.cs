namespace GitLab.Client.Models;

/// <summary>
///     One database dictionary table entry, as returned by
///     <c>GET /databases/:database_name/dictionary/tables</c> and
///     <c>GET /admin/databases/:database_name/dictionary/tables/:table_name</c> - GitLab's generated
///     documentation of which application feature owns a given database table.
/// </summary>
public sealed record GitLabDictionaryTable
{
    public required string TableName { get; init; }

    public IReadOnlyList<string>? FeatureCategories { get; init; }

    /// <summary>
    ///     The table's size classification (<c>small</c>, <c>medium</c>, <c>large</c>,
    ///     <c>over_limit</c>) as free text rather than the query-side <see cref="GitLabDictionaryTableSize" />
    ///     enum: GitLab reserves the right to introduce new classifications, and a response value the
    ///     enum does not know should not fail deserialization of an otherwise healthy payload.
    /// </summary>
    public string? TableSize { get; init; }
}