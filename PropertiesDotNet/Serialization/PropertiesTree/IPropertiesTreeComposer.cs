using PropertiesDotNet.Core;

namespace PropertiesDotNet.Serialization.PropertiesTree
{
    /// <summary>
    /// Represents a type that reads ".properties" documents into a tree of properties or objects. This tree can be emitted as 
    /// a properties document.
    /// </summary>
    public interface IPropertiesTreeComposer
    {
        /// <summary>
        /// Reads the input document and constructs a browsable object tree.
        /// </summary>
        /// <param name="input">The input document.</param>
        /// <returns>The root of the object tree. In a standard ".properties" document this is not contained
        /// within the document itself.</returns>
        /// <exception cref="PropertiesException">If an error was encountered whilst trying to
        /// read the document.</exception>
        PropertiesObject ReadObject(IPropertiesReader input);

        /// <summary>
        /// Writes an object tree into the specified document.
        /// </summary>
        /// <param name="output">The output document.</param>
        /// <param name="root">The root of the object tree to write.</param>
        /// <exception cref="PropertiesException">If an error was encountered whilst trying to
        /// write the document.</exception>
        void WriteObject(PropertiesObject root, IPropertiesWriter output);

        /// <summary>
        /// Creates an object that should be used as the root of the document tree.
        /// </summary>
        /// <returns>The root of a potential document tree.</returns>
        PropertiesObject CreateRoot();
    }
}
