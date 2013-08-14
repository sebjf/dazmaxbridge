using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Max;

namespace MaxManagedBridge
{
    public partial class MaxPlugin : MaxBridge
    {
        public IEnumerable<IINode> SceneNodes
        {
            get
            {
                return GetAllSceneNodes(GlobalInterface.Instance.COREInterface.RootNode);
            }
        }

        private IEnumerable<IINode> GetAllSceneNodes(IINode start)
        {
            for (int i = 0; i < start.NumChildren; i++)
            {
                yield return start.GetChildNode(i);
            }
        }

        IEnumerable<IINode> SceneITriObjects
        {
            get
            {
                foreach (var n in GetAllSceneNodes(GlobalInterface.Instance.COREInterface.RootNode))
                {
                    if (n.ObjectRef is ITriObject)
                    {
                        yield return n;
                    }
                }
            }
        }

        IEnumerable<T> GetNodeObjectsByName<T>(string name) where T : class
        {
            foreach (var n in SceneNodes)
            {
                if (n.ObjectRef is T)
                {
                    if (n.Name == name)
                    {
                        yield return (n.ObjectRef as T);
                    }
                }
            }
        }




    }
}
