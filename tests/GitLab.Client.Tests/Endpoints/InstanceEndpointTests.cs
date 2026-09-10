using System.Net;
using System.Text;

using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Endpoints;
using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Endpoints;

public sealed class InstanceEndpointTests
{
    [Fact]
    public async Task GetAppearanceAsync_BuildsAppearanceRoute_AndDeserializesTheResponse()
    {
        const string Json = """
                            {
                              "title": "GitLab",
                              "description": "Welcome",
                              "pwa_name": "GitLab PWA",
                              "pwa_short_name": "GL",
                              "pwa_description": "GitLab as an app",
                              "logo": "/uploads/-/system/appearance/logo/1/logo.png",
                              "pwa_icon": "/uploads/-/system/appearance/pwa_icon/1/icon.png",
                              "header_logo": "/uploads/-/system/appearance/header_logo/1/header.png",
                              "favicon": "/uploads/-/system/appearance/favicon/1/favicon.png",
                              "new_project_guidelines": "Read the handbook",
                              "member_guidelines": "Be kind",
                              "profile_image_guidelines": "Use a portrait",
                              "header_message": "Scheduled maintenance",
                              "footer_message": "GitLab 19.4",
                              "message_background_color": "#e75e40",
                              "message_font_color": "#ffffff",
                              "email_header_and_footer_enabled": true,
                              "site_name": "My GitLab"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        InstanceClient repository = new(connection);

        GitLabAppearance appearance = await repository.GetAppearanceAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/application/appearance",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("GitLab", appearance.Title);
        Assert.Equal("Welcome", appearance.Description);
        Assert.Equal("GitLab PWA", appearance.PwaName);
        Assert.Equal("GL", appearance.PwaShortName);
        Assert.Equal("GitLab as an app", appearance.PwaDescription);
        Assert.True(appearance.EmailHeaderAndFooterEnabled);
        Assert.Equal("/uploads/-/system/appearance/logo/1/logo.png", appearance.Logo?.ToString());
        Assert.Equal("/uploads/-/system/appearance/pwa_icon/1/icon.png", appearance.PwaIcon?.ToString());
        Assert.Equal("/uploads/-/system/appearance/header_logo/1/header.png", appearance.HeaderLogo?.ToString());
        Assert.Equal("/uploads/-/system/appearance/favicon/1/favicon.png", appearance.Favicon?.ToString());
        Assert.Equal("Read the handbook", appearance.NewProjectGuidelines);
        Assert.Equal("Be kind", appearance.MemberGuidelines);
        Assert.Equal("Use a portrait", appearance.ProfileImageGuidelines);
        Assert.Equal("Scheduled maintenance", appearance.HeaderMessage);
        Assert.Equal("GitLab 19.4", appearance.FooterMessage);
        Assert.Equal("#e75e40", appearance.MessageBackgroundColor);
        Assert.Equal("#ffffff", appearance.MessageFontColor);
        Assert.Equal("My GitLab", appearance.SiteName);
    }

    [Fact]
    public async Task UpdateAppearanceAsync_PutsOnlyTheChangedFields_AsMultipartFormData()
    {
        const string ResponseJson = """{ "title": "GitLab Test Instance", "site_name": "My GitLab" }""";

        string? sentBody = null;
        string? contentType = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            contentType = request.Content?.Headers.ContentType?.MediaType;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(ResponseJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        InstanceClient repository = new(connection);

        UpdateApplicationAppearanceRequest request = new()
        {
            Title = "GitLab Test Instance", EmailHeaderAndFooterEnabled = true
        };

        GitLabAppearance appearance =
            await repository.UpdateAppearanceAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/application/appearance",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("multipart/form-data", contentType);
        Assert.NotNull(sentBody);
        string body = sentBody.Replace("\"", string.Empty, StringComparison.Ordinal);
        Assert.Contains("name=title", body, StringComparison.Ordinal);
        Assert.Contains("GitLab Test Instance", body, StringComparison.Ordinal);
        Assert.Contains("name=email_header_and_footer_enabled", body, StringComparison.Ordinal);
        Assert.Contains("true", body, StringComparison.Ordinal);
        Assert.DoesNotContain("name=logo", body, StringComparison.Ordinal);
        Assert.Equal("GitLab Test Instance", appearance.Title);
        Assert.Equal("My GitLab", appearance.SiteName);
    }

    [Theory]
    [InlineData("logo")]
    [InlineData("header_logo")]
    [InlineData("pwa_icon")]
    [InlineData("favicon")]
    public async Task SetAppearanceImageAsync_PutsMultipart_UnderTheFieldNameGitLabExpects(string fieldName)
    {
        const string ResponseJson = """{ "title": "GitLab" }""";

        string? sentBody = null;
        string? contentType = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            contentType = request.Content?.Headers.ContentType?.MediaType;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(ResponseJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        InstanceClient repository = new(connection);

        using MemoryStream content = new("PNG-BYTES"u8.ToArray());

        // FieldName is left at its "file" default on purpose: the repository must override it, because
        // GitLab answers 200 and silently ignores an image part sent under any other name.
        GitLabFileUpload upload = new() { Content = content, FileName = "image.png", ContentType = "image/png" };

        GitLabAppearance appearance = fieldName switch
        {
            "logo" => await repository.SetAppearanceLogoAsync(upload, TestContext.Current.CancellationToken),
            "header_logo" => await repository.SetAppearanceHeaderLogoAsync(upload,
                TestContext.Current.CancellationToken),
            "pwa_icon" => await repository.SetAppearancePwaIconAsync(upload, TestContext.Current.CancellationToken),
            "favicon" => await repository.SetAppearanceFaviconAsync(upload, TestContext.Current.CancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(fieldName), fieldName, "Unknown field name.")
        };

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/application/appearance",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("multipart/form-data", contentType);
        string body = (sentBody ?? string.Empty).Replace("\"", string.Empty, StringComparison.Ordinal);
        Assert.Contains($"name={fieldName}", body, StringComparison.Ordinal);
        Assert.DoesNotContain("name=file;", body, StringComparison.Ordinal);
        Assert.Contains("filename=image.png", body, StringComparison.Ordinal);
        Assert.Equal("GitLab", appearance.Title);
    }

    [Fact]
    public async Task GetSettingsAsync_BuildsSettingsRoute_AndDeserializesTheResponse()
    {
        const string Json = """
                            {
                              "id": "1",
                              "default_projects_limit": "100000",
                              "signup_enabled": "true",
                              "default_ci_config_path": ".gitlab-ci.yml"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        InstanceClient repository = new(connection);

        GitLabApplicationSettings settings = await repository.GetSettingsAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/application/settings",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("1", settings.Id);
        Assert.Equal("100000", settings.DefaultProjectsLimit);
        Assert.Equal(".gitlab-ci.yml", settings.DefaultCiConfigPath);
    }

    [Fact]
    public async Task GetSettingsAsync_DeserializesTheGitLab19_4LateSchemaMembers()
    {
        const string Json = """
                            {
                              "auto_accept_awarded_achievements": "true",
                              "audit_events_api_limit": "200",
                              "built_in_project_templates_enabled": true,
                              "concurrent_pull_request_import_jobs_limit": "200",
                              "email_otp_enabled": "true",
                              "tags_create_limit": "5"
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        InstanceClient repository = new(new GitLabApiConnection(httpClient));

        GitLabApplicationSettings settings = await repository.GetSettingsAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("true", settings.AutoAcceptAwardedAchievements);
        Assert.Equal("200", settings.AuditEventsApiLimit);
        Assert.Equal(true, settings.BuiltInProjectTemplatesEnabled);
        Assert.Equal("200", settings.ConcurrentPullRequestImportJobsLimit);
        Assert.Equal("true", settings.EmailOtpEnabled);
        Assert.Equal("5", settings.TagsCreateLimit);
    }

    [Fact]
    public async Task UpdateSettingsAsync_PutsOnlyTheChangedFields_InDeclarationOrder()
    {
        const string ResponseJson = """{ "id": "1", "admin_mode": "true" }""";

        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(ResponseJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        InstanceClient repository = new(connection);

        UpdateApplicationSettingsRequest request = new()
        {
            AdminMode = true,
            DefaultBranchProtectionDefaults = new GitLabBranchProtectionDefaults
            {
                AllowedToPush = [new GitLabBranchProtectionAccessRequirement { AccessLevel = 40 }],
                AllowForcePush = false
            },
            DefaultGroupVisibility = GitLabVisibility.Internal,
            EnabledGitAccessProtocol = GitLabGitAccessProtocol.Ssh
        };

        GitLabApplicationSettings settings =
            await repository.UpdateSettingsAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/application/settings",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(
            """
            {"admin_mode":true,"default_branch_protection_defaults":{"allowed_to_push":[{"access_level":40}],"allow_force_push":false},"default_group_visibility":"internal","enabled_git_access_protocol":"ssh"}
            """,
            sentBody);
        Assert.Equal("1", settings.Id);
    }

    [Fact]
    public async Task GetStatisticsAsync_BuildsStatisticsRoute_AndDeserializesCounts()
    {
        const string Json = """{ "projects": 42, "users": 100, "active_users": 87 }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        InstanceClient repository = new(connection);

        GitLabApplicationStatistics statistics =
            await repository.GetStatisticsAsync(TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/application/statistics",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(42, statistics.Projects);
        Assert.Equal(87, statistics.ActiveUsers);
    }

    [Fact]
    public async Task GetMetadataAsync_BuildsMetadataRoute_AndDeserializesTheCamelCaseKasObject()
    {
        const string Json = """
                            {
                              "version": "18.4-pre",
                              "revision": "abc1234",
                              "kas": {
                                "enabled": true,
                                "externalUrl": "grpc://gitlab.example.com:8150",
                                "externalK8sProxyUrl": "https://gitlab.example.com:8150/k8s-proxy",
                                "version": "18.4.0"
                              },
                              "enterprise": false
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        InstanceClient repository = new(connection);

        GitLabMetadata metadata = await repository.GetMetadataAsync(TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/metadata", handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("18.4-pre", metadata.Version);
        Assert.False(metadata.Enterprise);
        Assert.NotNull(metadata.Kas);
        Assert.True(metadata.Kas!.Enabled);
        Assert.Equal("grpc://gitlab.example.com:8150", metadata.Kas.ExternalUrl);
        Assert.Equal("https://gitlab.example.com:8150/k8s-proxy", metadata.Kas.ExternalK8sProxyUrl);
    }

    [Fact]
    public async Task GetPlanLimitsAsync_WithNoOptions_OmitsTheQueryString()
    {
        const string Json = """{ "ci_active_jobs": 500, "storage_size_limit": 15000 }""";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        InstanceClient repository = new(connection);

        GitLabPlanLimits limits =
            await repository.GetPlanLimitsAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/application/plan_limits",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal(500, limits.CiActiveJobs);
        Assert.Equal(15000, limits.StorageSizeLimit);
    }

    [Fact]
    public async Task GetPlanLimitsAsync_WithPlanName_AddsThePlanNameQueryParameter_AndDeserializesHistory()
    {
        const string Json = """
                            {
                              "ci_active_jobs": 500,
                              "limits_history": {
                                "enforcement_limit": [
                                  { "timestamp": 1686909124, "user_id": 1, "username": "alice", "value": 5 }
                                ]
                              }
                            }
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        InstanceClient repository = new(connection);

        PlanLimitsOptions options = new() { PlanName = GitLabPlanName.Ultimate };

        GitLabPlanLimits limits =
            await repository.GetPlanLimitsAsync(options, TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/application/plan_limits?plan_name=ultimate",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.NotNull(limits.LimitsHistory);
        IReadOnlyList<GitLabPlanLimitHistoryEntry> entries = limits.LimitsHistory!["enforcement_limit"];
        GitLabPlanLimitHistoryEntry entry = Assert.Single(entries);
        Assert.Equal(1686909124, entry.Timestamp);
        Assert.Equal("alice", entry.Username);
        Assert.Equal(5, entry.Value);
    }

    [Fact]
    public async Task UpdatePlanLimitsAsync_PutsThePlanNameAndLimits()
    {
        const string ResponseJson = """{ "ci_active_jobs": 1000 }""";

        string? sentBody = null;
        using StubHttpMessageHandler handler = new(request =>
        {
            sentBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(ResponseJson, Encoding.UTF8, "application/json")
            };
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        InstanceClient repository = new(connection);

        UpdatePlanLimitsRequest request = new() { PlanName = GitLabPlanName.Ultimate, CiActiveJobs = 1000 };

        GitLabPlanLimits limits =
            await repository.UpdatePlanLimitsAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/application/plan_limits",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("""{"plan_name":"ultimate","ci_active_jobs":1000}""", sentBody);
        Assert.Equal(1000, limits.CiActiveJobs);
    }

    [Fact]
    public async Task UpdatePlanLimitsAsync_WithANullRequest_ThrowsBeforeSendingAnHttpRequest()
    {
        using StubHttpMessageHandler handler = new(_ =>
            throw new InvalidOperationException("A null request must not reach the HTTP transport."));

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        InstanceClient repository = new(new GitLabApiConnection(httpClient));

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            repository.UpdatePlanLimitsAsync(null!, TestContext.Current.CancellationToken));
    }
}