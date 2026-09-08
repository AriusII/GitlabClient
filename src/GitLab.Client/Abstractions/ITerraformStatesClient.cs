using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab Terraform state API area (<c>/projects/:id/terraform/state</c> and
///     <c>/projects/:id/terraform/state_protection_rules</c>) - GitLab's implementation of Terraform's HTTP
///     remote-state backend, plus the rules that govern who may write each state.
///     <para>
///         The state document itself is opaque: it is whatever JSON Terraform wrote, stored verbatim, and
///         it is deliberately not modelled as a DTO. It comes back as a raw
///         <see cref="GitLabFileResponse" /> the caller must <c>await using</c>.
///     </para>
///     <para>
///         State names are free text and commonly contain <c>/</c> and <c>.</c>
///         (<c>env/production.tfstate</c>). Pass them raw - they are percent-encoded for you.
///     </para>
/// </summary>
public interface ITerraformStatesClient
{
    /// <summary>
    ///     Downloads one Terraform state by name.
    /// </summary>
    /// <param name="projectId">The project holding the state.</param>
    /// <param name="name">The state name, passed raw.</param>
    /// <param name="lockId">
    ///     The lock this read is being made under, when the caller holds one. Sent as Terraform's <c>ID</c>
    ///     parameter; omit it for an unlocked read.
    /// </param>
    /// <param name="cancellationToken">Cancels the request, and any read from the returned stream.</param>
    /// <returns>
    ///     The state document as an open stream. The caller owns it and must <c>await using</c> it - it
    ///     holds the HTTP response and its connection open until disposed.
    /// </returns>
    Task<GitLabFileResponse> DownloadAsync(ProjectId projectId, string name, string? lockId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Stores a Terraform state, creating it or appending a new version to it. This is the write half
    ///     of Terraform's HTTP backend, and the counterpart to
    ///     <see cref="DownloadAsync(ProjectId, string, string?, CancellationToken)" />.
    ///     <para>
    ///         GitLab takes the document as a <c>multipart/form-data</c> upload and answers with no body,
    ///         so nothing is returned; the new serial is whatever the uploaded document declares.
    ///     </para>
    /// </summary>
    /// <param name="projectId">The project to store the state in.</param>
    /// <param name="name">The state name, passed raw.</param>
    /// <param name="state">
    ///     The state document. Its stream is borrowed and read, never disposed here - the caller still owns
    ///     it after the call returns.
    /// </param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <exception cref="Exceptions.GitLabConflictException">Another run holds the lock on this state.</exception>
    Task UploadAsync(ProjectId projectId, string name, GitLabFileUpload state,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes a Terraform state and every version of it.</summary>
    Task DeleteAsync(ProjectId projectId, string name, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Asks GitLab Workhorse to authorize a state upload before it is sent, which is how a large state
    ///     is streamed straight to object storage instead of through Rails.
    ///     <para>
    ///         The reply is Workhorse's internal routing document, which the API declares no schema for and
    ///         only Workhorse itself consumes, so it is not surfaced here.
    ///     </para>
    /// </summary>
    Task AuthorizeUploadAsync(ProjectId projectId, string name, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Locks a Terraform state, so that no other run may write it until it is unlocked.
    ///     <para>
    ///         Terraform's own protocol uses the custom <c>LOCK</c> verb; GitLab exposes the same operation
    ///         as an ordinary <c>POST</c> carrying Terraform's lock-info document.
    ///     </para>
    /// </summary>
    /// <exception cref="Exceptions.GitLabConflictException">The state is already locked by someone else.</exception>
    Task LockAsync(ProjectId projectId, string name, LockTerraformStateRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Releases a Terraform state lock. Terraform's protocol calls this <c>UNLOCK</c>; GitLab exposes it
    ///     as a <c>DELETE</c>.
    /// </summary>
    /// <param name="projectId">The project holding the state.</param>
    /// <param name="name">The state name, passed raw.</param>
    /// <param name="lockId">
    ///     The id returned when the lock was taken. Omit it to force the lock open, which is what
    ///     <c>terraform force-unlock</c> does.
    /// </param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task UnlockAsync(ProjectId projectId, string name, string? lockId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads one historical version of a Terraform state by its serial number.
    /// </summary>
    /// <returns>
    ///     The state document as an open stream. The caller owns it and must <c>await using</c> it.
    /// </returns>
    Task<GitLabFileResponse> DownloadVersionAsync(ProjectId projectId, string name, long serial,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes one historical version of a Terraform state, leaving the state itself in place.</summary>
    Task DeleteVersionAsync(ProjectId projectId, string name, long serial,
        CancellationToken cancellationToken = default);

    /// <summary>Streams every Terraform state protection rule on the project.</summary>
    IAsyncEnumerable<GitLabTerraformStateProtectionRule> ListProtectionRulesAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a protection rule for one named state. State names are unique per rule, so a second rule
    ///     for the same name is rejected.
    /// </summary>
    Task<GitLabTerraformStateProtectionRule> CreateProtectionRuleAsync(ProjectId projectId,
        CreateTerraformStateProtectionRuleRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates a protection rule. Only the members set on the request are sent; anything left null keeps
    ///     its current value.
    /// </summary>
    Task<GitLabTerraformStateProtectionRule> UpdateProtectionRuleAsync(ProjectId projectId, long ruleId,
        UpdateTerraformStateProtectionRuleRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes a protection rule, leaving the state it protected writable under the project's defaults.</summary>
    Task DeleteProtectionRuleAsync(ProjectId projectId, long ruleId, CancellationToken cancellationToken = default);
}