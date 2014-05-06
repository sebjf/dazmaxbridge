using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Max;

namespace MaxManagedBridge
{
    public partial class MaxPlugin : MaxBridge
    {
        public IEnumerable<IINode> GetMappedNodes(string Name)
        {
            //This is our mapping function for now -> iterate over every node in the scene and return those with the correct name!
            return (SceneNodes.Where(n => (n.ObjectRef is ITriObject) && (n.Name == Name)));
        }

        public IEnumerable<IINode> SceneNodes
        {
            get
            {
                return GetChildNodesRecursive(globalInterface.COREInterface.RootNode);
            }
        }

        private IEnumerable<IINode> GetChildNodesRecursive(IINode start)
        {
            for (int i = 0; i < start.NumChildren; i++)
            {
                yield return start.GetChildNode(i);

                foreach(var n in GetChildNodesRecursive(start.GetChildNode(i)))
                    yield return n;
            }
        }

    }
}
