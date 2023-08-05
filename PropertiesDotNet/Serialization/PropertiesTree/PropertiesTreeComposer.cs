using System;
using System.Collections.Generic;
using System.Text;

using PropertiesDotNet.Core;
using PropertiesDotNet.ObjectModel;

namespace PropertiesDotNet.Serialization.PropertiesTree
{
    /// <summary>
    /// A default implementation of an <see cref="IPropertiesTreeComposer"/>.
    /// </summary>
    public sealed class PropertiesTreeComposer : IPropertiesTreeComposer
    {
        /// <summary>
        /// The default node delimeter.
        /// </summary>
        public const char DEFFAULT_DELIMETER = '.';

        /// <summary>
        /// The delimeter used to differentiate the nodes.
        /// </summary>
        public char Delimeter { get; set; } = DEFFAULT_DELIMETER;

        private StringBuilder _keyBuilder;

        /// <summary>
        /// A default implementation of an <see cref="IPropertiesTreeComposer"/>.
        /// </summary>
        public PropertiesTreeComposer()
        {
            // Lazy init
            _keyBuilder = null!;
        }

        /// <summary>
        /// Reads the input document and constructs a browsable object tree, with nodes separated by the <see cref="Delimeter"/>.
        /// </summary>
        /// <param name="input">The input document.</param>
        /// <returns>The root of the object tree. In a standard ".properties" document this is not contained
        /// within the document itself.</returns>
        public PropertiesObject ReadObject(PropertiesDocument input)
        {
            // TODO: fix naming
            var rootNode = CreateRoot();

            foreach (var prop in input)
                ReadObject(prop, rootNode, prop.Key.Split(Delimeter));

            return rootNode;
        }

        private void ReadObject(PropertiesProperty prop, PropertiesObject parent, string[] nodeNames, int depth = 0)
        {
            if (depth == nodeNames.Length - 1)
            {
                parent.Add(new PropertiesPrimitive(nodeNames[nodeNames.Length - 1], prop.Value));
                return;
            }

            PropertiesObject obj;

            if (parent.Contains(nodeNames[depth]))
            {
                obj = parent[nodeNames[depth]] as PropertiesObject ?? throw new PropertiesException($"Cannot compose a predefined primitive property (\"{string.Join(Delimeter.ToString(), nodeNames, 0, depth + 1)}\")");
            }
            else
            {
                parent.Add(obj = new PropertiesObject(nodeNames[depth]));
            }

            ReadObject(prop, obj, nodeNames, ++depth);
        }


        /// <inheritdoc/>
        public PropertiesObject ReadObject(IPropertiesReader input)
        {
            // TODO: fix naming
            var rootNode = CreateRoot();

            while (input.MoveNext())
            {
                switch (input.Token.Type)
                {
                    case PropertiesTokenType.Comment:
                    case PropertiesTokenType.Assigner:
                    case PropertiesTokenType.Value:
                        continue;
                    case PropertiesTokenType.Error:
                        throw new PropertiesException(input.Token.Text);
                    case PropertiesTokenType.Key:
                        ReadObject(input, rootNode, input.Token.Text.Split(Delimeter));
                        break;
                    default:
                        throw new PropertiesException($"Cannot intepret token: {input.Token.Type}!");
                }
            }

            return rootNode;
        }

        private void ReadObject(IPropertiesReader input, PropertiesObject parent, string[] nodeNames, int depth = 0)
        {
            if (depth == nodeNames.Length - 1)
            {
                if (input.MoveNext() && input.Token.Type == PropertiesTokenType.Assigner)
                    input.MoveNext();

                if (input.Token.Type != PropertiesTokenType.Value)
                    throw new PropertiesException($"Expected {PropertiesTokenType.Value} got {input.Token.Type}! (\"{input.Token.Text}\")");

                parent.Add(new PropertiesPrimitive(nodeNames[nodeNames.Length - 1], input.Token.Text));
                return;
            }

            PropertiesObject obj;

            if (parent.Contains(nodeNames[depth]))
            {
                obj = parent[nodeNames[depth]] as PropertiesObject ?? throw new PropertiesException($"Cannot compose a predefined primitive property (\"{string.Join(Delimeter.ToString(), nodeNames, 0, depth + 1)}\") (Line: {input.TokenStart?.Line} Column: {input.TokenStart?.Column})");
            }
            else
            {
                parent.Add(obj = new PropertiesObject(nodeNames[depth]));
            }

            ReadObject(input, obj, nodeNames, ++depth);
        }

        /// <inheritdoc/>
        public void WriteObject(PropertiesObject root, IPropertiesWriter output)
        {
            WriteObject(null, root, output);
        }

        private void WriteObject(PropertiesObject? parent, PropertiesObject obj, IPropertiesWriter output)
        {
            _keyBuilder ??= new StringBuilder();
            bool trueRoot = false;

            foreach (var node in obj)
            {
                if (_keyBuilder.Length == 0)
                    trueRoot = true;
                else
                    _keyBuilder.Append(Delimeter);

                _keyBuilder.Append(node.Name);

                // TODO: Allow customization of handle
                for (int i = 0; i < parent?.Comments?.Count; i++)
                    output.Write(new PropertiesToken(PropertiesTokenType.Comment, parent.Comments[i]));

                // TODO: Allow customization of handle
                for (int i = 0; i < node.Comments?.Count; i++)
                    output.Write(new PropertiesToken(PropertiesTokenType.Comment, node.Comments[i]));

                if (node is PropertiesPrimitive prop)
                {
                    output.WriteProperty(_keyBuilder.ToString(), prop.Value);
                }
                else if (node is PropertiesObject childObj)
                {
                    WriteObject(obj, childObj, output);
                }
                else throw new ArgumentException($"Cannot write tree node of type: {node.GetType()}");

                // 1 = delimeter
                _keyBuilder.Length -= (trueRoot ? 0 : 1) + node.Name.Length;
            }
        }

        /// <summary>
        /// Writes an object tree into the specified document.
        /// </summary>
        /// <param name="root">The root of the object tree to write.</param>
        /// <param name="doc">The output document.</param>
        /// <exception cref="ArgumentException">The tree contains nodes that cannot be serialized.</exception>
        public void WriteObject(PropertiesObject root, PropertiesDocument doc)
        {
            WriteObject(null, root, doc);
        }

        private void WriteObject(PropertiesObject? parent, PropertiesObject obj, PropertiesDocument doc)
        {
            _keyBuilder ??= new StringBuilder();
            bool trueRoot = false;

            foreach (var node in obj)
            {
                if (_keyBuilder.Length == 0)
                    trueRoot = true;
                else
                    _keyBuilder.Append(Delimeter);

                _keyBuilder.Append(node.Name);

                if (node is PropertiesPrimitive prop)
                {
                    var property = new PropertiesProperty(_keyBuilder.ToString(), prop.Value);

                    if (parent != null)
                        property.Comments = parent.Comments;

                    if(node.Comments != null)
                    {
                        if(property.Comments is null)
                            prop.Comments = node.Comments;
                        else
                            prop.Comments.AddRange(node.Comments);
                    }

                    doc.AddProperty(property);
                }
                else if (node is PropertiesObject childObj)
                {
                    WriteObject(obj, childObj, doc);
                }
                else throw new ArgumentException($"Cannot write tree node of type: {node.GetType()}");

                // 1 = delimeter
                _keyBuilder.Length -= (trueRoot ? 0 : 1) + node.Name.Length;
            }
        }

        /// <inheritdoc/>
        public PropertiesObject CreateRoot() => new PropertiesObject(null!);
    }
}
