using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;

namespace GitLab.Client.Tests.Infrastructure;

/// <summary>
///     Pins the encoding contract every route in the library depends on. GitLab silently ignores an
///     unknown query parameter and answers 200 with unfiltered results, so a wrong name or a wrongly
///     escaped separator is invisible at runtime — these assertions are where it shows up instead.
/// </summary>
public sealed class GitLabRouteBuilderTests
{
    private static readonly Uri BaseAddress = new("https://gitlab.example/api/v4/");

    private static readonly string[] LabelsWithSpaceAndComma = ["bug", "needs review", "a,b"];

    private static readonly string[] BlankLabels = ["", "   "];

    private static readonly string[] TwoLabels = ["bug", "needs review"];

    private static readonly long[] TwoIds = [41, 42];

    private static readonly long[] OneId = [41];

    [Fact]
    public void Segment_AppendsLiteralWordsUnescaped_AndNamespacedIdsPercentEncoded()
    {
        Uri route = GitLabRouteBuilder.Create("projects")
            .Segment(ProjectId.FromPath("gitlab-org/gitlab"))
            .Literal("repository")
            .Literal("commits")
            .Segment(47)
            .Build();

        Assert.Equal("projects/gitlab-org%2Fgitlab/repository/commits/47", route.OriginalString);
    }

    [Fact]
    public void Query_EscapesTheValue_AndSeparatesWithQuestionMarkThenAmpersand()
    {
        Uri route = GitLabRouteBuilder.Create("projects")
            .Query("search", "^v 1.0")
            .Query("per_page", 20)
            .Query("simple", true)
            .Build();

        Assert.Equal("projects?search=%5Ev%201.0&per_page=20&simple=true", route.OriginalString);
    }

    [Fact]
    public void Query_SkipsNullAndBlankValues_WithoutLeavingAStraySeparator()
    {
        Uri route = GitLabRouteBuilder.Create("projects")
            .Query("search", (string?)null)
            .Query("regex", "   ")
            .Query("per_page", (int?)null)
            .Query("archived", (bool?)null)
            .Query("topic_id", (long?)null)
            .Query("per_page", 20)
            .Build();

        Assert.Equal("projects?per_page=20", route.OriginalString);
    }

    [Fact]
    public void Query_Int64_UsesInvariantFormatting()
    {
        Uri route = GitLabRouteBuilder.Create("projects").Query("author_id", 9_007_199_254_740_993L).Build();

        Assert.Equal("projects?author_id=9007199254740993", route.OriginalString);
    }

    [Fact]
    public void Query_DateTimeOffset_NormalisesToUtc_AndUsesTheDocumentedIso8601Shape()
    {
        Uri route = GitLabRouteBuilder.Create("projects")
            .Query("updated_after", new DateTimeOffset(2024, 1, 2, 5, 4, 5, TimeSpan.FromHours(2)))
            .Build();

        // A caller in +02:00 must not silently shift the filter window, and ':' is a legal query
        // character unescaped, so the value stays readable rather than becoming %3A.
        Assert.Equal("projects?updated_after=2024-01-02T03:04:05Z", route.OriginalString);
    }

    [Fact]
    public void Query_DateOnly_UsesTheIsoCalendarDate()
    {
        Uri route = GitLabRouteBuilder.Create("projects")
            .Query("marked_for_deletion_on", new DateOnly(2024, 1, 2))
            .Build();

        Assert.Equal("projects?marked_for_deletion_on=2024-01-02", route.OriginalString);
    }

    [Fact]
    public void Query_StringList_JoinsWithCommas_AndEscapesEachElementSeparately()
    {
        Uri route = GitLabRouteBuilder.Create("projects").Query("labels", LabelsWithSpaceAndComma).Build();

        // The separators stay literal so the URL is readable; the element that legitimately contains a
        // comma is escaped to %2C, so the server cannot mistake it for a separator.
        Assert.Equal("projects?labels=bug,needs%20review,a%2Cb", route.OriginalString);
    }

    [Fact]
    public void Query_StringList_WhenEveryElementIsBlank_OmitsTheParameterEntirely()
    {
        Uri route = GitLabRouteBuilder.Create("projects")
            .Query("labels", BlankLabels)
            .Query("per_page", 20)
            .Build();

        Assert.Equal("projects?per_page=20", route.OriginalString);
    }

    [Fact]
    public void Query_Int64List_JoinsWithCommas_AndOmitsAnEmptyList()
    {
        Uri route = GitLabRouteBuilder.Create("projects")
            .Query("iids", Array.Empty<long>())
            .Query("skip_groups", TwoIds)
            .Build();

        Assert.Equal("projects?skip_groups=41,42", route.OriginalString);
    }

    [Fact]
    public void QueryRepeated_WritesOneBracketedParameterPerValue()
    {
        Uri route = GitLabRouteBuilder.Create("projects")
            .QueryRepeated("iids", TwoIds)
            .QueryRepeated("labels", TwoLabels)
            .Build();

        Assert.Equal("projects?iids[]=41&iids[]=42&labels[]=bug&labels[]=needs%20review", route.OriginalString);
    }

    [Fact]
    public void QueryRepeated_WithNullList_AddsNothing()
    {
        Uri route = GitLabRouteBuilder.Create("projects")
            .QueryRepeated("iids", (IReadOnlyList<long>?)null)
            .QueryRepeated("labels", (IReadOnlyList<string>?)null)
            .Build();

        Assert.Equal("projects", route.OriginalString);
    }

