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
            // Treat root comments as document-level comments
            WriteComments(root, output);
            WriteObject(root, null, output);

            if (_keyBuilder != null)
                _keyBuilder.Length = 0;

            output.Flush();
        }

        private void WriteObject(PropertiesObject obj, List<PropertiesObject>? parents, IPropertiesWriter output)
        {
            foreach (var node in obj)
            {
                if (node is PropertiesPrimitive primitive)
                {
                    for (int i = 0; i < parents?.Count; i++)
                        WriteComments(parents[i], output);

                    WriteComments(primitive, output);

                    if (parents is null)
                    {
                        output.WriteProperty(primitive.Name, primitive.Value);
                    }
                    else
                    {
                        output.WriteProperty(_keyBuilder.Append(primitive.Name).ToString(), primitive.Value);
                        _keyBuilder.Length -= primitive.Name.Length;
                    }

                }
                else if (node is PropertiesObject objNode)
                {
                    _keyBuilder ??= new StringBuilder();
                    _keyBuilder.Append(objNode.Name).Append(Delimeter);

                    parents ??= new List<PropertiesObject>();
                    int index = parents.Count;

                    parents.Add(objNode);
                    WriteObject(objNode, parents, output);
                    parents.RemoveAt(index);
                    // 1 = delimeter
                    _keyBuilder.Length -= objNode.Name.Length + 1;
                }
                else throw new ArgumentException($"Cannot write tree node of type: {node.GetType()}");
            }
        }

        private void WriteComments(PropertiesTreeNode node, IPropertiesWriter output)
        {
            for (int i = 0; i < node.Comments?.Count; i++)
                output.Write(new PropertiesToken(PropertiesTokenType.Comment, node.Comments[i]));
        }

        /// <summary>
        /// Writes an object tree into the specified document.
        /// </summary>
        /// <param name="root">The root of the object tree to write.</param>
        /// <param name="doc">The output document.</param>
        /// <exception cref="ArgumentException">The tree contains nodes that cannot be serialized.</exception>
        public void WriteObject(PropertiesObject root, PropertiesDocument doc)
        {
            WriteObject(root, null, doc);

            if (_keyBuilder != null)
                _keyBuilder.Length = 0;
        }

        private void WriteObject(PropertiesObject obj, List<PropertiesObject>? parents, PropertiesDocument doc)
        {
            foreach (var node in obj)
            {
                if (node is PropertiesPrimitive primitive)
                {
                    string key;

                    if (parents is null)
                    {
                        key = primitive.Name;
                    }
                    else
                    {
                        key = _keyBuilder.Append(primitive.Name).ToString();
                        _keyBuilder.Length -= primitive.Name.Length;
                    }

                    var prop = new PropertiesProperty(key, primitive.Value);

                    for (int i = 0; i < parents?.Count; i++)
                        AddComments(prop, parents[i]);

                    AddComments(prop, node);

                    doc.AddProperty(prop);
                }
                else if (node is PropertiesObject objNode)
                {
                    _keyBuilder ??= new StringBuilder();
                    _keyBuilder.Append(objNode.Name).Append(Delimeter);

                    parents ??= new List<PropertiesObject>();
                    int index = parents.Count;

                    parents.Add(objNode);
                    WriteObject(objNode, parents, doc);
                    parents.RemoveAt(index);
                    // 1 = delimeter
                    _keyBuilder.Length -= objNode.Name.Length + 1;
                }
                else throw new ArgumentException($"Cannot write tree node of type: {node.GetType()}");
            }
        }

        private void AddComments(PropertiesProperty prop, PropertiesTreeNode node)
        {
            if (prop.Comments is null)
                prop.Comments ??= new List<string>();

            for (int i = 0; i < node.Comments?.Count; i++)
                prop.Comments.Add(node.Comments[i]);
        }

        /// <inheritdoc/>
        public PropertiesObject CreateRoot() => new PropertiesObject(null!);
    }
}