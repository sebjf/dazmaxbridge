using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Max;

namespace MaxManagedBridge
{
    public class MaxPlugin : MaxBridge
    {
        public string PrintName(System.Int64 handle)
        {
            var node = Convert(handle);
            return (node.NodeName + " of type " + node.ClassName);
        }

        public string PrintObject(object obj)
        {
            return obj.GetType().GetType().Name;
        }

        public IAnimatable Convert(System.Int64 handle)
        {
            IAnimatable anim = Autodesk.Max.GlobalInterface.Instance.Animatable.GetAnimByHandle((UIntPtr)handle);
            return anim;
        }

        public System.Int64 Convert(IAnimatable obj)
        {
            return (System.Int64)Autodesk.Max.GlobalInterface.Instance.Animatable.GetHandleByAnim(obj);
        }

        public IINode MakeMeshNode(MyMesh myMesh)
        {
            IINode myEntity = GlobalInterface.Instance.COREInterface.CreateObjectNode(MakeMesh(myMesh));
            return myEntity;
        }

        public ITriObject MakeMesh(MyMesh myMesh)
        {
            ITriObject obj = GlobalInterface.Instance.CreateNewTriObject;
            AddMeshData(obj.Mesh, myMesh);
            return obj;
        }

        public void AddMeshData(IMesh maxMesh, MyMesh myMesh)
        {
            maxMesh.SetNumVerts(myMesh.NumVertices, false, false);
            for (int i = 0; i < myMesh.NumVertices; i++)
            {
                maxMesh.SetVert(i, myMesh.Vertices[(i * 3) + 0], myMesh.Vertices[(i * 3) + 1], myMesh.Vertices[(i * 3) + 2]);
            }

            maxMesh.SetNumTVerts(myMesh.NumTextureCoordinates, false);
            int elementsPerVertex = myMesh.TextureCoordinates.Count / myMesh.NumTextureCoordinates;
            switch (elementsPerVertex)
            {
                case 2:
                    for (int i = 0; i < myMesh.NumTextureCoordinates; i++)
                    {
                        maxMesh.SetTVert(i, myMesh.TextureCoordinates[(i * 2) + 0], myMesh.TextureCoordinates[(i * 2) + 1], 0.0f);
                    }
                    break;
                case 3:
                    for (int i = 0; i < myMesh.NumTextureCoordinates; i++)
                    {
                        maxMesh.SetTVert(i, myMesh.TextureCoordinates[(i * 3) + 0], myMesh.TextureCoordinates[(i * 3) + 1], myMesh.TextureCoordinates[(i * 3) + 2]);
                    }
                    break;
                default:
                    throw new NotImplementedException(("Unable to handle texture coordinates with " + elementsPerVertex + " coordinates."));
            }

            TriangulateFaces(myMesh);

            maxMesh.SetNumFaces(myMesh.TriangulatedFaces.Length, false, false);
            maxMesh.SetNumTVFaces(myMesh.TriangulatedFaces.Length, false, 0);
            for (int i = 0; i < myMesh.TriangulatedFaces.Length; i++)
            {
                Face myFace = myMesh.TriangulatedFaces[i];
                maxMesh.Faces[i].SetVerts(myFace.PositionVertex1, myFace.PositionVertex2, myFace.PositionVertex3);
                maxMesh.TvFace[i].SetTVerts(myFace.TextureVertex1, myFace.TextureVertex2, myFace.TextureVertex3);
                maxMesh.Faces[i].MatID = (ushort)myFace.MaterialId;
            }

            //NEED TO MAKE EDGES

            maxMesh.BuildStripsAndEdges();

            maxMesh.InvalidateEdgeList();
            maxMesh.InvalidateStrips();
            maxMesh.InvalidateGeomCache();
            maxMesh.InvalidateTopologyCache();
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
