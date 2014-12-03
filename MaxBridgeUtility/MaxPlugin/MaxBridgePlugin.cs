using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Max;


namespace MaxManagedBridge
{
    public partial class MaxPlugin : MaxBridge
    {
        protected IGlobal gi = null;

        public bool RebuildMaterials { get; set; }
        public bool RemoveTransparentFaces { get; set; }

        public MaterialLibraryView Templates { get; private set; }


        public MaxPlugin(IGlobal globalinterface)
        {
            this.gi = globalinterface;
            RebuildMaterials = false;
            RemoveTransparentFaces = true;
            Templates = new MaterialLibraryView(Defaults.MaterialLibraryFilename);
        }

        public void UpdateMeshes(MyScene scene)
        {
            try
            {
                gi.COREInterface.DisableSceneRedraw();
                gi.COREInterface.EnableUndo(false);

                foreach (var m in scene.Items)
                {
                    Log.Add("Updating mesh " + m.Name, LogLevel.Debug);
                    UpdateMeshData(m);
                }
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Caught exception in UpdateMeshes(): " + e.Message + e.TargetSite + e.StackTrace);
            }
            finally
            {
                gi.COREInterface.EnableUndo(true);
                gi.COREInterface.EnableSceneRedraw();

                gi.COREInterface10.RedrawViewportsNow(gi.COREInterface.Time, MaxFlags.VP_DONT_SIMPLIFY);
            }
        }

        /* Ideally we would create selection sets for each character so that they can be easily included/excluded from things like lighting */

        //public void AddToSelectionSet(string selectionSetName, IINode node)
        //{
        //    ITab<IINode> selectionSetNodes = globalInterface.Tab.Create<IINode>();
        //    selectionSetNodes.Append(1, node., 0);
        //    globalInterface.INamedSelectionSetManager.Instance.GetNamedSelSetList(selectionSetNodes,  globalInterface.INamedSelectionSetManager.Instance.get

        //    globalInterface.INamedSelectionSetManager.Instance.ReplaceNamedSelSet(
        //}

        protected void UpdateMeshData(MyMesh myMesh)
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

                UpdateMeshAnimation((m.ObjectRef as ITriObject), myMesh.Keyframes);

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

        protected IINode CreateMeshNode(MyMesh mesh)
        {
            IMatrix3 identity = gi.Matrix3.Create();
            identity.IdentityMatrix();

            ITriObject meshObject = gi.CreateNewTriObject();
            IINode myNode = gi.COREInterface.CreateObjectNode(meshObject);
            myNode.Name = mesh.Name;

            /* In this section we reset the transform of the node (as it may have been initialised with a rotation depending on what viewport was selected)
             * and rotate it with OBJECT ONLY flag set, which will set the object-offset transform which sits been the vertex position data and WSM */

            myNode.SetNodeTM(0, identity);

            myNode.Rotate(0, 
                identity,
                gi.AngAxis.Create(1.0f, 0.0f, 0.0f, (float)DegreeToRadian(-90.0f)), 
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

        protected void UpdateProgress(float progress, string message)
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
