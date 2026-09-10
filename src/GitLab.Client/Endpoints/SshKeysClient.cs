using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class SshKeysClient(IGitLabApiConnection connection) : ISshKeysClient
{
    /// <summary>
    ///     The singular root: GitLab's alias for the account the request is authenticated as. One letter
    ///     apart from <see cref="NamedUserRoot" />, and operating on the wrong one silently touches the
    ///     wrong account - which is why the two are named constants rather than inline literals.
    /// </summary>
    private const string CurrentUserRoot = "user";

    /// <summary>The plural root, addressing a user by numeric ID.</summary>
    private const string NamedUserRoot = "users";

    private const string Keys = "keys";

    /// <summary>The group-level SSH certificate authorities, which share GitLab's "Keys" API tag.</summary>
    private const string SshCertificates = "ssh_certificates";

    private const string Groups = "groups";

    public IAsyncEnumerable<GitLabSshKey> ListForCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create(CurrentUserRoot)
                .Literal(Keys)
                .Build(),
            GitLabJsonContext.Default.GitLabSshKeyArray,
            cancellationToken);
    }

    public Task<GitLabSshKey> GetForCurrentUserAsync(long keyId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create(CurrentUserRoot)
                .Literal(Keys)
                .Segment(keyId)
                .Build(),
            GitLabJsonContext.Default.GitLabSshKey,
            cancellationToken);
    }

    public Task<GitLabSshKey> CreateForCurrentUserAsync(CreateSshKeyRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create(CurrentUserRoot)
                .Literal(Keys)
                .Build(),
            request,
            GitLabJsonContext.Default.CreateSshKeyRequest,
            GitLabJsonContext.Default.GitLabSshKey,
            cancellationToken);
    }

    public Task DeleteForCurrentUserAsync(long keyId, CancellationToken cancellationToken = default)
    {
        // GitLab answers 200 with the deleted key rather than 204. DeleteAsync checks the status and
        // discards the body, which is right: the caller already had the key it just named.
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create(CurrentUserRoot)
                .Literal(Keys)
                .Segment(keyId)
                .Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabSshKey> ListForUserAsync(long userId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create(NamedUserRoot)
                .Segment(userId)
                .Literal(Keys)
                .Build(),
            GitLabJsonContext.Default.GitLabSshKeyArray,
            cancellationToken);
    }

    public Task<GitLabSshKey> GetForUserAsync(long userId, long keyId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create(NamedUserRoot)
                .Segment(userId)
                .Literal(Keys)
                .Segment(keyId)
                .Build(),
            GitLabJsonContext.Default.GitLabSshKey,
            cancellationToken);
    }

    public Task<GitLabSshKey> CreateForUserAsync(long userId, CreateSshKeyRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create(NamedUserRoot)
                .Segment(userId)
                .Literal(Keys)
                .Build(),
            request,
            GitLabJsonContext.Default.CreateSshKeyRequest,
            GitLabJsonContext.Default.GitLabSshKey,
            cancellationToken);
    }

    public Task DeleteForUserAsync(long userId, long keyId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create(NamedUserRoot)
                .Segment(userId)
                .Literal(Keys)
                .Segment(keyId)
                .Build(),
            cancellationToken);
    }

    public Task<GitLabUser> GetUserByFingerprintAsync(string fingerprint,
        CancellationToken cancellationToken = default)
    {
        // The fingerprint is a query parameter, not a path segment - which is what makes this endpoint
        // usable at all, since a SHA-256 fingerprint carries ":", "+" and "/". Query() percent-encodes it.
        return connection.GetAsync(
            GitLabRouteBuilder.Create(Keys)
                .Query("fingerprint", fingerprint)
                .Build(),
            GitLabJsonContext.Default.GitLabUser,
            cancellationToken);
    }

    public Task<GitLabSshKey> GetByIdAsync(long keyId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create(Keys)
                .Segment(keyId)
                .Build(),
            GitLabJsonContext.Default.GitLabSshKey,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabSshCertificate> ListGroupCertificatesAsync(GroupId groupId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create(Groups)
                .Segment(groupId)
                .Literal(SshCertificates)
                .Build(),
            GitLabJsonContext.Default.GitLabSshCertificateArray,
            cancellationToken);
    }

    public Task<GitLabSshCertificate> AddGroupCertificateAsync(GroupId groupId,
        CreateSshCertificateRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create(Groups)
                .Segment(groupId)
                .Literal(SshCertificates)
                .Build(),
            request,
            GitLabJsonContext.Default.CreateSshCertificateRequest,
            GitLabJsonContext.Default.GitLabSshCertificate,
            cancellationToken);
    }

    public Task DeleteGroupCertificateAsync(GroupId groupId, long certificateId,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create(Groups)
                .Segment(groupId)
                .Literal(SshCertificates)
                .Segment(certificateId)
                .Build(),
            cancellationToken);
    }
}