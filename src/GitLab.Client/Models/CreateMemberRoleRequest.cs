namespace GitLab.Client.Models;

/// <summary>
///     Body of <c>POST /member_roles</c> and <c>POST /groups/:id/member_roles</c> - the custom role to create,
///     as a base role plus the abilities layered on top of it.
///     <para>
///         Every ability is nullable so an ability the caller does not mention is omitted from the request
///         entirely rather than sent as <c>false</c>. GitLab defaults each one to <c>false</c> server-side.
///     </para>
/// </summary>
public sealed record CreateMemberRoleRequest
{
    /// <summary>
    ///     GitLab's numeric role ladder to build the custom role on - 10 (Guest), 15 (Planner), 20 (Reporter), 25, 30
    ///     (Developer), 40 (Maintainer) or 50 (Owner). It is an integer on the wire. GitLab requires it when creating
    ///     a member role.
    /// </summary>
    public int? BaseAccessLevel { get; init; }

    /// <summary>Name for role (default: 'Custom')</summary>
    public string? Name { get; init; }

    /// <summary>Description for role</summary>
    public string? Description { get; init; }

    /// <summary>Apply security scan profiles.</summary>
    public bool? ApplySecurityScanProfiles { get; init; }

    /// <summary>Allows approval of merge requests.</summary>
    public bool? AdminMergeRequest { get; init; }

    /// <summary>Allows archiving of projects.</summary>
    public bool? ArchiveProject { get; init; }

    /// <summary>Enable, disable, and configure custom agents and flows from the AI catalog for a project.</summary>
    public bool? AdminAiCatalogItemConsumer { get; init; }

    /// <summary>Create security scan profiles.</summary>
    public bool? CreateSecurityScanProfiles { get; init; }

    /// <summary>Delete packages and package files in the package registry.</summary>
    public bool? DestroyPackage { get; init; }

    /// <summary>Allows deletion of projects.</summary>
    public bool? RemoveProject { get; init; }

    /// <summary>Delete security scan profiles.</summary>
    public bool? DeleteSecurityScanProfiles { get; init; }

    /// <summary>
    ///     Ability to delete or restore a subgroup. This ability does not allow deleting top-level groups. Review the
    ///     retention period settings to prevent accidental deletion.
    /// </summary>
    public bool? RemoveGroup { get; init; }

    /// <summary>Allows linking security policy projects.</summary>
    public bool? ManageSecurityPolicyLink { get; init; }

    /// <summary>Create, edit, and delete custom agents and flows in the AI catalog.</summary>
    public bool? AdminAiCatalogItem { get; init; }

    /// <summary>
    ///     Create, read, update, and delete compliance frameworks. Users with this permission can also assign a
    ///     compliance framework label to a project, and set the default framework of a group.
    /// </summary>
    public bool? AdminComplianceFramework { get; init; }

    /// <summary>Create, read, update, and delete CI/CD variables.</summary>
    public bool? AdminCicdVariables { get; init; }

    /// <summary>Manage deploy tokens at the group or project level.</summary>
    public bool? ManageDeployTokens { get; init; }

    /// <summary>
    ///     Create, read, update, and delete group access tokens. When creating a token, users with this custom
    ///     permission must select a role for that token that has the same or fewer permissions as the default role
    ///     used as the base for the custom role.
    /// </summary>
    public bool? ManageGroupAccessTokens { get; init; }

    /// <summary>
    ///     Add or remove users in a group, and assign roles to users. When assigning a role, users with this custom
    ///     permission must select a role that has the same or fewer permissions as the default role used as the base
    ///     for their custom role.
    /// </summary>
    public bool? AdminGroupMember { get; init; }

    /// <summary>Create, read, update, and delete integrations with external applications.</summary>
    public bool? AdminIntegrations { get; init; }

    /// <summary>
    ///     Configure merge request settings at the group or project level. Group actions include managing merge
    ///     checks and approval settings. Project actions include managing MR configurations, approval rules and
    ///     settings, and branch targets. In order to enable Suggested reviewers, the
    ///     <c>Manage project access tokens</c> custom permission needs to be enabled.
    /// </summary>
    public bool? ManageMergeRequestSettings { get; init; }

    /// <summary>
    ///     Create, read, update, and delete project access tokens. When creating a token, users with this custom
    ///     permission must select a role for that token that has the same or fewer permissions as the default role
    ///     used as the base for the custom role.
    /// </summary>
    public bool? ManageProjectAccessTokens { get; init; }

