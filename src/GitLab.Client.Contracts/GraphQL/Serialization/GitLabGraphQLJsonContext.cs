using System.Text.Json.Serialization;

using GitLab.Client.GraphQL.Protocol;
using GitLab.Client.GraphQL.WorkItems.Connections;
using GitLab.Client.GraphQL.WorkItems.Identifiers;
using GitLab.Client.GraphQL.WorkItems.Models;
using GitLab.Client.GraphQL.WorkItems.Mutations;
using GitLab.Client.GraphQL.WorkItems.Queries;
using GitLab.Client.GraphQL.WorkItems.Widgets;
using GitLab.Client.GraphQL.WorkItems.Widgets.Inputs;

namespace GitLab.Client.GraphQL.Serialization;

/// <summary>
///     Source-generated JSON metadata used exclusively by the GitLab GraphQL endpoint.
/// </summary>
/// <remarks>
///     GitLab REST payloads use snake_case and are intentionally handled by
///     <c>GitLab.Client.Serialization.GitLabJsonContext</c>. GraphQL uses camelCase instead, so sharing that
///     context would silently produce invalid request members such as <c>operation_name</c>. Keep every GraphQL
///     operation's concrete request, variables, data, and response-envelope type registered here; an open generic
///     <see cref="GitLabGraphQLResponse{TData}" /> registration cannot provide Native-AOT-safe metadata.
/// </remarks>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    RespectNullableAnnotations = true)]
[JsonSerializable(typeof(GitLabGraphQLRequest))]
[JsonSerializable(typeof(GitLabGraphQLRequest[]))]
[JsonSerializable(typeof(GitLabGraphQLError))]
[JsonSerializable(typeof(GitLabGraphQLErrorLocation))]

// Curated Work Items response envelopes. Every closure is explicit so GraphQL clients never fall back to reflection
// when preserving operation data together with top-level GraphQL errors.
[JsonSerializable(typeof(GitLabGraphQLResponse<GitLabWorkItemByIdQueryData>))]
[JsonSerializable(typeof(GitLabGraphQLResponse<GitLabWorkItemByLocatorQueryData>))]
[JsonSerializable(typeof(GitLabGraphQLResponse<GitLabWorkItemsQueryData>))]
[JsonSerializable(typeof(GitLabGraphQLResponse<GitLabWorkItemTypesQueryData>))]
[JsonSerializable(typeof(GitLabGraphQLResponse<GitLabWorkItemCreateMutationData>))]
[JsonSerializable(typeof(GitLabGraphQLResponse<GitLabWorkItemUpdateMutationData>))]
[JsonSerializable(typeof(GitLabGraphQLResponse<GitLabWorkItemDeleteMutationData>))]

// Work Items operation variables, data, and mutation payloads. The direct registrations provide named JsonTypeInfo
// properties for typed operation factories as well as for their enclosing response envelopes.
[JsonSerializable(typeof(GitLabWorkItemByIdQueryVariables))]
[JsonSerializable(typeof(GitLabWorkItemByIdQueryData))]
[JsonSerializable(typeof(GitLabWorkItemByLocatorQueryVariables))]
[JsonSerializable(typeof(GitLabWorkItemByLocatorQueryData))]
[JsonSerializable(typeof(GitLabWorkItemsQueryVariables))]
[JsonSerializable(typeof(GitLabWorkItemsQueryData))]
[JsonSerializable(typeof(GitLabWorkItemTypesQueryVariables))]
[JsonSerializable(typeof(GitLabWorkItemTypesQueryData))]
[JsonSerializable(typeof(GitLabWorkItemCreateMutationVariables))]
[JsonSerializable(typeof(GitLabWorkItemCreateMutationData))]
[JsonSerializable(typeof(GitLabWorkItemCreateInput))]
[JsonSerializable(typeof(GitLabWorkItemCreatePayload))]
[JsonSerializable(typeof(GitLabWorkItemUpdateMutationVariables))]
[JsonSerializable(typeof(GitLabWorkItemUpdateMutationData))]
[JsonSerializable(typeof(GitLabWorkItemUpdateInput))]
[JsonSerializable(typeof(GitLabWorkItemUpdatePayload))]
[JsonSerializable(typeof(GitLabWorkItemDeleteMutationVariables))]
[JsonSerializable(typeof(GitLabWorkItemDeleteMutationData))]
[JsonSerializable(typeof(GitLabWorkItemDeleteInput))]
[JsonSerializable(typeof(GitLabWorkItemDeletePayload))]

// Supporting graph shapes and mutation-widget inputs. These registrations keep the source-generated metadata
// directly addressable as curated selections evolve, without introducing an object/dynamic escape hatch.
[JsonSerializable(typeof(GitLabWorkItemNamespace))]
[JsonSerializable(typeof(GitLabWorkItem))]
[JsonSerializable(typeof(GitLabWorkItemConnection))]
[JsonSerializable(typeof(GitLabWorkItemTypeConnection))]
[JsonSerializable(typeof(GitLabWorkItemUserConnection))]
[JsonSerializable(typeof(GitLabWorkItemLabelConnection))]
[JsonSerializable(typeof(GitLabWorkItemReferenceConnection))]
[JsonSerializable(typeof(GitLabWorkItemPageInfo))]
[JsonSerializable(typeof(GitLabWorkItemType))]
[JsonSerializable(typeof(GitLabWorkItemUser))]
[JsonSerializable(typeof(GitLabWorkItemLabel))]
[JsonSerializable(typeof(GitLabWorkItemMilestone))]
[JsonSerializable(typeof(GitLabWorkItemIteration))]
[JsonSerializable(typeof(GitLabWorkItemReference))]
[JsonSerializable(typeof(GitLabWorkItemWidget))]
[JsonSerializable(typeof(GitLabWorkItemAssigneesWidget))]
[JsonSerializable(typeof(GitLabWorkItemColorWidget))]
[JsonSerializable(typeof(GitLabWorkItemDescriptionWidget))]
[JsonSerializable(typeof(GitLabWorkItemHealthStatusWidget))]
[JsonSerializable(typeof(GitLabWorkItemStartAndDueDateWidget))]
[JsonSerializable(typeof(GitLabWorkItemLabelsWidget))]
[JsonSerializable(typeof(GitLabWorkItemMilestoneWidget))]
[JsonSerializable(typeof(GitLabWorkItemIterationWidget))]
[JsonSerializable(typeof(GitLabWorkItemHierarchyWidget))]
[JsonSerializable(typeof(GitLabGraphQLGlobalId))]
[JsonSerializable(typeof(GitLabWorkItemAssigneesWidgetInput))]
[JsonSerializable(typeof(GitLabWorkItemColorWidgetInput))]
[JsonSerializable(typeof(GitLabWorkItemDescriptionWidgetInput))]
[JsonSerializable(typeof(GitLabWorkItemHealthStatusWidgetInput))]
[JsonSerializable(typeof(GitLabWorkItemStartAndDueDateWidgetInput))]
[JsonSerializable(typeof(GitLabWorkItemLabelsCreateWidgetInput))]
[JsonSerializable(typeof(GitLabWorkItemLabelsUpdateWidgetInput))]
[JsonSerializable(typeof(GitLabWorkItemMilestoneWidgetInput))]
[JsonSerializable(typeof(GitLabWorkItemIterationWidgetInput))]
[JsonSerializable(typeof(GitLabWorkItemHierarchyCreateWidgetInput))]
[JsonSerializable(typeof(GitLabWorkItemHierarchyUpdateWidgetInput))]
internal sealed partial class GitLabGraphQLJsonContext : JsonSerializerContext;