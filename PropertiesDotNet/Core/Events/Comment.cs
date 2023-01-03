using System;

namespace PropertiesDotNet.Core.Events
{
    /// <summary>
    /// Represents a comment in a ".properties" document.
    /// </summary>
    public sealed class Comment : PropertiesEvent, IEquatable<Comment>, IEquatable<string>
    {
        /// <inheritdoc/>
        public override bool Canonical => true;

        /// <inheritdoc/>
        public override int DepthIncrease => 0;

        /// <summary>
        ///	The handle for this comment.
        /// </summary>
        public CommentHandle Handle { get; }

        /// <summary>
        ///	The handle for this comment, as a <see cref="char"/>.
        /// </summary>
        public char HandleCharacter { get; }

        /// <summary>
        ///	The content of this comment.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Writes a <see cref="Comment"/>.
        /// </summary>
        /// <param name="value">The comment's value.</param>
        public Comment(string? value) : this(CommentHandle.Hash, value)
        {

        }

        /// <summary>
        /// Creates a new <see cref="Comment"/>.
        /// </summary>
        /// <param name="start">The starting position of this event in the text stream. Null if this
        /// event was created for writing.</param>
        /// <param name="end">The ending position of this event in the text stream. Null if this
        /// event was created for writing.</param>
        /// <param name="value">The comment's value.</param>
        public Comment(StreamMark? start, StreamMark? end, string? value) : this(start, end, CommentHandle.Hash, value)
        {

        }

        /// <summary>
        /// Writes a <see cref="Comment"/>.
        /// </summary>
        /// <param name="handle">The handle for this comment.</param>
        /// <param name="value">The comment's value.</param>
        public Comment(CommentHandle handle, string? value) : this(null, null, handle, value)
        {

        }

        /// <summary>
        /// Creates a new <see cref="Comment"/>.
        /// </summary>
        /// <param name="start">The starting position of this event in the text stream. Null if this
        /// event was created for writing.</param>
        /// <param name="end">The ending position of this event in the text stream. Null if this
        /// event was created for writing.</param>
        /// <param name="handle">The handle for this comment.</param>
        /// <param name="value">The comment's value.</param>
        public Comment(StreamMark? start, StreamMark? end, CommentHandle handle, string? value) : base(start, end)
        {
            Handle = handle;
            Value = value ?? string.Empty;

            // Check for invalid comment (multi-line)
            if (Value.Contains("\r") || Value.Contains("\n"))
                throw new ArgumentException($"Multi-line comments are not supported in a \".properites\" document!");

            HandleCharacter = handle == CommentHandle.Exclamation ? '!' : '#';
        }

        internal Comment(StreamMark? start, StreamMark? end, CommentHandle handle, char handleChar, string? value) : base(start, end)
        {
            Handle = handle;
            Value = value ?? string.Empty;
            HandleCharacter = handleChar;
        }

        /// <summary>
		/// Accepts an <see cref="IEventVisitor"/>.
		/// </summary>
		/// <param name="visitor">The visitor to accept.</param>
		public override void AcceptVisitor(IEventVisitor visitor) => visitor.Visit(this);

        /// <inheritdoc/>
        public override bool Equals(PropertiesEvent? other) => other is Comment comment && Equals(comment);

        /// <inheritdoc/>
        public bool Equals(Comment? other) => Value.Equals(other?.Value);

        /// <inheritdoc/>
        public bool Equals(string? other) => Value.Equals(other);

        /// <summary>
        /// Returns the <see cref="Value"/> of this <see cref="Comment"/>.
        /// </summary>
        /// <returns>The <see cref="Value"/> of this <see cref="Comment"/>.</returns>
        public override string ToString() => Value;
    }
}
