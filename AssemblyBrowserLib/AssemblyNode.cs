using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace AssemblyBrowserLib
{
    public class AssemblyNode
    {
        public AssemblyNode(string fullSignature)
        {
            FullSignature = fullSignature;
        }

        public AssemblyNode(string fullSignature, List<AssemblyNode> childNodes) : this(fullSignature)
        {
            Children = childNodes;
        }

        public string FullSignature { get; }
        public List<AssemblyNode> Children { get; } = new();
    }
}