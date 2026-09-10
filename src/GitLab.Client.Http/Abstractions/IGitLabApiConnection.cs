using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace GitLab.Client.Abstractions;

/// <summary>
///     The low-level transport every resource client is built on. Exposed publicly as an escape hatch for
///     GitLab endpoints that don't yet have a typed resource client, using the caller's own
///     <see cref="JsonSerializerContext" />-generated metadata (never reflection-based serialization).
/// </summary>
public interface IGitLabApiConnection
{
    Task<TResponse> GetAsync<TResponse>(
        Uri requestUri,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken = default);

    /// <summary>Streams every item across all pages, following the response's <c>Link: rel="next"</c> header.</summary>
    IAsyncEnumerable<TItem> GetPagedAsync<TItem>(
        Uri requestUri,
        JsonTypeInfo<TItem[]> pageTypeInfo,
        CancellationToken cancellationToken = default);

    Task<TResponse> PostAsync<TRequest, TResponse>(
        Uri requestUri,
        TRequest request,
        JsonTypeInfo<TRequest> requestTypeInfo,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     For GitLab action endpoints that are triggered by an empty-bodied POST (cancel/retry/play/stop) and return the
    ///     updated resource.
    /// </summary>
    Task<TResponse> PostAsync<TResponse>(
        Uri requestUri,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     The no-content sibling of <see cref="PostAsync{TResponse}(Uri, JsonTypeInfo{TResponse}, CancellationToken)" />:
    ///     GitLab answers plenty of action endpoints
    ///     with <c>202 Accepted</c> or <c>204 No Content</c> and an empty body (marking to-dos done, forcing a
    ///     remote-mirror sync, retrying a status check), which the deserializing overload would report as an
    ///     empty response body.
    /// </summary>
    Task PostAsync(
        Uri requestUri,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     For GitLab bulk-action endpoints that take a JSON body and answer with no content, such as publishing
    ///     a merge request's draft notes.
    /// </summary>
    Task PostAsync<TRequest>(
        Uri requestUri,
        TRequest request,
        JsonTypeInfo<TRequest> requestTypeInfo,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Sends fields without a file as <c>multipart/form-data</c> and deserializes GitLab's JSON response.
    ///     This is the shape used by form-mode endpoints such as creating a user or changing the instance
    ///     appearance when no binary field is supplied.
    /// </summary>
    /// <typeparam name="TResponse">The type GitLab answers with.</typeparam>
    /// <param name="requestUri">The route to post to, relative to the configured GitLab instance.</param>
    /// <param name="formFields">
    ///     The already wire-named text fields. Nested values use GitLab's bracket notation, for example
    ///     <c>settings[enabled]</c>; no CLR object is reflected or serialized by this transport.
    /// </param>
    /// <param name="responseTypeInfo">Source-generated metadata for <typeparamref name="TResponse" />.</param>
    /// <param name="cancellationToken">Cancels the request and response-body read.</param>
    /// <returns>The deserialized response.</returns>
    /// <exception cref="Exceptions.GitLabApiException">GitLab answered with a non-success status code.</exception>
    Task<TResponse> PostMultipartFormAsync<TResponse>(
        Uri requestUri,
        IReadOnlyDictionary<string, string> formFields,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Sends fields without a file as <c>multipart/form-data</c> to an endpoint whose successful response
    ///     has no body.
    /// </summary>
    /// <param name="requestUri">The route to post to, relative to the configured GitLab instance.</param>
    /// <param name="formFields">
    ///     The already wire-named text fields. Nested values use GitLab's bracket notation, for example
    ///     <c>settings[enabled]</c>.
    /// </param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <exception cref="Exceptions.GitLabApiException">GitLab answered with a non-success status code.</exception>
    Task PostMultipartFormAsync(
        Uri requestUri,
        IReadOnlyDictionary<string, string> formFields,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Sends fields without a file as <c>multipart/form-data</c> with <c>PUT</c> and deserializes GitLab's
    ///     JSON response.
    /// </summary>
    /// <typeparam name="TResponse">The type GitLab answers with.</typeparam>
    /// <param name="requestUri">The route to put to, relative to the configured GitLab instance.</param>
    /// <param name="formFields">
    ///     The already wire-named text fields. Nested values use GitLab's bracket notation, for example
    ///     <c>settings[enabled]</c>; no CLR object is reflected or serialized by this transport.
    /// </param>
    /// <param name="responseTypeInfo">Source-generated metadata for <typeparamref name="TResponse" />.</param>
    /// <param name="cancellationToken">Cancels the request and response-body read.</param>
    /// <returns>The deserialized response.</returns>
    /// <exception cref="Exceptions.GitLabApiException">GitLab answered with a non-success status code.</exception>
    Task<TResponse> PutMultipartFormAsync<TResponse>(
        Uri requestUri,
        IReadOnlyDictionary<string, string> formFields,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Sends fields without a file as <c>multipart/form-data</c> with <c>PUT</c> to an endpoint whose
    ///     successful response has no body.
    /// </summary>
    /// <param name="requestUri">The route to put to, relative to the configured GitLab instance.</param>
    /// <param name="formFields">
    ///     The already wire-named text fields. Nested values use GitLab's bracket notation, for example
    ///     <c>settings[enabled]</c>.
    /// </param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <exception cref="Exceptions.GitLabApiException">GitLab answered with a non-success status code.</exception>
    Task PutMultipartFormAsync(
        Uri requestUri,
        IReadOnlyDictionary<string, string> formFields,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Sends an <c>application/x-www-form-urlencoded</c> request and deserializes GitLab's JSON response.
    ///     GitLab's direct bulk-import endpoint uses this representation rather than JSON or multipart.
    /// </summary>
    /// <typeparam name="TResponse">The type GitLab answers with.</typeparam>
    /// <param name="requestUri">The route to post to, relative to the configured GitLab instance.</param>
    /// <param name="formFields">
    ///     The already wire-named text fields. Use indexed bracket keys such as
    ///     <c>entities[0][source_full_path]</c> for nested bulk-import values.
    /// </param>
    /// <param name="responseTypeInfo">Source-generated metadata for <typeparamref name="TResponse" />.</param>
    /// <param name="cancellationToken">Cancels the request and response-body read.</param>
    /// <returns>The deserialized response.</returns>
    /// <exception cref="Exceptions.GitLabApiException">GitLab answered with a non-success status code.</exception>
    Task<TResponse> PostUrlEncodedFormAsync<TResponse>(
        Uri requestUri,
        IReadOnlyDictionary<string, string> formFields,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken = default);

    Task<TResponse> PutAsync<TRequest, TResponse>(
        Uri requestUri,
        TRequest request,
        JsonTypeInfo<TRequest> requestTypeInfo,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     A body-less PUT that GitLab answers with no content - resetting a merge request's approvals is the
    ///     shape this exists for. The generic overload would both send a payload the endpoint does not declare
    ///     and then fail trying to deserialize the empty response.
    /// </summary>
    Task PutAsync(
        Uri requestUri,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     A PUT that carries a body and that GitLab answers with no content. Setting a webhook's URL
    ///     variable or custom header is the shape this exists for: the endpoint declares a request body but
    ///     returns <c>204</c>, so the generic overload would fail trying to deserialize an empty response.
    /// </summary>
    Task PutAsync<TRequest>(
        Uri requestUri,
        TRequest request,
        JsonTypeInfo<TRequest> requestTypeInfo,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     A body-less PUT that returns the updated resource - promoting a project label to a group label,
    ///     or unprotecting a branch. The <c>TRequest</c> overload would send a payload the endpoint does not
    ///     declare; <see cref="PutAsync(Uri, CancellationToken)" /> would discard the resource.
    /// </summary>
    Task<TResponse> PutAsync<TResponse>(
        Uri requestUri,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken = default);

    /// <summary>For GitLab delete endpoints, which respond <c>204 No Content</c> on success.</summary>
    Task DeleteAsync(
        Uri requestUri,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     A DELETE that answers <c>200</c> with the deleted resource rather than <c>204</c>. Removing an
    ///     issue link, a release asset link or a metric image returns the entity that was removed, and
    ///     <c>DELETE /projects/:id/environments/review_apps</c> returns the dry-run preview - which is the
    ///     whole point of that call, so discarding the body there loses the answer.
    /// </summary>
    Task<TResponse> DeleteAsync<TResponse>(
        Uri requestUri,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     A GET whose success response carries no body - <c>GET /projects/:id/pages_access</c> is the one
    ///     endpoint shaped this way, using the status code alone as the answer.
    /// </summary>
    Task GetAsync(
        Uri requestUri,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads a body that is bytes rather than JSON - a repository archive, a raw file blob, a merge
    ///     request's raw diffs, a job artifact or a job trace log. The body is streamed and never reaches the
    ///     serializer.
    /// </summary>
    /// <param name="requestUri">The route to download, relative to the configured GitLab instance.</param>
    /// <param name="cancellationToken">Cancels the request, and any read from the returned stream.</param>
    /// <returns>
    ///     The open body plus its media type, length and <c>Content-Disposition</c> file name. The caller owns
    ///     it and must dispose it - it holds the HTTP response and the connection open until then.
    /// </returns>
    /// <exception cref="Exceptions.GitLabApiException">GitLab answered with a non-success status code.</exception>
    Task<GitLabFileResponse> GetFileAsync(
        Uri requestUri,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Uploads one file as <c>multipart/form-data</c> and deserializes GitLab's JSON answer - project and
    ///     group uploads, wiki attachments, issue metric images, secure files, avatars, project import.
    /// </summary>
    /// <typeparam name="TResponse">The type GitLab answers with.</typeparam>
    /// <param name="requestUri">The route to post to, relative to the configured GitLab instance.</param>
    /// <param name="file">The file part. Its stream is read but not disposed.</param>
    /// <param name="formFields">
    ///     Simple string form fields to send alongside the file, or null when the endpoint takes none.
    /// </param>
    /// <param name="responseTypeInfo">Source-generated metadata for <typeparamref name="TResponse" />.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The deserialized response.</returns>
    /// <exception cref="Exceptions.GitLabApiException">GitLab answered with a non-success status code.</exception>
    Task<TResponse> PostFileAsync<TResponse>(
        Uri requestUri,
        GitLabFileUpload file,
        IReadOnlyDictionary<string, string>? formFields,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     A multipart upload that GitLab answers with no content - storing a Terraform state, or a group's
    ///     placeholder reassignments. The generic overload would fail trying to deserialize the empty body.
    /// </summary>
    /// <param name="requestUri">The route to upload to, relative to the configured GitLab instance.</param>
    /// <param name="file">The file part. Its stream is borrowed, never disposed here.</param>
    /// <param name="formFields">Simple string fields to send alongside the file, or <see langword="null" />.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <exception cref="Exceptions.GitLabApiException">GitLab answered with a non-success status code.</exception>
    Task PostFileAsync(
        Uri requestUri,
        GitLabFileUpload file,
        IReadOnlyDictionary<string, string>? formFields,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     The <c>PUT</c> sibling of
    ///     <see
    ///         cref="PostFileAsync{TResponse}(Uri, GitLabFileUpload, IReadOnlyDictionary{string, string}, JsonTypeInfo{TResponse}, CancellationToken)" />
    ///     , for the upload endpoints GitLab models as a replacement rather than a creation.
    /// </summary>
    /// <typeparam name="TResponse">The type GitLab answers with.</typeparam>
    /// <param name="requestUri">The route to put to, relative to the configured GitLab instance.</param>
    /// <param name="file">The file part. Its stream is read but not disposed.</param>
    /// <param name="formFields">
    ///     Simple string form fields to send alongside the file, or null when the endpoint takes none.
    /// </param>
    /// <param name="responseTypeInfo">Source-generated metadata for <typeparamref name="TResponse" />.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The deserialized response.</returns>
    /// <exception cref="Exceptions.GitLabApiException">GitLab answered with a non-success status code.</exception>
    Task<TResponse> PutFileAsync<TResponse>(
        Uri requestUri,
        GitLabFileUpload file,
        IReadOnlyDictionary<string, string>? formFields,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     The <c>PUT</c> sibling of
    ///     <see cref="PostFileAsync(Uri, GitLabFileUpload, IReadOnlyDictionary{string, string}, CancellationToken)" />,
    ///     for the multipart upload endpoints GitLab answers with no content at all - publishing a package
    ///     revision file (Conan, Debian, generic Maven/npm/PyPI package files) or a Terraform module archive,
    ///     where GitLab's Workhorse layer returns a bare success status with an empty body. The generic
    ///     <see cref="PutFileAsync{TResponse}" /> overload would fail trying to deserialize that empty body.
    /// </summary>
    /// <param name="requestUri">The route to upload to, relative to the configured GitLab instance.</param>
    /// <param name="file">The file part. Its stream is borrowed, never disposed here.</param>
    /// <param name="formFields">Simple string fields to send alongside the file, or <see langword="null" />.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <exception cref="Exceptions.GitLabApiException">GitLab answered with a non-success status code.</exception>
    Task PutFileAsync(
        Uri requestUri,
        GitLabFileUpload file,
        IReadOnlyDictionary<string, string>? formFields,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Issues a <c>GET</c> whose successful answer is a redirect rather than a body or a status code -
    ///     the PyPI package-proxy forwarding route is the first of these. A 3xx status is this method's
    ///     success case and is returned, not thrown; every other non-success status still throws the typed
    ///     exception exactly as every other verb does. See <see cref="GitLabRedirectResponse" /> for why
    ///     this needs its own method rather than following the redirect automatically.
    /// </summary>
    /// <param name="requestUri">The route to request, relative to the configured GitLab instance.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The status GitLab answered with and, when present, the parsed <c>Location</c> header.</returns>
    /// <exception cref="Exceptions.GitLabApiException">
    ///     GitLab answered with a non-success, non-redirect status code.
    /// </exception>
    Task<GitLabRedirectResponse> GetRedirectAsync(
        Uri requestUri,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Issues a <c>HEAD</c> request as an existence check. GitLab exposes these for repository branches
    ///     and files, answering with no body and putting the metadata in <c>X-Gitlab-*</c> headers.
    /// </summary>
    /// <param name="requestUri">The route to probe, relative to the configured GitLab instance.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>
    ///     The probe result. A <c>404</c> comes back as <see cref="GitLabHeadResponse.Exists" /> being false
    ///     rather than as an exception; every other failure still throws the typed exception it would on any
    ///     other verb.
    /// </returns>
    /// <exception cref="Exceptions.GitLabApiException">
    ///     GitLab answered with a non-success status code other than <c>404 Not Found</c>.
    /// </exception>
    Task<GitLabHeadResponse> HeadAsync(
        Uri requestUri,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     A <c>PATCH</c> with a JSON body that answers with the updated resource. GitLab reserves the verb
    ///     for genuinely partial updates - <c>PATCH /user/status</c>, <c>PATCH /admin/zoekt/namespaces/{id}</c>.
    /// </summary>
    /// <typeparam name="TRequest">The request body type.</typeparam>
    /// <typeparam name="TResponse">The type GitLab answers with.</typeparam>
    /// <param name="requestUri">The route to patch, relative to the configured GitLab instance.</param>
    /// <param name="request">The request body.</param>
    /// <param name="requestTypeInfo">Source-generated metadata for <typeparamref name="TRequest" />.</param>
    /// <param name="responseTypeInfo">Source-generated metadata for <typeparamref name="TResponse" />.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The deserialized response.</returns>
    /// <exception cref="Exceptions.GitLabApiException">GitLab answered with a non-success status code.</exception>
    Task<TResponse> PatchAsync<TRequest, TResponse>(
        Uri requestUri,
        TRequest request,
        JsonTypeInfo<TRequest> requestTypeInfo,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken = default);

    /// <summary>A <c>PATCH</c> with a JSON body that GitLab answers with no content.</summary>
    /// <typeparam name="TRequest">The request body type.</typeparam>
    /// <param name="requestUri">The route to patch, relative to the configured GitLab instance.</param>
    /// <param name="request">The request body.</param>
    /// <param name="requestTypeInfo">Source-generated metadata for <typeparamref name="TRequest" />.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>A task that completes once GitLab has accepted the request.</returns>
    /// <exception cref="Exceptions.GitLabApiException">GitLab answered with a non-success status code.</exception>
    Task PatchAsync<TRequest>(
        Uri requestUri,
        TRequest request,
        JsonTypeInfo<TRequest> requestTypeInfo,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     A body-less <c>PATCH</c> that GitLab answers with no content, which is the shape of
    ///     <c>PATCH /users/{id}/disable_two_factor</c>.
    /// </summary>
    /// <param name="requestUri">The route to patch, relative to the configured GitLab instance.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>A task that completes once GitLab has accepted the request.</returns>
    /// <exception cref="Exceptions.GitLabApiException">GitLab answered with a non-success status code.</exception>
    Task PatchAsync(
        Uri requestUri,
        CancellationToken cancellationToken = default);
}