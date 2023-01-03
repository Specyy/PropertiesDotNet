namespace PropertiesDotNet.Serialization.TypeGraph
{
    public class CompositeNode : TypeGraphNode
    {
        public CompositeNode(CompositeNode? parent, string name) : base(parent, name)
        {
        }
    }
}