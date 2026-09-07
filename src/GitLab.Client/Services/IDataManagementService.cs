using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Data management and Database dictionary, sitting between the
///     public <c>IDataManagementClient</c> controller and <c>IDataManagementRepository</c>'s raw GitLab
///     access. Mirrors the repository's method shapes 1:1 today (its implementation is generated); this
///     is the seam where request validation, caching, or cross-resource composition would go once the
///     resource needs more than pass-through.
/// </summary>
internal interface IDataManagementService
{
    IAsyncEnumerable<GitLabAdminModelRecord> ListModelRecordsAsync(string modelName,
        AdminModelListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabAdminModelRecord> RecalculateModelChecksumsAsync(string modelName,
        RecalculateModelChecksumsRequest? request = null, CancellationToken cancellationToken = default);

    Task<GitLabAdminModelRecord> GetModelRecordAsync(string modelName, string recordIdentifier,
        CancellationToken cancellationToken = default);

    Task<GitLabAdminModelRecord> RecalculateModelRecordChecksumAsync(string modelName, string recordIdentifier,
        CancellationToken cancellationToken = default);

    Task<GitLabDictionaryTable> GetDictionaryTableAsync(string databaseName, string tableName,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabDictionaryTable> ListDictionaryTablesAsync(string databaseName,
        DictionaryTableListOptions? options = null, CancellationToken cancellationToken = default);
}