    /// <summary>Create, read, update, and delete protected branches for a project.</summary>
    public bool? AdminProtectedBranch { get; init; }

    /// <summary>Create, read, update, and delete protected environments</summary>
    public bool? AdminProtectedEnvironments { get; init; }

    /// <summary>Configure push rules for repositories at the group or project level.</summary>
    public bool? AdminPushRules { get; init; }

    /// <summary>
    ///     Create, view, edit, and delete group or project Runners. Includes configuring Runner settings.
    /// </summary>
    public bool? AdminRunners { get; init; }

    /// <summary>
    ///     Manage the security categories and attributes belonging to a top-level group. Also requires the
    ///     <c>read_security_attribute</c> permission.
    /// </summary>
    public bool? AdminSecurityAttributes { get; init; }

    /// <summary>Execute terraform commands, lock/unlock terraform state files, and remove file versions.</summary>
    public bool? AdminTerraformState { get; init; }

    /// <summary>
    ///     Edit the status, linked issue, and severity of a vulnerability object. Also requires the
    ///     <c>read_vulnerability</c> permission.
    /// </summary>
    public bool? AdminVulnerability { get; init; }

    /// <summary>Manage webhooks</summary>
    public bool? AdminWebHook { get; init; }

    /// <summary>
    ///     Read GitLab Duo Agent Platform artifacts, including audit events and session metadata, that are exposed
    ///     through the agent artifacts dashboard.
    /// </summary>
    public bool? ReadAgentArtifacts { get; init; }

    /// <summary>
    ///     Read compliance capabilities including adherence, violations, and frameworks for groups and projects.
    /// </summary>
    public bool? ReadComplianceDashboard { get; init; }

    /// <summary>Read security scan profiles.</summary>
    public bool? ReadSecurityScanProfiles { get; init; }

    /// <summary>
    ///     Allows read access to virtual registries at the group level. Enables users to resolve packages through the
    ///     virtual registry without requiring broader group membership permissions. Only works on top level groups.
    /// </summary>
    public bool? ReadVirtualRegistry { get; init; }

    /// <summary>
    ///     Update security AI workflow settings such as SAST Vulnerability Resolution. Also requires the
    ///     <c>read_vulnerability</c> permission.
    /// </summary>
    public bool? UpdateSecAiWorkflowSettings { get; init; }

    /// <summary>Update security scan profiles.</summary>
    public bool? UpdateSecurityScanProfiles { get; init; }

    /// <summary>Read CI/CD details for runners and jobs in the Admin Area.</summary>
    public bool? ReadAdminCicd { get; init; }

    /// <summary>Read CRM contact.</summary>
    public bool? ReadCrmContact { get; init; }

    /// <summary>Allows read-only access to the dependencies and licenses.</summary>
    public bool? ReadDependency { get; init; }

    /// <summary>Read group details in the Admin Area.</summary>
    public bool? ReadAdminGroups { get; init; }

    /// <summary>Read project details in the Admin Area.</summary>
    public bool? ReadAdminProjects { get; init; }

    /// <summary>
    ///     Allows read-only access to the source code in the user interface. Does not allow users to edit or download
    ///     repository archives, clone or pull repositories, view source code in an IDE, or view merge requests for
    ///     private projects. You can download individual files because read-only access inherently grants the ability
    ///     to make a local copy of the file.
    /// </summary>
    public bool? ReadCode { get; init; }

    /// <summary>Allows read-only access to group or project runners, including the runner fleet dashboard.</summary>
    public bool? ReadRunners { get; init; }

    /// <summary>
    ///     Allows read-only access to the security categories and attributes that belong to a top-level group.
    /// </summary>
    public bool? ReadSecurityAttribute { get; init; }

    /// <summary>Read subscription details in the Admin area.</summary>
    public bool? ReadAdminSubscription { get; init; }

    /// <summary>
    ///     Read system information such as background migrations, health checks, and Gitaly in the Admin Area.
    /// </summary>
    public bool? ReadAdminMonitoring { get; init; }

    /// <summary>Read the user list and user details in the Admin area.</summary>
    public bool? ReadAdminUsers { get; init; }

    /// <summary>Read vulnerability reports and security dashboards.</summary>
    public bool? ReadVulnerability { get; init; }
}