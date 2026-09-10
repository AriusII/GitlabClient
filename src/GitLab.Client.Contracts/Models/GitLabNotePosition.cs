using GitLab.Client.Models.Requests;

namespace GitLab.Client.Models;

/// <summary>
///     Anchors a discussion to a line in a diff - the difference between a plain merge request comment and
///     a review comment attached to code. Sent as the <c>position</c> member of
///     <see cref="CreateDiscussionRequest" /> on the merge request and commit routes, and returned on
///     <see cref="GitLabNote.Position" /> for the corresponding review notes. GitLab rejects it everywhere else.
///     <para>
///         The three SHAs identify the diff the line numbers are relative to:
///         <see cref="BaseSha" /> is the merge base, <see cref="StartSha" /> the commit in the target
///         branch, and <see cref="HeadSha" /> the head of the source branch. They are required when creating
///         an anchored note, but GitLab can return any of them as null in a response while the diff is
///         unavailable. Which of the remaining members apply depends on the position type - the line and
///         path members for <see cref="GitLabNotePositionType.Text" />, the pixel members for
///         <see cref="GitLabNotePositionType.Image" />.
///     </para>
///     <para>
///         The OpenAPI document types this member as a bare <c>type: object</c> on the Discussions routes;
///         the shape below is the fully declared one GitLab publishes for the identical <c>position</c>
///         parameter of the draft notes routes, cross-checked against the example on its draft note entity.
///     </para>
/// </summary>
public sealed record GitLabNotePosition
{
    /// <summary>SHA of the merge base commit the diff is computed from.</summary>
    public string? BaseSha { get; init; }

    /// <summary>SHA of the commit in the target branch the diff starts at.</summary>
    public string? StartSha { get; init; }

    /// <summary>SHA of the head commit of the source branch.</summary>
    public string? HeadSha { get; init; }

    /// <summary>Whether the position refers to a diff line, an image, or a whole file.</summary>
    public GitLabNotePositionType? PositionType { get; init; }

    /// <summary>File path after the change. Required for a text position on an added or modified file.</summary>
    public string? NewPath { get; init; }

    /// <summary>Line number after the change; absent when commenting on a removed line.</summary>
    public int? NewLine { get; init; }

    /// <summary>File path before the change. Required for a text position on a removed or renamed file.</summary>
    public string? OldPath { get; init; }

    /// <summary>Line number before the change; absent when commenting on an added line.</summary>
    public int? OldLine { get; init; }

    /// <summary>Width of the image, for an image position.</summary>
    public int? Width { get; init; }

    /// <summary>Height of the image, for an image position.</summary>
    public int? Height { get; init; }

    /// <summary>X coordinate of the comment marker on the image, for an image position.</summary>
    public double? X { get; init; }

    /// <summary>Y coordinate of the comment marker on the image, for an image position.</summary>
    public double? Y { get; init; }

    /// <summary>The span covered by a multiline comment. Leave unset for a single-line comment.</summary>
    public GitLabNoteLineRange? LineRange { get; init; }
}