    [Fact]
    public void Build_ProducesARelativeUri_WhoseUnescapedCharactersSurviveCombinationWithTheBaseAddress()
    {
        Uri route = GitLabRouteBuilder.Create("projects")
            .Query("labels", LabelsWithSpaceAndComma)
            .Query("updated_after", new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero))
            .QueryRepeated("iids", OneId)
            .Build();

        Assert.False(route.IsAbsoluteUri);

        // ',' ':' '[' and ']' are appended raw rather than escaped. This is what proves that choice is
        // safe: HttpClient combines the relative route with its BaseAddress before sending it, and the
        // result has to come out byte for byte.
        Assert.Equal(
            "https://gitlab.example/api/v4/projects?labels=bug,needs%20review,a%2Cb" +
            "&updated_after=2024-01-02T03:04:05Z&iids[]=41",
            new Uri(BaseAddress, route).AbsoluteUri);
    }

    [Fact]
    public void EscapedTemplate_SubstitutesEachValueEscapedIndependently_IntoOneSegment()
    {
        Uri route = GitLabRouteBuilder.Create("projects")
            .Segment(7)
            .Literal("packages").Literal("nuget").Literal("v2")
            .EscapedTemplate("Packages(Id='{0}',Version='{1}')", "My.Pkg", "1.0.0")
            .Build();

        Assert.Equal("projects/7/packages/nuget/v2/Packages(Id='My.Pkg',Version='1.0.0')", route.OriginalString);
    }

    [Fact]
    public void EscapedTemplate_EscapesAValueContainingATemplateCharacter_RatherThanLettingItBreakOutOfItsSlot()
    {
        Uri route = GitLabRouteBuilder.Create("projects")
            .Segment(7)
            .EscapedTemplate("Packages(Id='{0}',Version='{1}')", "it's/tricky", "1.0.0")
            .Build();

        Assert.Equal(
            "projects/7/Packages(Id='it%27s%2Ftricky',Version='1.0.0')",
            route.OriginalString);
    }

    // The three tests below pin the pooled-StringBuilder allocation strategy added on top of the
    // encoding contract above: they prove reuse never leaks state between routes on the same thread,
    // that a nested/reentrant Create() cannot corrupt the outer instance's buffer, and that concurrent
    // threads never observe each other's in-progress route. None of them touch what gets built - that
    // contract is exhaustively covered above and must never change.

    [Fact]
    public void SuccessiveBuilds_OnTheSameThread_DoNotLeakStateBetweenRoutes()
    {
        Uri first = GitLabRouteBuilder.Create("projects")
            .Segment(1)
            .Literal("repository")
            .Query("ref", "main")
            .Build();

        // A much shorter second route reuses the same thread-static buffer. If Build() failed to reset
        // it, this would still carry "projects/1/repository?ref=main" as a prefix or leftover suffix.
        Uri second = GitLabRouteBuilder.Create("groups").Segment(2).Build();

        Assert.Equal("projects/1/repository?ref=main", first.OriginalString);
        Assert.Equal("groups/2", second.OriginalString);

        // A third cycle proves the buffer is reusable indefinitely, not just once.
        Uri third = GitLabRouteBuilder.Create("users").Query("username", "octocat").Build();
        Assert.Equal("users?username=octocat", third.OriginalString);
    }

    [Fact]
    public void NestedCreate_WhileAnOuterBuilderIsStillOpen_ProducesCorrectOutputForBoth()
    {
        GitLabRouteBuilder outer = GitLabRouteBuilder.Create("projects").Segment(1);

        // Constructing a second builder before the outer one calls Build() simulates the one scenario
        // that would corrupt a naively-shared thread-static buffer: a nested/reentrant Create(). The
        // fallback path (a fresh StringBuilder for the inner instance) must keep both routes intact.
        Uri inner = GitLabRouteBuilder.Create("groups").Segment(2).Literal("subgroups").Build();
        Uri outerRoute = outer.Literal("repository").Literal("commits").Build();

        Assert.Equal("groups/2/subgroups", inner.OriginalString);
        Assert.Equal("projects/1/repository/commits", outerRoute.OriginalString);
    }

    [Fact]
    public async Task ConcurrentBuilds_OnDifferentThreads_NeverObserveEachOthersRoute()
    {
        const int threadCount = 8;
        const int iterationsPerThread = 200;

        Task[] tasks = new Task[threadCount];

        for (int threadIndex = 0; threadIndex < threadCount; threadIndex++)
        {
            long projectId = threadIndex + 1;

            tasks[threadIndex] = Task.Run(() =>
            {
                string expected = $"projects/{projectId}/repository/commits?ref_name=branch-{projectId}";

                for (int iteration = 0; iteration < iterationsPerThread; iteration++)
                {
                    Uri route = GitLabRouteBuilder.Create("projects")
                        .Segment(projectId)
                        .Literal("repository")
                        .Literal("commits")
                        .Query("ref_name", $"branch-{projectId}")
                        .Build();

                    Assert.Equal(expected, route.OriginalString);
                }
            }, TestContext.Current.CancellationToken);
        }

        await Task.WhenAll(tasks).WaitAsync(TestContext.Current.CancellationToken);
    }
}
