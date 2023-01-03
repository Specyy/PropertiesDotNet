// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("DocumentationHeader", "ClassDocumentationHeader:The class must have a documentation header.", Justification = "<Pending>", Scope = "type", Target = "~T:PropertiesDotNet.Test.BenchmarkTester")]
[assembly: SuppressMessage("DocumentationHeader", "MethodDocumentationHeader:The method must have a documentation header.", Justification = "<Pending>", Scope = "member", Target = "~M:PropertiesDotNet.Test.BenchmarkTester.PDN_Read")]
[assembly: SuppressMessage("DocumentationHeader", "ClassDocumentationHeader:The class must have a documentation header.", Justification = "<Pending>", Scope = "type", Target = "~T:PropertiesDotNet.Test.FeatureTester")]


/*
// NodeGraphComposer
// NodeGraphConstructor
public sealed class ObjectGraphComposer : IObjectGraphComposer {
    
}

public abstract class ObjectGraphNode {
    public string Name; // get
}


public class ObjectGraph : ObjectGraphNode {
    public List<ObjectGraphNode> Children; // get set
    
    public ObjectGraph(List<ObjectGraphNode> children){
	Children = children ?? throw 1;
    }
}

public class ObjectLeaf : ObjectGraphNode {
    public string Value; // get set?

    public ObjectLeaf (string value){
	Value = value;
    }
}

 */