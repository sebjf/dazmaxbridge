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
        public const uint BitmapTexture_A = 576;
        public const uint RGB_Multiply_A = 656;
        public const uint StandardMaterial_A = 2;
        public const uint MultiMaterial = 512;
    }

    public partial class MaxPlugin : MaxBridge
    {
        public IEnumerable<IINode> GetMappedNodes(string Name)
        {
            //This is our mapping function for now -> iterate over every node in the scene and return those with the correct name!
            return (SceneNodes.Where(n => (n.ObjectRef is ITriObject) && (n.Name == Name)));
        }

        public void UpdateMeshes(IList<string> items)
        {
            var Scene = UpdateFromDaz(items);
            foreach (MyMesh m in Scene.Items)
            {
                UpdateMesh(m);
            }
        }

        public bool UpdateMesh(MyMesh myMesh)
        {
            //Autodesk.Max.IInterface.DisableSceneRedraw()
            GlobalInterface.Instance.COREInterface.DisableSceneRedraw();
            GlobalInterface.Instance.COREInterface.EnableUndo(false);

            UpdateProgress(0.0f, "Getting mapped items...");

            IList<IINode> mappedNodes = GetMappedNodes(myMesh.Name).ToList();

            //No mesh nodes exist with that name yet, so initialise a new node for the object
            if (mappedNodes.Count < 1)
            {
                UpdateProgress(0.1f, "Creating object...");
                mappedNodes.Add(CreateMeshNode(myMesh));
            }

            foreach (var m in mappedNodes)
            {
                UpdateProgress(0.2f, "Updating geometry...");
                UpdateMesh((m.ObjectRef as ITriObject).Mesh, myMesh);

                if (m.Mtl == null)
                {
                    UpdateProgress(0.6f, "Updating materials...");
                    m.Mtl = CreateMaterial(myMesh);
                }
            }

            UpdateProgress(1.0f, "Done.");

            GlobalInterface.Instance.COREInterface.EnableUndo(true);
            GlobalInterface.Instance.COREInterface.EnableSceneRedraw();

            return true;
        }

        public IINode CreateMeshNode(MyMesh mesh)
        {
            ITriObject meshObject = GlobalInterface.Instance.CreateNewTriObject();
            IINode myNode = GlobalInterface.Instance.COREInterface.CreateObjectNode(meshObject);
            myNode.Name = mesh.Name;

            myNode.Rotate(0, myNode.GetObjectTM(0, 
                GlobalInterface.Instance.Interval.Create()), 
                GlobalInterface.Instance.AngAxis.Create(1.0f, 0.0f, 0.0f, (float)DegreeToRadian(-90.0f)), 
                true, true, 
                (int)PivotMode.PIV_OBJECT_ONLY, 
                true);

            if (mesh.ParentName != null && mesh.ParentName != mesh.Name)
            {
                var parentNode = GetMappedNodes(mesh.ParentName).FirstOrDefault();
                if (parentNode != null)
                {
                    parentNode.AttachChild(myNode, false);
                }
            }

            return myNode;
        }

        public void UpdateProgress(float progress, string message)
        {
            if (ProgressChanged != null)
            {
                ProgressChanged(progress, message);
            }
        }

        public delegate void ProgressUpdateHandler(float progress, string message);
        public event ProgressUpdateHandler ProgressChanged;
    }
}
