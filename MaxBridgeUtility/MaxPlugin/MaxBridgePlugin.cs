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
        protected IGlobal globalInterface = null;

        public bool RebuildMaterials { get; set; }
        public bool RemoveTransparentFaces { get; set; }

        public MaxPlugin(IGlobal globalinterface)
        {
            this.globalInterface = globalinterface;
            RebuildMaterials = false;
            RemoveTransparentFaces = true;
        }

        public IEnumerable<IINode> GetMappedNodes(string Name)
        {
            //This is our mapping function for now -> iterate over every node in the scene and return those with the correct name!
            return (SceneNodes.Where(n => (n.ObjectRef is ITriObject) && (n.Name == Name)));
        }

        public void UpdateMeshes(MyScene scene)
        {
            Log.Add("[m0] Received scene from Daz. Updating mesh data...");

            try
            {
                UpdateMeshes(scene.Items);
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Caught exception in UpdateMeshes(): " + e.Message + e.TargetSite + e.StackTrace);
            }
        }

        public void UpdateMeshes(IEnumerable<MyMesh> myMeshes)
        {
            Log.Add("[m2] Disabling undo and redraw");

            globalInterface.COREInterface.DisableSceneRedraw();
            globalInterface.COREInterface.EnableUndo(false);

            foreach (var m in myMeshes)
            {
                Log.Add("[m3] Updating mesh " + m.Name);

                UpdateMeshData(m);
            }

            Log.Add("[m4] Enabling undo and redraw");

            globalInterface.COREInterface.EnableUndo(true);
            globalInterface.COREInterface.EnableSceneRedraw();
        }

        /* Ideally we would create selection sets for each character so that they can be easily included/excluded from things like lighting */

        //public void AddToSelectionSet(string selectionSetName, IINode node)
        //{
        //    ITab<IINode> selectionSetNodes = globalInterface.Tab.Create<IINode>();
        //    selectionSetNodes.Append(1, node., 0);
        //    globalInterface.INamedSelectionSetManager.Instance.GetNamedSelSetList(selectionSetNodes,  globalInterface.INamedSelectionSetManager.Instance.get

        //    globalInterface.INamedSelectionSetManager.Instance.ReplaceNamedSelSet(
        //}

        public void UpdateMeshData(MyMesh myMesh)
        {
            UpdateProgress(0.0f, "Getting mapped items...");

            IList<IINode> mappedNodes = GetMappedNodes(myMesh.Name).ToList();

            //No mesh nodes exist with that name yet, so initialise a new node for the object
            if (mappedNodes.Count < 1)
            {
                UpdateProgress(0.1f, "Creating object...");

                mappedNodes.Add(CreateMeshNode(myMesh));
            }

            IList<MaterialWrapper> Materials = GetMaterials(myMesh).ToList();

            foreach (var m in mappedNodes)
            {
                UpdateProgress(0.2f, "Updating geometry...");

                if (RemoveTransparentFaces)
                {
                    foreach (var transparentMaterial in Materials.Where(ms => ms.IsTransparent))
                    {
                        RemoveFacesByMaterialId(myMesh, transparentMaterial.MaterialIndex);
                    }
                }

                UpdateMesh((m.ObjectRef as ITriObject).Mesh, myMesh);

                if (RebuildMaterials)
                {
                    m.Mtl = null;
                }

                if (m.Mtl == null)
                {
                    UpdateProgress(0.6f, "Updating materials...");

                    m.Mtl = CreateMultiMaterial(Materials);
                }
            }

            UpdateProgress(1.0f, "Done.");
        }

        public IINode CreateMeshNode(MyMesh mesh)
        {
            IMatrix3 identity = globalInterface.Matrix3.Create();
            identity.IdentityMatrix();

            ITriObject meshObject = globalInterface.CreateNewTriObject();
            IINode myNode = globalInterface.COREInterface.CreateObjectNode(meshObject);
            myNode.Name = mesh.Name;

            /* In this section we reset the transform of the node (as it may have been initialised with a rotation depending on what viewport was selected)
             * and rotate it with OBJECT ONLY flag set, which will set the object-offset transform which sits been the vertex position data and WSM */

            myNode.SetNodeTM(0, identity);

            myNode.Rotate(0, 
                identity,
                globalInterface.AngAxis.Create(1.0f, 0.0f, 0.0f, (float)DegreeToRadian(-90.0f)), 
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
            if (ProgressCallback != null)
            {
                ProgressCallback(progress, message);
            }
        }

        public delegate void ProgressUpdateHandler(float progress, string message);
        public ProgressUpdateHandler ProgressCallback;
        public event ProgressUpdateHandler ProgressChanged;
    }
}
