using GitLab.Client.Abstractions;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Data management and Database dictionary resource: builds routes
///     via <see cref="Infrastructure.Routing.GitLabRouteBuilder" /> and calls
///     <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this layer should
///     build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IDataManagementService), typeof(IDataManagementClient))]
internal interface IDataManagementRepository
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