using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Max;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace MaxManagedBridge
{
    public partial class MaxPlugin : MaxBridge
    {
        protected void UpdateMeshSkin(IINode mesh, MyMesh myMesh)
        {
            //creating modifier and adding weights: http://dl3d.free.fr/phpBB2/viewtopic.php?t=478

            var skeleton_map = CreateSkeleton_BoneSys(myMesh);
            CreateSkinning(mesh, myMesh, skeleton_map);
        }

        #region Skeleton/Rig Creation

        protected Dictionary<string,IINode> CreateSkeleton(MyMesh mesh)
        {
            Dictionary<string, IINode> skeleton = new Dictionary<string, IINode>();

            foreach (MyBone bone in mesh.Skeleton.Bones)
            {
                IObject bone_obj = gi.COREInterface.CreateInstance(SClass_ID.Geomobject, gi.Class_ID.Create((uint)BuiltInClassIDA.BONE_OBJ_CLASSID, (uint)BuiltInClassIDB.BONE_OBJ_CLASSID)) as IObject;
                IINode bone_node = gi.COREInterface.CreateObjectNode(bone_obj);
                bone_node.Name = bone.Name;

                skeleton.Add(bone.Name, bone_node);
            }

            return skeleton;
        }

        protected Dictionary<string, IINode> CreateSkeleton_BoneSys(MyMesh mesh)
        {
            Dictionary<string, IINode> skeleton = new Dictionary<string, IINode>();

            foreach (MyBone bone in mesh.Skeleton.Bones)
            {
                skeleton.Add(bone.Name, CreateBone(bone));
            }

            return skeleton;
        }

        protected IINode CreateBone(MyBone bone)
        {
            string MaxScript = String.Format("(b = BoneSys.createBone [{0},{1},{2}] [{3},{4},{5}] [0,0,1]; b.name = \"{6}\";", 
                bone.OriginX,
                -bone.OriginZ,
                bone.OriginY,
                bone.EndpointX,
                -bone.EndpointZ,
                bone.EndpointY,
                bone.Name);

            if(bone.ParentName != "root")
            {
                MaxScript += String.Format("b.parent = ${0};",
                    bone.ParentName);
            }

            MaxScript += "(getHandleByAnim b) as String;)";

            string handle_string = ManagedServices.MaxscriptSDK.ExecuteStringMaxscriptQuery(MaxScript);
            handle_string = Regex.Replace(handle_string, "\\D", string.Empty);
            System.Int64 handle = System.Int64.Parse(handle_string);
            Autodesk.Max.IINode bonenode = gi.Animatable.GetAnimByHandle((UIntPtr)handle) as Autodesk.Max.IINode;

            if (bonenode == null)
            {
                Log.Add("Problem creating bone with maxscript: {" + MaxScript + "}", LogLevel.Error);
            }

            return bonenode.ActualINode;
        }

        #endregion

        #region Skinning Modifier Creation

        protected void CreateSkinning(IINode mesh, MyMesh myMesh, Dictionary<string, IINode> skeletonMap)
        {
            IModifier skin_modifier = GetSkinModifier(mesh);
            IISkinImportData skin = skin_modifier.GetInterface((InterfaceID)InterfaceIDs.I_SKINIMPORTDATA) as IISkinImportData;

            foreach (var bone in skeletonMap.Values)
            {
                skin.AddBoneEx(bone, false);
            }

            mesh.EvalWorldState(0, true); //must be called after adding bones, but before adding weights (presumably so that something in the graph realises the bones have been added when the weights need them

            List<vertexWeightMap> vertexWeights = GetWeightMapsByVertex(myMesh, skeletonMap);
            for (int i = 0; i < vertexWeights.Count; i++)
            {
                skin.AddWeights(mesh, i, vertexWeights[i].GetBonesITab(), vertexWeights[i].GetWeightsITab());
            }
        }

        protected class vertexWeightMap
        {
            public List<IINode> bones = new List<IINode>();
            public List<float> weights = new List<float>();

            public ITab<IINode> GetBonesITab()
            {
                IINodeTab tbones = Autodesk.Max.GlobalInterface.Instance.INodeTabNS.Create();
                foreach (IINode n in bones)
                {
                    tbones.AppendNode(n, true, 0);
                }                
                return tbones;
            }

            unsafe public ITab<float> GetWeightsITab()
            {
                ITab<float> tweights = Autodesk.Max.GlobalInterface.Instance.Tab.Create<float>();
                fixed(float* pWeights = weights.ToArray())
                {
                    tweights.Append(weights.Count, (IntPtr)pWeights, 0);
                }
                return tweights;
            }
        }

        /* This function takes a list of per-bone-weightmaps and converts them to a list of per-vertex-weightmaps */
        protected List<vertexWeightMap> GetWeightMapsByVertex(MyMesh mesh, Dictionary<string,IINode> skeleton)
        {
            List<vertexWeightMap> weights = new List<vertexWeightMap>();

            int vertexCount = mesh.WeightMaps[0].Weights.Count;
            for (int v = 0; v < vertexCount; v++)
            {
                weights.Add(new vertexWeightMap());

                for (int b = 0; b < mesh.WeightMaps.Count; b++)
                {
                    weights[v].bones.Add(skeleton[mesh.WeightMaps[b].BoneName]);
                    weights[v].weights.Add(mesh.WeightMaps[b].Weights[v]);
                }
            }

            return weights;
        }

        protected IModifier GetSkinModifier(IINode mesh)
        {
            IObject Obj = mesh.ObjectRef;
            IIDerivedObject dObj = Obj as IIDerivedObject;

            if (dObj == null)
            {
                dObj = Autodesk.Max.GlobalInterface.Instance.CreateDerivedObject(Obj);
                RefResult refs_transferred = dObj.TransferReferences(Obj, false);
                RefResult ref_replaced = dObj.ReferenceObject(Obj);
            }

            IClass_ID skinClassId = gi.Class_ID.Create(ClassIDs.Skin_A, ClassIDs.Skin_B);

            foreach (var m in dObj.Modifiers)
            {
                if (m.ClassID.EqualsClassID(skinClassId))
                {
                    return m;
                }
            }

            gi.SuspendAnimate();
            gi.AnimateOff();

            IModifier skin = gi.COREInterface.CreateInstance(SClass_ID.Osm, skinClassId) as IModifier;
            dObj.AddModifier(skin, null, 0);

            gi.AnimateOn();
            gi.ResumeAnimate();

            return skin;
        }

        #endregion

    }
}
