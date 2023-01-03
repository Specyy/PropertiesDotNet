using PropertiesDotNet.Core.Events;

namespace PropertiesDotNet.Core
{
    /// <summary>
    /// Provides extensions methods for a <see cref="IPropertiesWriter"/>.
    /// </summary>
    public static class PropertiesWriterExtensions
    {
        /// <summary>
        /// Writes a property into the given <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The properties writer to write into.</param>
        /// <param name="key">The property key, as text.</param>
        /// <param name="assigner">The property assigner</param>
        /// <param name="value">The property value, as text.</param>
        public static void WriteProperty(this IPropertiesWriter writer, Text key, ValueAssigner assigner, Text value)
        {
            StartProperty(writer);
            writer.Write(key);
            writer.Write(assigner);
            writer.Write(value);
            EndProperty(writer);
        }

        /// <summary>
        /// Writes a property into the given <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The properties writer to write into.</param>
        /// <param name="key">The property key, as a string.</param>
        /// <param name="assigner">The property assigner</param>
        /// <param name="value">The property value, as a string.</param>
        public static void WriteProperty(this IPropertiesWriter writer, string key, ValueAssigner assigner, string? value)
        {
            WriteProperty(writer, new Key(key), assigner, new Value(value));
        }

        /// <summary>
        /// Writes a property into the given <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The properties writer to write into.</param>
        /// <param name="key">The property key, as text.</param>
        /// <param name="value">The property value, as text.</param>
        public static void WriteProperty(this IPropertiesWriter writer, Text key, Text value)
        {
            WriteProperty(writer, key, ValueAssignerType.Equals, value);
        }

        /// <summary>
        /// Writes a property into the given <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The properties writer to write into.</param>
        /// <param name="key">The property key, as a string.</param>
        /// <param name="value">The property value, as a string.</param>
        public static void WriteProperty(this IPropertiesWriter writer, string key, string? value)
        {
            WriteProperty(writer, new Key(key), new Value(value));
        }

        /// <summary>
        /// Writes a comment into the <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The properties writer.</param>
        /// <param name="value">The comment value.</param>
        public static void WriteComment(this IPropertiesWriter writer, string? value)
        {
            writer.Write(new Comment(value));
        }

        /// <summary>
        /// Writes a text value into the <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The properties writer.</param>
        /// <param name="value">The text value.</param>
        public static void WriteText(this IPropertiesWriter writer, string value)
        {
            writer.Write(new Text(value));
        }

        /// <summary>
        /// Writes an assigner into the <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The properties writer.</param>
        /// <param name="value">The text value.</param>
        public static void WriteAssigner(this IPropertiesWriter writer, char value)
        {
            writer.Write(new ValueAssigner(value));
        }

        /// <summary>
        /// Starts a new property scope in the given <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The properties writer.</param>
        public static void StartProperty(this IPropertiesWriter writer)
        {
            writer.Write(new PropertyStart());
        }

        /// <summary>
        /// Ends the current property scope for the given <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The properties writer.</param>
        public static void EndProperty(this IPropertiesWriter writer)
        {
            writer.Write(new PropertyEnd());
        }

        /// <summary>
        /// Starts the properties stream for the given <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The properties writer.</param>
        public static void StartDocument(this IPropertiesWriter writer)
        {
            writer.Write(new DocumentStart());
        }

        /// <summary>
        /// Ends the properties stream for the given <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The properties writer.</param>
        public static void EndDocument(this IPropertiesWriter writer)
        {
            writer.Write(new DocumentEnd());
        }
    }
}
