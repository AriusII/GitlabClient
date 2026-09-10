using GitLab.Client.Domain;

namespace GitLab.Client.Tests.Domain;

public sealed class GroupIdTests
{
    [Fact]
    public void FromId_WithPositiveValue_FormatsAnInvariantNumericRouteValue()
    {
        GroupId groupId = GroupId.FromId(42);

        Assert.True(groupId.IsNumeric);
        Assert.Equal("42", groupId.ToRouteValue());
        Assert.Equal("42", groupId.ToString());
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    public void FromId_WithNonPositiveValue_ThrowsArgumentOutOfRangeException(long id)
    {
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(() => GroupId.FromId(id));

        Assert.Equal("id", exception.ParamName);
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    public void ImplicitLongConversion_WithNonPositiveValue_ThrowsArgumentOutOfRangeException(long id)
    {
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            GroupId _ = id;
        });

        Assert.Equal("id", exception.ParamName);
    }

    [Fact]
    public void FromPath_WithRawFullPath_PercentEncodesTheRouteValue()
    {
        GroupId groupId = GroupId.FromPath("group/subgroup");

        Assert.False(groupId.IsNumeric);
        Assert.Equal("group/subgroup", groupId.ToString());
        Assert.Equal("group%2Fsubgroup", groupId.ToRouteValue());
    }

    [Fact]
    public void FromPath_WithAlreadyEncodedPath_IntentionallyEncodesThePercentSignAgain()
    {
        GroupId groupId = GroupId.FromPath("group%2Fsubgroup");

        Assert.Equal("group%252Fsubgroup", groupId.ToRouteValue());
    }
}