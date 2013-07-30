using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MaxBridgeLib;
using Autodesk.Max;
using Autodesk.Max.Plugins;

namespace MaxBridgePlugin
{
    public partial class MaxBridgeImporterPlugin
    {
        protected void Create(MaxScene scene)
        {
            foreach (MaxMesh mesh in scene.Items)
            {
                IINode myEntity = global.COREInterface.CreateObjectNode(MakeMesh(mesh));
                myEntity.Mtl = MakeMultiMaterial(mesh);
            }
        }

        protected ITriObject MakeMesh(MaxMesh mesh)
        {
            ITriObject obj = global.TriObject.Create();

            IMesh myMesh = obj.Mesh;

       
            myMesh.SetNumVerts(mesh.NumVertices, false, false);
            for (int i = 0; i < mesh.NumVertices; i++){
                myMesh.SetVert(i, mesh.Vertices[(i * 3) + 0], mesh.Vertices[(i * 3) + 1], mesh.Vertices[(i * 3) + 2]);
            }


            myMesh.SetNumTVerts(mesh.NumTextureCoordinates, false);
            int elementsPerVertex = mesh.TextureCoordinates.Count / mesh.NumTextureCoordinates;
            switch (elementsPerVertex)
            {
                case 2:
                    for (int i = 0; i < mesh.NumTextureCoordinates; i++){
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

            MaxBridge.TriangulateFaces(mesh);

            myMesh.SetNumFaces(mesh.TriangulatedFaces.Length,false,false);
            myMesh.SetNumTVFaces(mesh.TriangulatedFaces.Length, false, 0);
            for(int i = 0; i < mesh.TriangulatedFaces.Length; i++)
            {
                Face myFace = mesh.TriangulatedFaces[i];
                myMesh.Faces[i].SetVerts(myFace.PositionVertex1, myFace.PositionVertex2, myFace.PositionVertex3);
                myMesh.TvFace[i].SetTVerts(myFace.TextureVertex1, myFace.TextureVertex2, myFace.TextureVertex3);
                myMesh.Faces[i].MatID = (ushort)myFace.MaterialId;
            }

            myMesh.InvalidateGeomCache();
            myMesh.InvalidateTopologyCache();

            return obj;
        }

        protected IMultiMtl MakeMultiMaterial(MaxMesh mesh)
        {
            IMultiMtl myMaterial = global.NewDefaultMultiMtl;

            myMaterial.SetNumSubMtls(mesh.Materials.Keys.ToArray().Max() + 1);

            foreach(int i in mesh.Materials.Keys.ToArray())
            {
                myMaterial.SetSubMtl(i, MakeStandardMaterial(mesh.Materials[i]));
            }

            return myMaterial;
        }

        protected IMtl MakeStandardMaterial(Material material)
        {
            IStdMat2 myMaterial = global.NewDefaultStdMat;

            myMaterial.Name = material.MaterialName;
            myMaterial.TwoSided = true;
            myMaterial.LockAmbDiffTex(false);



            return myMaterial;
        }

        protected void UpdateMaterial(IStdMat2 mat, string property, string value)
        {


            switch(property)
            {
                case	"Ambient Color"		: mat.SetAmbient(toColor(value),0); break;
                /*
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

        IColor toColor(string value)
        {
            var components = value.Split(' ');
            return global.Color.Create(float.Parse(components[1]), float.Parse(components[2]), float.Parse(components[3]));
        }
    }
}
