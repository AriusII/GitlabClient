using GitLab.Client.Infrastructure.Pagination;

namespace GitLab.Client.Tests.Infrastructure;

public sealed class GitLabLinkHeaderTests
{
    [Fact]
    public void GetNextPageUrl_ParsesRelNext_FromRfc5988LinkHeader()
    {
        using HttpResponseMessage response = new();
        response.Headers.TryAddWithoutValidation(
            "Link",
            "<https://gitlab.example/api/v4/projects?page=2>; rel=\"next\", <https://gitlab.example/api/v4/projects?page=1>; rel=\"prev\"");

        string? next = GitLabLinkHeader.GetNextPageUrl(response.Headers);

        Assert.Equal("https://gitlab.example/api/v4/projects?page=2", next);
    }

    [Fact]
    public void GetNextPageUrl_ReturnsNull_WhenNoLinkHeaderPresent()
    {
        using HttpResponseMessage response = new();

        Assert.Null(GitLabLinkHeader.GetNextPageUrl(response.Headers));
    }

    [Fact]
    public void GetNextPageUrl_KeepsCommasThatBelongToTheUrl()
    {
        // RFC 8288 3: a comma inside <> is part of the URI-Reference, not a link-value separator. Splitting on
        // ',' truncated the URL here and silently ended the enumeration.
        using HttpResponseMessage response = new();
        response.Headers.TryAddWithoutValidation(
            "Link",
            "<https://gitlab.example/api/v4/issues?iids[]=1,2,3&page=2>; rel=\"next\"");

        Assert.Equal("https://gitlab.example/api/v4/issues?iids[]=1,2,3&page=2",
            GitLabLinkHeader.GetNextPageUrl(response.Headers));
    }

    [Fact]
    public void GetNextPageUrl_AcceptsAnUnquotedRelation()
    {
        using HttpResponseMessage response = new();
        response.Headers.TryAddWithoutValidation("Link", "<https://gitlab.example/api/v4/projects?page=2>; rel=next");

        Assert.Equal("https://gitlab.example/api/v4/projects?page=2",
            GitLabLinkHeader.GetNextPageUrl(response.Headers));
    }

    [Fact]
    public void GetNextPageUrl_AcceptsARelationTypeList()
    {
        using HttpResponseMessage response = new();
        response.Headers.TryAddWithoutValidation(
            "Link", "<https://gitlab.example/api/v4/projects?page=2>; rel=\"next last\"");

        Assert.Equal("https://gitlab.example/api/v4/projects?page=2",
            GitLabLinkHeader.GetNextPageUrl(response.Headers));
    }

    [Fact]
    public void GetNextPageUrl_IgnoresOtherParametersOnTheSameLinkValue()
    {
        using HttpResponseMessage response = new();
        response.Headers.TryAddWithoutValidation(
            "Link",
            "<https://gitlab.example/api/v4/projects?page=2>; title=\"page, two; really\"; rel=\"next\"");

        Assert.Equal("https://gitlab.example/api/v4/projects?page=2",
            GitLabLinkHeader.GetNextPageUrl(response.Headers));
    }

    [Fact]
    public void GetNextPageUrl_ReturnsNull_WhenOnlyOtherRelationsArePresent()
    {
        using HttpResponseMessage response = new();
        response.Headers.TryAddWithoutValidation(
            "Link",
            "<https://gitlab.example/api/v4/projects?page=1>; rel=\"prev\", <https://gitlab.example/api/v4/projects?page=9>; rel=\"last\"");

        Assert.Null(GitLabLinkHeader.GetNextPageUrl(response.Headers));
    }
}