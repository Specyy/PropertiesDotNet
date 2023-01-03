using System;

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
        /// Whether an <see cref="IPropertiesWriter"/> should allow all characters, or only characters from the
        /// "ISO-8859-1" character set.
        /// </summary>
        public virtual bool AllCharacters { get; set; } = false;

        /// <summary>
        /// Whether an <see cref="IPropertiesWriter"/> should output the '\U' unicode identifier, 
        /// rather than only '\u', for larger code-points.
        /// </summary>
        public virtual bool AllUnicodeEscapes { get; set; } = false;

        /// <summary>
        /// Whether an <see cref="IPropertiesWriter"/> should close the stream when a <see cref="Events.DocumentEnd"/>
        /// is produced.
        /// </summary>
        public virtual bool CloseStreamOnEnd { get; set; } = true;

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
        /// <param name="closeStreamOnEnd">Whether an <see cref="IPropertiesWriter"/> should close the stream when a <see cref="Events.DocumentEnd"/>
        /// is produced.</param>
        public PropertiesWriterSettings(bool ignoreComments = false, bool allCharacters = false, bool allUnicodeEscapes = false, bool closeStreamOnEnd = true)
        {
            IgnoreComments = ignoreComments;
            AllCharacters = allCharacters;
            AllUnicodeEscapes = allUnicodeEscapes;
            CloseStreamOnEnd = closeStreamOnEnd;
        }

        internal PropertiesWriterSettings()
        {
            
        }

        /// <inheritdoc/>
        public bool Equals(PropertiesWriterSettings? other)
        {
            return 
                IgnoreComments == other?.IgnoreComments &&
                AllCharacters == other?.AllCharacters &&
                AllUnicodeEscapes == other?.AllUnicodeEscapes &&
                CloseStreamOnEnd == other?.CloseStreamOnEnd;
        }
    }
}
