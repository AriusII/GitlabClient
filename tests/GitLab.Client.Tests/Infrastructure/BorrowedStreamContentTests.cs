using GitLab.Client.Infrastructure.Http;

namespace GitLab.Client.Tests.Infrastructure;

public sealed class BorrowedStreamContentTests
{
    [Fact]
    public async Task CopyToAsync_RestoresTheSeekableSourcePosition()
    {
        using MemoryStream source = new("prefix-payload"u8.ToArray());
        source.Position = "prefix-"u8.Length;
        long initialPosition = source.Position;
        using BorrowedStreamContent content = new(source);
        using MemoryStream destination = new();

        await content.CopyToAsync(destination, null, TestContext.Current.CancellationToken);

        Assert.Equal(initialPosition, source.Position);
        Assert.True(source.CanRead);
        Assert.Equal("payload"u8.ToArray(), destination.ToArray());
    }

    [Fact]
    public async Task CopyToAsync_WhenCancelled_RestoresTheSeekableSourcePosition()
    {
        using MemoryStream source = new("prefix-payload"u8.ToArray());
        source.Position = "prefix-"u8.Length;
        long initialPosition = source.Position;
        using BorrowedStreamContent content = new(source);
        using MemoryStream destination = new();
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            content.CopyToAsync(destination, null, cancellation.Token));

        Assert.Equal(initialPosition, source.Position);
        Assert.True(source.CanRead);
    }

    [Fact]
    public void CopyTo_RestoresTheSeekableSourcePosition()
    {
        using MemoryStream source = new("prefix-payload"u8.ToArray());
        source.Position = "prefix-"u8.Length;
        long initialPosition = source.Position;
        using BorrowedStreamContent content = new(source);
        using MemoryStream destination = new();

        content.CopyTo(destination, null, TestContext.Current.CancellationToken);

        Assert.Equal(initialPosition, source.Position);
        Assert.True(source.CanRead);
        Assert.Equal("payload"u8.ToArray(), destination.ToArray());
    }
}