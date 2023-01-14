using System;

namespace PropertiesDotNet.Core
{
    /// <summary>
    /// Provides settings for an <see cref="IPropertiesReader"/>. Inheriting this class allows for
    /// additional custom settings on top of the defaults.
    /// </summary>
    public class PropertiesReaderSettings : IEquatable<PropertiesReaderSettings>
    {
        /// <summary>
        /// Whether an <see cref="IPropertiesReader"/> should ignore document comments.
        /// </summary>
        public virtual bool IgnoreComments { get; set; } = false;

        /// <summary>
        /// Whether an <see cref="IPropertiesReader"/> should error on invalid character escapes.
        /// </summary>
        public virtual bool InvalidEscapes { get; set; } = true;

        /// <summary>
        /// Whether a <see cref="IPropertiesReader"/> should allow all Unicode escape identifiers,
        /// including '\x' and '\U', rather than only '\u'.
        /// </summary>
        public virtual bool AllUnicodeEscapes { get; set; } = false;

        /// <summary>
        /// Whether an <see cref="IPropertiesReader"/> should allow all characters, or only characters from the
        /// "ISO-8859-1" character set.
        /// </summary>
        public virtual bool AllCharacters { get; set; } = false;

        /// <summary>
        /// Whether a <see cref="PropertiesException"/> should be thrown whenever a <see cref="IPropertiesReader"/> encounters
        /// an error in a ".properties" document, or if an <see cref="PropertiesTokenType.Error"/> should be produced.
        /// </summary>
        public virtual bool ThrowOnError { get; set; } = true;

        /// <summary>
        /// Whether an <see cref="IPropertiesReader"/> should close the underlying stream when a <see cref="PropertiesTokenType.Error"/>
        /// is produced or the end of the document is reached.
        /// </summary>
        public virtual bool CloseOnEnd { get; set; } = true;

        /// <summary>
        /// Returns a <see cref="PropertiesReaderSettings"/> with the default settings.
        /// </summary>
        public static PropertiesReaderSettings Default => new PropertiesReaderSettings();

        /// <summary>
        /// Creates a new <see cref="PropertiesReaderSettings"/> with the given settings.
        /// </summary>
        /// <param name="ignoreComments">Whether an <see cref="IPropertiesReader"/> should ignore document comments.</param>
        /// <param name="invalidEscapes">Whether an <see cref="IPropertiesReader"/> should error on invalid character escapes. The specification mandates
        /// this setting be true; only change if you wish to deal with the errors manually.</param>
        /// <param name="allUnicodeEscapes">Whether a <see cref="IPropertiesReader"/> should allow all Unicode escape identifiers,
        /// including '\x' and '\U', rather than only '\u'.</param>
        /// <param name="allCharacters">Whether an <see cref="IPropertiesReader"/> should allow all characters, or only characters from the
        /// "ISO-8859-1" character set.</param>
        /// <param name="throwOnError">Whether a <see cref="PropertiesException"/> should be thrown whenever a <see cref="IPropertiesReader"/> encounters
        /// an error in a ".properties" document, or if an <see cref="PropertiesTokenType.Error"/> should be produced.</param>
        /// <param name="closeOnEnd">Whether an <see cref="IPropertiesReader"/> should close the underlying stream when a <see cref="Events.DocumentEnd"/>
        /// is produced.</param>
        public PropertiesReaderSettings(bool ignoreComments = false,
                                        bool invalidEscapes = true,
                                        bool allUnicodeEscapes = false,
                                        bool allCharacters = false,
                                        bool throwOnError = true,
                                        bool closeOnEnd = true)
        {
            IgnoreComments = ignoreComments;
            InvalidEscapes = invalidEscapes;
            AllUnicodeEscapes = allUnicodeEscapes;
            AllCharacters = allCharacters;
            ThrowOnError = throwOnError;
            CloseOnEnd = closeOnEnd;
        }

        private PropertiesReaderSettings()
        {
        }
        
        /// <summary>
        /// Copies the configuration of the <paramref name="settings"/> into this instance.
        /// </summary>
        /// <param name="settings">The settings to copy.</param>
        public void CopyFrom(PropertiesReaderSettings settings)
        {
            IgnoreComments = settings.IgnoreComments;
            InvalidEscapes = settings.InvalidEscapes;
            AllUnicodeEscapes = settings.AllUnicodeEscapes;
            AllCharacters = settings.AllCharacters;
            ThrowOnError = settings.ThrowOnError;
            CloseOnEnd = settings.CloseOnEnd;
        }

        /// <inheritdoc/>
        public bool Equals(PropertiesReaderSettings? other)
        {
            return 
                IgnoreComments == other?.IgnoreComments &&
                InvalidEscapes == other?.InvalidEscapes &&
                AllUnicodeEscapes == other?.AllUnicodeEscapes &&
                AllCharacters == other?.AllCharacters &&
                ThrowOnError == other?.ThrowOnError &&
                CloseOnEnd == other?.CloseOnEnd;
        }
    }
}
