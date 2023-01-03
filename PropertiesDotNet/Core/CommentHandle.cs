namespace PropertiesDotNet.Core
{
    /// <summary>
    /// Represents a comment handle, or prefix in a ".properties" document.
    /// </summary>
    public enum CommentHandle
    {
        /// <summary>
        /// Represents the "#" handle. This is the default value.
        /// </summary>
        Hash,

        /// <summary>
        /// Represents the "!" handle.
        /// </summary>
        Exclamation,
    }
}