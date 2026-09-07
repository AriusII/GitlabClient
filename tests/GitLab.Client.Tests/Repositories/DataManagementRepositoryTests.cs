using System.Net;
using System.Text;
using System.Text.Json;

using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class DataManagementRepositoryTests
{
    private const string ModelRecordJson = """
                                           {
                                             "record_identifier": 42,
                                             "model_class": "Upload",
                                             "created_at": "2025-01-31T15:10:45.080Z",
                                             "file_size": 123,
                                             "checksum_information": {
                                               "checksum": "abc123",
                                               "last_checksum": "def456",
                                               "checksum_state": "succeeded",
                                               "checksum_retry_count": "0",
                                               "checksum_retry_at": null,
                                               "checksum_failure": null
                                             }
                                           }
                                           """;

    private const string StringIdentifierModelRecordJson = """
                                                           {
                                                             "record_identifier": "abc123",
                                                             "model_class": "Project",
                                                             "created_at": "2025-01-31T15:10:45.080Z"
                                                           }
                                                           """;

    private const string DictionaryTableJson = """
                                               {
                                                 "table_name": "users",
                                                 "feature_categories": ["users"],
                                                 "table_size": "large"
                                               }
                                               """;

    [Fact]
    public async Task ListModelRecordsAsync_AppliesEveryFilter_AndDeserializesTheNumericRecordIdentifier()
    {
        string json = $"[{ModelRecordJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DataManagementRepository repository = new(connection);

        AdminModelListOptions options = new()
        {
            Page = 1,
            PerPage = 20,
            Identifiers = ["1", "2"],
            ChecksumState = "pending",
            Cursor = "abc",
            Sort = AdminModelSortDirection.Ascending
        };

        List<GitLabAdminModelRecord> records = new();
        await foreach (GitLabAdminModelRecord item in
                       repository.ListModelRecordsAsync("uploads", options, TestContext.Current.CancellationToken))
        {
            records.Add(item);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://gitlab.example/api/v4/admin/data_management/uploads"
            + "?page=1&per_page=20&identifiers=1,2&checksum_state=pending&cursor=abc&sort=asc",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabAdminModelRecord record = Assert.Single(records);
        Assert.Equal(JsonValueKind.Number, record.RecordIdentifier?.ValueKind);
        Assert.Equal(42, record.RecordIdentifier?.GetInt32());
        Assert.Equal("Upload", record.ModelClass);
        Assert.Equal(123, record.FileSize);
        Assert.Equal("succeeded", record.ChecksumInformation?.ChecksumState);
        Assert.Equal("0", record.ChecksumInformation?.ChecksumRetryCount);
    }

    [Fact]
    public async Task RecalculateModelChecksumsAsync_PutsIdentifiersAndChecksumState()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(ModelRecordJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DataManagementRepository repository = new(connection);

        await repository.RecalculateModelChecksumsAsync("uploads",
            new RecalculateModelChecksumsRequest { Identifiers = ["1", "2"], ChecksumState = "failed" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin/data_management/uploads/checksum",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"identifiers":["1","2"],"checksum_state":"failed"}""", sentBody);
    }

    [Fact]
    public async Task RecalculateModelChecksumsAsync_WithNoRequest_SendsAnEmptyBody()
    {
        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(ModelRecordJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DataManagementRepository repository = new(connection);

        await repository.RecalculateModelChecksumsAsync("uploads",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("{}", sentBody);
    }

    [Fact]
    public async Task GetModelRecordAsync_EscapesModelNameAndRecordIdentifier_AndDeserializesAStringIdentifier()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(StringIdentifierModelRecordJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DataManagementRepository repository = new(connection);

        GitLabAdminModelRecord record = await repository.GetModelRecordAsync("user uploads", "some id",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin/data_management/user%20uploads/some%20id",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal(JsonValueKind.String, record.RecordIdentifier?.ValueKind);
        Assert.Equal("abc123", record.RecordIdentifier?.GetString());
        Assert.Equal("Project", record.ModelClass);
        Assert.Null(record.ChecksumInformation);
    }

    [Fact]
    public async Task RecalculateModelRecordChecksumAsync_SendsNoBody_AndReturnsTheRecord()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ModelRecordJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DataManagementRepository repository = new(connection);

        GitLabAdminModelRecord record = await repository.RecalculateModelRecordChecksumAsync("uploads", "42",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin/data_management/uploads/42/checksum",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Null(handler.LastRequest?.Content);
        Assert.Equal("Upload", record.ModelClass);
    }

    [Fact]
    public async Task GetDictionaryTableAsync_BuildsTheAdminRoute()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(DictionaryTableJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DataManagementRepository repository = new(connection);

        GitLabDictionaryTable table = await repository.GetDictionaryTableAsync("main", "users",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/admin/databases/main/dictionary/tables/users",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("users", table.TableName);
        Assert.Equal("large", table.TableSize);
        Assert.Equal("users", Assert.Single(table.FeatureCategories!));
    }

    [Fact]
    public async Task ListDictionaryTablesAsync_BuildsTheNonAdminRoute_WithTableSizeFilter()
    {
        string json = $"[{DictionaryTableJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        DataManagementRepository repository = new(connection);

        List<GitLabDictionaryTable> tables = new();
        await foreach (GitLabDictionaryTable item in repository.ListDictionaryTablesAsync("main",
                           new DictionaryTableListOptions { TableSize = GitLabDictionaryTableSize.Large },
                           TestContext.Current.CancellationToken))
        {
            tables.Add(item);
        }

        Assert.Equal("https://gitlab.example/api/v4/databases/main/dictionary/tables?table_size=large",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("users", Assert.Single(tables).TableName);
    }
}