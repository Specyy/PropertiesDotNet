namespace PropertiesDotNet.ObjectModel
{
    /// <summary>
    /// Represents a type of properties node.
    /// </summary>
    public enum PropertiesNodeType
    {
        /// <summary>
        /// Represents a property key, or a <see cref="Core.Events.Key"/>.
        /// </summary>
        Key,

        /// <summary>
        /// Represents a property value assigner, or a <see cref="Core.Events.ValueAssigner"/>.
        /// </summary>
        Assigner,

        /// <summary>
        /// Represents a property value, or a <see cref="Core.Events.Value"/>.
        /// </summary>
        Value,

        /// <summary>
        /// Represents a property inside a document. This is equivalent to the <see cref="Core.Events.PropertyStart"/>
        /// and <see cref="Core.Events.PropertyEnd"/> encapsulation.
        /// </summary>
        Property
    }
}
