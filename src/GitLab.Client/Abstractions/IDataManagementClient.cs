using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps two small instance-administrator data-inspection APIs: Data management
///     (<c>/admin/data_management/:model_name</c>), which reports on and recalculates checksums for the
///     underlying records of an instance data model, and Database dictionary
///     (<c>/databases/:database_name/dictionary/tables</c>,
///     <c>/admin/databases/:database_name/dictionary/tables/:table_name</c>), GitLab's generated map of
///     which application feature category owns each database table.
///     <para>
///         Every Data management method here is available only to instance administrators, and the two
///         checksum-recalculation methods only on a Geo primary site; GitLab answers <c>403</c>
///         otherwise. The valid <c>modelName</c> values are the ones documented at
///         <c>https://docs.gitlab.com/administration/admin_area/#data-management</c> - GitLab does not
///         expose them through the API itself, so this client cannot validate the name before sending it.
///     </para>
/// </summary>
public interface IDataManagementClient
{
    /// <summary>
    ///     Streams the records of an instance data model, most recently checksummed first unless
    ///     <paramref name="options" /> says otherwise.
    /// </summary>
    IAsyncEnumerable<GitLabAdminModelRecord> ListModelRecordsAsync(string modelName,
        AdminModelListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Enqueues a background job to recalculate the checksum of every record of a model, or only the
    ///     ones <paramref name="request" /> selects. Instance administrators on a Geo primary site only.
    /// </summary>
    Task<GitLabAdminModelRecord> RecalculateModelChecksumsAsync(string modelName,
        RecalculateModelChecksumsRequest? request = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets one record of an instance data model. <paramref name="recordIdentifier" /> is a number for
    ///     some models and free text for others; pass it exactly as GitLab reports it.
    /// </summary>
    Task<GitLabAdminModelRecord> GetModelRecordAsync(string modelName, string recordIdentifier,
        CancellationToken cancellationToken = default);

    /// <summary>Recalculates the checksum of one model record. Instance administrators on a Geo primary site only.</summary>
    Task<GitLabAdminModelRecord> RecalculateModelRecordChecksumAsync(string modelName, string recordIdentifier,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one database table's dictionary entry - its owning feature categories and size classification.</summary>
    Task<GitLabDictionaryTable> GetDictionaryTableAsync(string databaseName, string tableName,
        CancellationToken cancellationToken = default);

    /// <summary>Streams every dictionary table entry for a database.</summary>
    IAsyncEnumerable<GitLabDictionaryTable> ListDictionaryTablesAsync(string databaseName,
        DictionaryTableListOptions? options = null, CancellationToken cancellationToken = default);
}