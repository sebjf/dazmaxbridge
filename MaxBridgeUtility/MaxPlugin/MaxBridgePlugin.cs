using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Max;


namespace MaxManagedBridge
{
    /* http://code.google.com/p/snakengine/source/browse/trunk/OtherLibs/3dmax+sdk/2012/include/INodeTransformModes.h?r=5 */
    enum PivotMode : int
    {
         PIV_NONE             = 0,
         PIV_PIVOT_ONLY       = 1,
         PIV_OBJECT_ONLY      = 2,
         PIV_HIERARCHY_ONLY   = 3
    }

    /*http://docs.autodesk.com/3DSMAX/15/ENU/3ds-Max-SDK-Programmer-Guide/index.html?url=files/GUID-B41DC781-221E-4DE3-8AA1-EC3C2666FC5C.htm,topicNumber=d30e22562 */
    public static class ClassIDs
    {
        public const uint XFORM_A = 622942244;
        public const uint XFORM_B = 0;
    }

    public partial class MaxPlugin : MaxBridge
    {
        public IEnumerable<IINode> GetMappedNodes(MyMesh source)
        {
            //This is our mapping function for now -> iterate over every node in the scene and return those with the correct name!
            return (SceneNodes.Where(n => (n.ObjectRef is ITriObject) && (n.Name == source.Name)));
        }

        public IEnumerable<IMesh> GetMappedMeshes(MyMesh source)
        {
            foreach (var n in GetMappedNodes(source))
            {
                yield return (n.ObjectRef as ITriObject).Mesh;
            }
        }

        public void UpdateAllMeshes()
        {
            foreach (MyMesh m in Scene.Items)
            {
                UpdateMesh(m);
            }
        }

        public bool UpdateMesh(MyMesh myMesh)
        {
            IList<IMesh> mappedMeshes = GetMappedMeshes(myMesh).ToList();

            //No mesh nodes exist with that name yet, so initialise a new node for the object
            if (mappedMeshes.Count < 1)
            {
                mappedMeshes.Add(CreateMeshNode(myMesh.Name));
            }

            foreach (var m in mappedMeshes)
            {
                UpdateMesh(m, myMesh);
            }

            return true;
        }

        public IMesh CreateMeshNode(string name)
        {
            ITriObject meshObject = GlobalInterface.Instance.CreateNewTriObject();
            IINode myNode = GlobalInterface.Instance.COREInterface.CreateObjectNode(meshObject);
            myNode.Name = name;

            myNode.Rotate(0, myNode.GetObjectTM(0, 
                GlobalInterface.Instance.Interval.Create()), 
                GlobalInterface.Instance.AngAxis.Create(1.0f, 0.0f, 0.0f, (float)DegreeToRadian(-90.0f)), 
                true, true, 
                (int)PivotMode.PIV_OBJECT_ONLY, 
                true);

            return meshObject.Mesh;
        }

    }
}
