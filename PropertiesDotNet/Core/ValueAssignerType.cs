namespace PropertiesDotNet.Core
{
    /// <summary>
    /// Represents a type of value assigner in a ".properties" document.
    /// </summary>
    public enum ValueAssignerType : byte
    {
        /// <summary>
        /// Represents a value assigner that is the '=' character.
        /// </summary>
        Equals,

        /// <summary>
        /// Represents a value assigner that is the ':' character.
        /// </summary>
        Colon,

        /// <summary>
        /// Represents a value assigner that is a white-space character.
        /// </summary>
        Whitespace,
    }
}