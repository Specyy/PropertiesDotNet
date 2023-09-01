using System;

using PropertiesDotNet.Core;

namespace PropertiesDotNet.Serialization
{
    /// <summary>
    /// Provides custom ".properties" object serialization instructions for the <see cref="Converters.ObjectConverter"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class PropertiesMemberAttribute : Attribute
    {
        /// <summary>
        /// Specifies that this property should be serialized as the given type, rather than using the actual runtime value's type.
        /// </summary>
        public Type? SerializeAs { get; set; }

        /// <summary>
        /// Instructs the <see cref="PropertiesSerializer"/> to use a different member name for (de)serialization.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Whether this memeber should be serialized.
        /// </summary>
        public bool Serialize { get; set; } = true;

        /// <summary>
        /// Provides custom ".properties" object serialization instructions for the <see cref="Converters.ObjectConverter"/>.
        /// </summary>
        /// <param name="serialize">Whether this property should be (de)serialized.</param>
        public PropertiesMemberAttribute(bool serialize)
        {
            Serialize = serialize;
        }

        /// <summary>
        /// Provides custom ".properties" object serialization instructions for the <see cref="Converters.ObjectConverter"/>.
        /// </summary>
        /// <param name="name">The name that should be used for the serialization and deserialization of this member.</param>
        public PropertiesMemberAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Provides custom ".properties" object serialization instructions for the <see cref="Converters.ObjectConverter"/>.
        /// </summary>
        /// <param name="serializeAs">Specifies that this property should be serialized as the given type, rather than using the actual runtime value's type.</param>
        public PropertiesMemberAttribute(Type serializeAs)
        {
            SerializeAs = serializeAs;
        }

        /// <summary>
        /// Provides custom ".properties" object serialization instructions for the <see cref="Converters.ObjectConverter"/>.
        /// </summary>
        /// <param name="serializeAs">Specifies that this property should be serialized as the given type, rather than using the actual runtime value's type.</param>
        /// <param name="name">The name that should be used for the serialization and deserialization of this member.</param>
        public PropertiesMemberAttribute(Type serializeAs, string name) : this(serializeAs)
        {
            Name = name;
        }
    }

    /// <summary>
    /// Instructs the <see cref="Converters.ObjectConverter"/> that this comment should be written above this member.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public sealed class PropertiesCommentAttribute : Attribute
    {
        ///// <summary>
        ///// The comment handle to be used, if applicable.
        ///// </summary>
        //public char? Handle
        //{
        //    get => _handle;
        //    set => _handle = value.HasValue ?
        //        (value.Value == '#' || value.Value == '!' ? value : throw new PropertiesException("Command handle must be '#' or '!'")) : null;
        //}

        //private char? _handle;

        /// <summary>
        /// The content of this comment.
        /// </summary>
        public string Comment { get; set; }

        ///// <summary>
        ///// Instructs the <see cref="Converters.ObjectConverter"/> that this comment should be written above this member.
        ///// </summary>
        ///// <param name="handle">The comment handle to be used, if applicable.</param>
        ///// <param name="comment">The content of this comment.</param>
        //public PropertiesCommentAttribute(char handle, string comment)
        //{
        //    Handle = handle;
        //    Comment = comment;
        //}

        /// <summary>
        /// Instructs the <see cref="Converters.ObjectConverter"/> that this comment should be written above this member.
        /// </summary>
        /// <param name="comment">The content of this comment.</param>
        public PropertiesCommentAttribute(string comment)
        {
            Comment = comment;
        }

        /// <summary>
        /// Returns the text content of this comment with the handle.
        /// </summary>
        /// <returns>The text content of this comment with the handle.</returns>
        public override string ToString() => $"# {Comment}";
    }
}
