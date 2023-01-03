using System;
using System.Collections.Generic;

namespace PropertiesDotNet.Serialization.TypeGraph
{
    public class TypeGraphComposer
    {
        public Type Type { get; }

        private Dictionary<string, string> _typeGraph;

        public TypeGraphComposer(Type type)
        {
            _typeGraph = new Dictionary<string, string>();
        }
    }
}