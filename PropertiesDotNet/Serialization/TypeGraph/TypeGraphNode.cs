using System;

namespace PropertiesDotNet.Serialization.TypeGraph
{
    public abstract class TypeGraphNode
    {
        internal const char DELIMETER = '.';
        
        public string Path { get; }
        public string Name { get; }
        public CompositeNode? Parent { get; }

        public TypeGraphNode(CompositeNode? parent, string name)
        {
            Parent = parent;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Path = parent is null ? name : parent.Path + DELIMETER + name;
        }
    }
}