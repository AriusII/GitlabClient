using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Push rules" API area (<c>/projects/:id/push_rule</c> and
///     <c>/groups/:id/push_rule</c>) - server-side checks GitLab runs against every push before accepting
///     it (commit message shape, branch naming, secret detection, and so on), and the group-level
///     defaults applied to every push rule of a newly created project inside a group.
///     <para>
///         Each project and each group has at most one push rule resource, so <c>Create</c> and
///         <c>Update</c> are genuinely different verbs here: <c>Create</c> fails once a push rule already
///         exists, and there is no "add another rule" operation to model.
///     </para>
///     <para>
///         Managing group push rules requires the Owner role for the group, or administrator access to
///         the instance.
///     </para>
/// </summary>
public interface IPushRulesClient
{
    /// <summary>Gets a project's push rule. Throws <see cref="Exceptions.GitLabNotFoundException" /> if none is configured.</summary>
    Task<GitLabProjectPushRule> GetForProjectAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>Adds a push rule to a project. Use only when the project has no push rule configured yet.</summary>
    Task<GitLabProjectPushRule> CreateForProjectAsync(ProjectId projectId, CreatePushRuleRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Updates a project's existing push rule.</summary>
    Task<GitLabProjectPushRule> UpdateForProjectAsync(ProjectId projectId, UpdatePushRuleRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes a project's push rule.</summary>
    Task DeleteForProjectAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    /// <summary>Gets a group's push rule. Throws <see cref="Exceptions.GitLabNotFoundException" /> if none is configured.</summary>
    Task<GitLabGroupPushRule> GetForGroupAsync(GroupId groupId, CancellationToken cancellationToken = default);

    /// <summary>Adds a push rule to a group. Use only when the group has no push rule configured yet.</summary>
    Task<GitLabGroupPushRule> CreateForGroupAsync(GroupId groupId, CreatePushRuleRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Updates a group's existing push rule.</summary>
    Task<GitLabGroupPushRule> UpdateForGroupAsync(GroupId groupId, UpdatePushRuleRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes a group's push rule.</summary>
    Task DeleteForGroupAsync(GroupId groupId, CancellationToken cancellationToken = default);
}