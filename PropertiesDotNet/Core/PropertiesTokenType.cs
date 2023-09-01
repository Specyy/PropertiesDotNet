namespace PropertiesDotNet.Core
{
    /// <summary>
    /// Represents the types of tokens in a .properties docuemnt.
    /// </summary>
    public enum PropertiesTokenType : byte
    {
        /// <summary>
        /// A <see langword="null"/> value. Returned when an <see cref="IPropertiesReader"/> has not yet
        /// read a token (stream start).
        /// </summary>
        None = 0,

        /// <summary>
        /// Represents an error in a ".properties" document. This token is not canonical.
        /// </summary>
        Error,

        /// <summary>
        /// Represents a comment in a ".properties" document. In a standard ".properties"
        /// document comment indicators begin with either a "!" or "#".
        /// </summary>
        Comment,

        /// <summary>
        /// Represents a key in a ".properties" document.
        /// </summary>
        Key,

        /// <summary>
        /// Represents a value assigner in a ".properties" document. This can either be a 
        /// ":" (colon), "=" (equals), or any type of whitespace.
        /// </summary>
        Assigner,

        /// <summary>
        /// Represents a value in a ".properties" document.
        /// </summary>
        Value
    }
}
