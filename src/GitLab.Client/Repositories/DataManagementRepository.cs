using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class DataManagementRepository(IGitLabApiConnection connection) : IDataManagementRepository
{
    public IAsyncEnumerable<GitLabAdminModelRecord> ListModelRecordsAsync(string modelName,
        AdminModelListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("admin").Literal("data_management").Escaped(modelName).QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabAdminModelRecordArray,
            cancellationToken);
    }

    public Task<GitLabAdminModelRecord> RecalculateModelChecksumsAsync(string modelName,
        RecalculateModelChecksumsRequest? request = null, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("admin").Literal("data_management").Escaped(modelName).Literal("checksum")
                .Build(),
            request ?? new RecalculateModelChecksumsRequest(),
            GitLabJsonContext.Default.RecalculateModelChecksumsRequest,
            GitLabJsonContext.Default.GitLabAdminModelRecord,
            cancellationToken);
    }

    public Task<GitLabAdminModelRecord> GetModelRecordAsync(string modelName, string recordIdentifier,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("admin").Literal("data_management").Escaped(modelName)
                .Escaped(recordIdentifier).Build(),
            GitLabJsonContext.Default.GitLabAdminModelRecord,
            cancellationToken);
    }

    public Task<GitLabAdminModelRecord> RecalculateModelRecordChecksumAsync(string modelName,
        string recordIdentifier, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("admin").Literal("data_management").Escaped(modelName)
                .Escaped(recordIdentifier).Literal("checksum").Build(),
            GitLabJsonContext.Default.GitLabAdminModelRecord,
            cancellationToken);
    }

    public Task<GitLabDictionaryTable> GetDictionaryTableAsync(string databaseName, string tableName,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("admin").Literal("databases").Escaped(databaseName).Literal("dictionary")
                .Literal("tables").Escaped(tableName).Build(),
            GitLabJsonContext.Default.GitLabDictionaryTable,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabDictionaryTable> ListDictionaryTablesAsync(string databaseName,
        DictionaryTableListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("databases").Escaped(databaseName).Literal("dictionary").Literal("tables")
                .QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabDictionaryTableArray,
            cancellationToken);
    }
}