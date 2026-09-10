using GitLab.Client.Domain;

namespace GitLab.Client.Tests.Domain;

public sealed class ProjectIdTests
{
    [Fact]
    public void FromId_WithPositiveValue_FormatsAnInvariantNumericRouteValue()
    {
        ProjectId projectId = ProjectId.FromId(42);

        Assert.True(projectId.IsNumeric);
        Assert.Equal("42", projectId.ToRouteValue());
        Assert.Equal("42", projectId.ToString());
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    public void FromId_WithNonPositiveValue_ThrowsArgumentOutOfRangeException(long id)
    {
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(() => ProjectId.FromId(id));

        Assert.Equal("id", exception.ParamName);
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    public void ImplicitLongConversion_WithNonPositiveValue_ThrowsArgumentOutOfRangeException(long id)
    {
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            ProjectId _ = id;
        });

        Assert.Equal("id", exception.ParamName);
    }

    [Fact]
    public void FromPath_WithRawNamespacedPath_PercentEncodesTheRouteValue()
    {
        ProjectId projectId = ProjectId.FromPath("group/subgroup/project");

        Assert.False(projectId.IsNumeric);
        Assert.Equal("group/subgroup/project", projectId.ToString());
        Assert.Equal("group%2Fsubgroup%2Fproject", projectId.ToRouteValue());
    }

    [Fact]
    public void FromPath_WithAlreadyEncodedPath_IntentionallyEncodesThePercentSignAgain()
    {
        ProjectId projectId = ProjectId.FromPath("group%2Fproject");

        Assert.Equal("group%252Fproject", projectId.ToRouteValue());
    }
}