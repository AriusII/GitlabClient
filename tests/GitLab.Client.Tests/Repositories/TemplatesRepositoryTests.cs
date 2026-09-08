using System.Net;
using System.Text;

using GitLab.Client.Infrastructure.Http;
using GitLab.Client.Models;
using GitLab.Client.Repositories;
using GitLab.Client.Tests.TestSupport;

namespace GitLab.Client.Tests.Repositories;

public sealed class TemplatesRepositoryTests
{
    private const string LicenseJson = """
                                       {
                                         "key": "apache-2.0",
                                         "name": "Apache License 2.0",
                                         "nickname": null,
                                         "html_url": "http://choosealicense.com/licenses/apache-2.0/",
                                         "source_url": "http://www.apache.org/licenses/LICENSE-2.0.html",
                                         "popular": true,
                                         "description": "A permissive license whose main conditions require preservation of copyright and license notices.",
                                         "conditions": ["include-copyright", "document-changes"],
                                         "permissions": ["commercial-use", "modifications", "distribution"],
                                         "limitations": ["trademark-use", "liability", "warranty"],
                                         "content": "Apache License Version 2.0, January 2004"
                                       }
                                       """;

    [Fact]
    public async Task ListDockerfilesAsync_BuildsTemplatesRoute_AndDeserializesSummaries()
    {
        const string json = """
                            [
                              { "key": "Binary", "name": "Binary" },
                              { "key": "Ruby", "name": "Ruby" }
                            ]
                            """;

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        TemplatesRepository repository = new(connection);

        List<GitLabTemplateSummary> templates = [];
        await foreach (GitLabTemplateSummary template in
                       repository.ListDockerfilesAsync(cancellationToken: TestContext.Current.CancellationToken))
        {
            templates.Add(template);
        }

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/templates/dockerfiles",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        Assert.Equal(2, templates.Count);
        Assert.Equal("Binary", templates[0].Key);
        Assert.Equal("Ruby", templates[1].Name);
    }

    [Fact]
    public async Task ListGitignoresAsync_BuildsGitignoresRoute_AndAppendsPerPage()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        TemplatesRepository repository = new(connection);

        await foreach (GitLabTemplateSummary _ in repository.ListGitignoresAsync(
                           new TemplateListOptions { PerPage = 50 }, TestContext.Current.CancellationToken))
        {
            // Draining the (empty) sequence is what issues the request.
        }

        Assert.Equal("https://gitlab.example/api/v4/templates/gitignores?per_page=50",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ListCiYmlsAsync_UsesGitLabsGitlabCiYmlsPathWord()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        TemplatesRepository repository = new(connection);

        await foreach (GitLabTemplateSummary _ in
                       repository.ListCiYmlsAsync(cancellationToken: TestContext.Current.CancellationToken))
        {
            // Draining the (empty) sequence is what issues the request.
        }

        Assert.Equal("https://gitlab.example/api/v4/templates/gitlab_ci_ymls",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }

    /// <summary>
    ///     "C++" is a real .gitignore template name. An unescaped '+' reaches GitLab as a space, so the
    ///     route builder must percent-encode it - this is the whole reason the name goes through
    ///     <c>Escaped</c> rather than <c>Literal</c>.
    /// </summary>
    [Fact]
    public async Task GetGitignoreAsync_PercentEncodesPlusSignsInTheTemplateName()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{ "name": "C++", "content": "*.o" }""", Encoding.UTF8,
                "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        TemplatesRepository repository = new(connection);

        GitLabTemplate template = await repository.GetGitignoreAsync("C++", TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/templates/gitignores/C%2B%2B",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("C++", template.Name);
        Assert.Equal("*.o", template.Content);
    }

    /// <summary>
    ///     A dot is an unreserved character, so a dotted name must survive escaping byte for byte rather
    ///     than being rewritten - and must not be collapsed as a relative-path segment by <see cref="Uri" />.
    /// </summary>
    [Fact]
    public async Task GetDockerfileAsync_RoundTripsADottedTemplateName()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{ "name": "Node.gitignore", "content": "node_modules/" }""",
                Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        TemplatesRepository repository = new(connection);

        GitLabTemplate template =
            await repository.GetDockerfileAsync("Node.gitignore", TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/templates/dockerfiles/Node.gitignore",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("Node.gitignore", template.Name);
    }

    [Fact]
    public async Task GetCiYmlAsync_BuildsGitlabCiYmlsRoute_AndDeserializesTheTemplate()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{ "name": "Ruby", "content": "image: ruby:3.3\n" }""", Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        TemplatesRepository repository = new(connection);

        GitLabTemplate template = await repository.GetCiYmlAsync("Ruby", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal("https://gitlab.example/api/v4/templates/gitlab_ci_ymls/Ruby",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("Ruby", template.Name);
        Assert.Equal("image: ruby:3.3\n", template.Content);
    }

    [Fact]
    public async Task ListLicensesAsync_AppendsPopularFilter_AndDeserializesTheFullLicenseEntity()
    {
        string json = $"[{LicenseJson}]";

        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        TemplatesRepository repository = new(connection);

        List<GitLabLicenseTemplate> licenses = [];
        await foreach (GitLabLicenseTemplate license in repository.ListLicensesAsync(
                           new LicenseTemplateListOptions { Popular = true },
                           TestContext.Current.CancellationToken))
        {
            licenses.Add(license);
        }

        Assert.Equal("https://gitlab.example/api/v4/templates/licenses?popular=true",
            handler.LastRequest?.RequestUri?.AbsoluteUri);

        GitLabLicenseTemplate only = Assert.Single(licenses);
        Assert.Equal("apache-2.0", only.Key);
        Assert.Equal("Apache License 2.0", only.Name);
        Assert.Null(only.Nickname);
        Assert.Equal(new Uri("http://choosealicense.com/licenses/apache-2.0/"), only.HtmlUrl);
        Assert.Equal(new Uri("http://www.apache.org/licenses/LICENSE-2.0.html"), only.SourceUrl);
        Assert.True(only.Popular);
        Assert.Equal(["include-copyright", "document-changes"], only.Conditions);
        Assert.Equal(["commercial-use", "modifications", "distribution"], only.Permissions);
        Assert.Equal(["trademark-use", "liability", "warranty"], only.Limitations);
        Assert.StartsWith("Apache License", only.Content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetLicenseAsync_EscapesTheNameAndCarriesTheCopyrightSubstitutions()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(LicenseJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        TemplatesRepository repository = new(connection);

        GitLabLicenseTemplate license = await repository.GetLicenseAsync(
            "Apache License 2.0",
            "GitLab Client",
            "Jane Doe",
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://gitlab.example/api/v4/templates/licenses/Apache%20License%202.0"
            + "?project=GitLab%20Client&fullname=Jane%20Doe",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
        Assert.Equal("apache-2.0", license.Key);
    }

    [Fact]
    public async Task GetLicenseAsync_OmitsTheSubstitutionParametersWhenTheyAreNotSupplied()
    {
        using StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(LicenseJson, Encoding.UTF8, "application/json")
        });

        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://gitlab.example/api/v4/") };
        GitLabApiConnection connection = new(httpClient);
        TemplatesRepository repository = new(connection);

        await repository.GetLicenseAsync("mit", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("https://gitlab.example/api/v4/templates/licenses/mit",
            handler.LastRequest?.RequestUri?.AbsoluteUri);
    }
}