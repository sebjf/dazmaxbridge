using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Max;

namespace MaxManagedBridge
{
    public class MaxBridgePlugin : MaxBridge
    {
        public string PrintName(System.Int64 handle)
        {
            var node = ObjectFromHandle<IAnimatable>(handle);
            return (node.NodeName + " of type " + node.ClassName);
        }

        public T ObjectFromHandle<T>(System.Int64 handle) where T : IAnimatable
        {
            IAnimatable anim = Autodesk.Max.GlobalInterface.Instance.Animatable.GetAnimByHandle((UIntPtr)handle);
            if (anim is T){
                return (T)anim;
            }
            else{
                throw new ArgumentException("Handle is of the type " + anim.GetType().Name + " not " + typeof(T).Name);
            }
        }

        public System.Int64 HandleFromObject(IAnimatable obj)
        {
            return (System.Int64)Autodesk.Max.GlobalInterface.Instance.Animatable.GetHandleByAnim(obj);
        }

        public void PopulateMesh(Int64 handle, int meshId)
        {
      //      PopulateMesh(ObjectFromHandle<IMesh>(handle), Scene.Items[meshId]);
        }

        public void PopulateMesh(IMesh myMesh, MaxMesh mesh)
        {
            myMesh.SetNumVerts(mesh.NumVertices, false, false);
            for (int i = 0; i < mesh.NumVertices; i++)
            {
                myMesh.SetVert(i, mesh.Vertices[(i * 3) + 0], mesh.Vertices[(i * 3) + 1], mesh.Vertices[(i * 3) + 2]);
            }

            myMesh.SetNumTVerts(mesh.NumTextureCoordinates, false);
            int elementsPerVertex = mesh.TextureCoordinates.Count / mesh.NumTextureCoordinates;
            switch (elementsPerVertex)
            {
                case 2:
                    for (int i = 0; i < mesh.NumTextureCoordinates; i++)
                    {
                        myMesh.SetTVert(i, mesh.TextureCoordinates[(i * 2) + 0], mesh.TextureCoordinates[(i * 2) + 1], 0.0f);
                    }
                    break;
                case 3:
                    for (int i = 0; i < mesh.NumTextureCoordinates; i++)
                    {
                        myMesh.SetTVert(i, mesh.TextureCoordinates[(i * 3) + 0], mesh.TextureCoordinates[(i * 3) + 1], mesh.TextureCoordinates[(i * 3) + 2]);
                    }
                    break;
                default:
                    throw new NotImplementedException(("Unable to handle texture coordinates with " + elementsPerVertex + " coordinates."));
            }

            TriangulateFaces(mesh);

            myMesh.SetNumFaces(mesh.TriangulatedFaces.Length, false, false);
            myMesh.SetNumTVFaces(mesh.TriangulatedFaces.Length, false, 0);
            for (int i = 0; i < mesh.TriangulatedFaces.Length; i++)
            {
                Face myFace = mesh.TriangulatedFaces[i];
                myMesh.Faces[i].SetVerts(myFace.PositionVertex1, myFace.PositionVertex2, myFace.PositionVertex3);
                myMesh.TvFace[i].SetTVerts(myFace.TextureVertex1, myFace.TextureVertex2, myFace.TextureVertex3);
                myMesh.Faces[i].MatID = (ushort)myFace.MaterialId;
            }

            myMesh.InvalidateGeomCache();
            myMesh.InvalidateTopologyCache();
        }

        //protected IMultiMtl MakeMultiMaterial(MaxMesh mesh)
        //{
        //    IMultiMtl myMaterial = global.NewDefaultMultiMtl;

        //    myMaterial.SetNumSubMtls(mesh.Materials.Keys.ToArray().Max() + 1);

        //    foreach (int i in mesh.Materials.Keys.ToArray())
        //    {
        //        myMaterial.SetSubMtl(i, MakeStandardMaterial(mesh.Materials[i]));
        //    }

        //    return myMaterial;
        //}

        //protected IMtl MakeStandardMaterial(Material material)
        //{
        //    IStdMat2 myMaterial = global.NewDefaultStdMat;

        //    myMaterial.Name = material.MaterialName;
        //    myMaterial.TwoSided = true;
        //    myMaterial.LockAmbDiffTex(false);



        //    return myMaterial;
        //}

        protected void UpdateMaterial(IStdMat2 mat, string property, string value)
        {
            switch (property)
            {
                /*
                case    "Ambient Color"     : mat.SetAmbient(toColor(value), 0); break;
                case	"Ambient Color Map"	: myMaterial.ambientMap					= toMap myValue
                case	"Ambient Strength"	: myMaterial.ambientMapAmount 		= toPercent myValue
                case	"Bump Strength Map"	: myMaterial.bumpMap 						= toMap myValue
                case	"Color Map"			: (getDiffuseMap myMaterial).map1 	= toMap myValue
                case	"Diffuse Color"		: (getDiffuseMap myMaterial).color2 	= toColour myValue
                case	"Diffuse Strength"	: myMaterial.diffuseMapAmount 			= toPercent myValue
                case	"Opacity Map"		: myMaterial.opacityMap 					= toMap myValue
                case	"Opacity Strength"	: myMaterial.opacityMapAmount			= toPercent myValue
                case	"Specular Color"	: myMaterial.specular 						= toColour myValue
                case	"Specular Color Map": myMaterial.specularMap					= toMap myValue
                case	"Specular Strength"	: myMaterial.specularLevel 				= toPercent myValue
                case	"Glossiness"	    :
                */
            }
        }

        
        //IColor toColor(string value)
        //{
        //    var components = value.Split(' ');
        //    return global.Color.Create(float.Parse(components[1]), float.Parse(components[2]), float.Parse(components[3]));
        //}
        
    }
}
