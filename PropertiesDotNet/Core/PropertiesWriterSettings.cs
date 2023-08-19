using System;

using PropertiesDotNet.Utils;

namespace PropertiesDotNet.Core
{
    /// <summary>
    /// Provides settings for an <see cref="IPropertiesWriter"/>.
    /// </summary>
    public class PropertiesWriterSettings : IEquatable<PropertiesWriterSettings>
    {
        /// <summary>
        /// Whether an <see cref="IPropertiesWriter"/> should ignore document comments.
        /// </summary>
        public virtual bool IgnoreComments { get; set; } = false;

        /// <summary>
        /// Whether an <see cref="IPropertiesWriter"/> should emit non-ISO-8859-1 characters as is or as a
        /// unicode escape sequence.
        /// </summary>
        public virtual bool AllCharacters { get; set; } = false;

        /// <summary>
        /// Whether an <see cref="IPropertiesWriter"/> should output the '\U' unicode identifier, 
        /// rather than only '\u', for larger code-points.
        /// </summary>
        public virtual bool AllUnicodeEscapes { get; set; } = false;

        /// <summary>
        /// Whether a <see cref="PropertiesException"/> should be thrown whenever an <see cref="IPropertiesWriter"/> is passed
        /// an incorrect token as an argument, depending on the context.
        /// </summary>
        public virtual bool ThrowOnError { get; set; } = true;

        /// <summary>
        /// Whether an <see cref="IPropertiesWriter"/> should close the underlying stream when it is disposed.
        /// </summary>
        public virtual bool CloseOnEnd { get; set; } = true;

        /// <summary>
        /// Whether an <see cref="IPropertiesWriter"/> should automatically flush after 
        /// <see cref="IPropertiesWriter.Write(PropertiesToken)"/> has been called <see cref="FlushInterval"/> number of times.
        /// </summary>
        public virtual bool AutoFlush { get; set; } = true;

        /// <summary>
        /// The number of <see cref="IPropertiesWriter.Write(PropertiesToken)"/> operations between each 
        /// automatic <see cref="IPropertiesWriter.Flush"/>. This only applies if <see cref="AutoFlush"/> is true.
        /// Must be greater than 0.
        /// </summary>
        public virtual uint FlushInterval
        {
            get => _flushInterval;
            set => _flushInterval = value > 0 ? value : throw new ArgumentException("Flush interval must be greater than 0", nameof(value));
        }

        private uint _flushInterval = 10;

        /// <summary>
        /// Returns a <see cref="PropertiesWriterSettings"/> with the default settings.
        /// </summary>
        public static PropertiesWriterSettings Default => new PropertiesWriterSettings();

        /// <summary>
        /// Creates a <see cref="PropertiesWriterSettings"/> with the given settings.
        /// </summary>
        /// <param name="ignoreComments">Whether an <see cref="IPropertiesWriter"/> should ignore document comments.</param>
        /// <param name="allCharacters">Whether an <see cref="IPropertiesWriter"/> should allow all characters, or only characters from the
        /// "ISO-8859-1" character set.</param>
        /// <param name="allUnicodeEscapes">Whether an <see cref="IPropertiesWriter"/> should output the '\U' unicode identifier, 
        /// rather than only '\u', for larger code-points.</param>
        /// <param name="throwOnError">Whether a <see cref="PropertiesException"/> should be thrown whenever an <see cref="IPropertiesWriter"/> is passed
        /// an incorrect token as an argument, depending on the context.</param>
        /// <param name="closeOnEnd">Whether an <see cref="IPropertiesWriter"/> should close the underlying stream when it is disposed.</param>
        /// <param name="autoFlush">Whether an <see cref="IPropertiesWriter"/> should automatically flush after 
        /// <see cref="IPropertiesWriter.Write(PropertiesToken)"/> has been called <see cref="FlushInterval"/> number of times.</param>
        /// <param name="flushInterval">The number of <see cref="IPropertiesWriter.Write(PropertiesToken)"/> operations between each 
        /// automatic <see cref="IPropertiesWriter.Flush"/>. This only applies if <see cref="AutoFlush"/> is true.
        /// Must be greater than 0.</param>
        public PropertiesWriterSettings(bool ignoreComments = false,
                                        bool allCharacters = false,
                                        bool allUnicodeEscapes = false,
                                        bool throwOnError = true,
                                        bool closeOnEnd = true,
                                        bool autoFlush = true,
                                        uint flushInterval = 10)
        {
            IgnoreComments = ignoreComments;
            AllCharacters = allCharacters;
            AllUnicodeEscapes = allUnicodeEscapes;
            ThrowOnError = throwOnError;
            CloseOnEnd = closeOnEnd;
            AutoFlush = autoFlush;
            FlushInterval = flushInterval;
        }

        internal PropertiesWriterSettings()
        {

        }

        /// <summary>
        /// Copies the configuration of the <paramref name="settings"/> into this instance.
        /// </summary>
        /// <param name="settings">The settings to copy.</param>
        public virtual void CopyFrom(PropertiesWriterSettings settings)
        {
            IgnoreComments = settings.IgnoreComments;
            AllCharacters = settings.AllCharacters;
            AllUnicodeEscapes = settings.AllUnicodeEscapes;
            ThrowOnError = settings.ThrowOnError;
            CloseOnEnd = settings.CloseOnEnd;
            AutoFlush = settings.AutoFlush;
            FlushInterval = settings.FlushInterval;
        }

        /// <inheritdoc/>
        public virtual bool Equals(PropertiesWriterSettings? other)
        {
            return
                IgnoreComments == other?.IgnoreComments &&
                AllCharacters == other?.AllCharacters &&
                AllUnicodeEscapes == other?.AllUnicodeEscapes &&
                ThrowOnError == other?.ThrowOnError &&
                CloseOnEnd == other?.CloseOnEnd &&
                AutoFlush == other?.AutoFlush &&
                FlushInterval == other?.FlushInterval;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => Equals(obj as PropertiesWriterSettings);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCodeHelper.GenerateHashCode(FlushInterval, (uint)HashCodeHelper.GenerateHashCode(IgnoreComments, 
            AllCharacters, AllUnicodeEscapes, ThrowOnError, CloseOnEnd, AutoFlush));
    }
}
