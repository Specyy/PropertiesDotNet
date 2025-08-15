using PropertiesDotNet.Core;

namespace PropertiesDotNet.Serialization.PropertiesTree
{
    /// <summary>
    /// An implementation of an <see cref="IPropertiesTreeComposer"/> that only accepts and output flat (1-level) documents.
    /// </summary>
    public class FlatTreeComposer : IPropertiesTreeComposer
    {
        /// <inheritdoc/>
        public PropertiesObject ReadObject(IPropertiesReader input)
        {
            var rootNode = CreateRoot();

            string key = null!;

            while (input.MoveNext())
            {
                var token = input.Token;

                switch (token.Type)
                {
                    case PropertiesTokenType.Comment:
                    case PropertiesTokenType.Assigner:
                        continue;
                    case PropertiesTokenType.Value:
                        rootNode.AddProperty(key, token.Text);
                        break;
                    case PropertiesTokenType.Error:
                        throw new PropertiesException(token.Text);
                    case PropertiesTokenType.Key:
                        key = token.Text!;
                        break;
                    default:
                        throw new PropertiesException($"Cannot interpret token: {token.Type}!");
                }
            }

            return rootNode;
        }

        /// <inheritdoc/>
        public void WriteObject(PropertiesObject root, IPropertiesWriter output)
        {
            if (root.ChildCount != root.DeepChildCount)
                throw new PropertiesException("Cannot compose nested document tree");

            // Treat root comments as document-level comments
            WriteComments(root, output);

            foreach (var node in root)
            {
                string key = root.Name;
                string? value = node is PropertiesPrimitive primitive ? primitive.Value : null;

                WriteComments(node, output);
                output.WriteProperty(key, value);
            }

            output.Flush();
        }

        private void WriteComments(PropertiesTreeNode node, IPropertiesWriter output)
        {
            for (int i = 0; i < node.Comments?.Count; i++)
                output.Write(new PropertiesToken(PropertiesTokenType.Comment, node.Comments[i]));
        }

        /// <inheritdoc/>
        public PropertiesObject CreateRoot()
        {
            // TODO: fix naming
            return new PropertiesObject(null!);
        }
    }
}