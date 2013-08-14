using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Max;

namespace MaxManagedBridge
{
    public partial class MaxPlugin : MaxBridge
    {
        public bool SetPositionVertices(IMesh maxMesh, MyMesh myMesh)
        {
            bool countChanged = false;

            if (maxMesh.NumVerts != myMesh.NumVertices)
            {
                maxMesh.SetNumVerts(myMesh.NumVertices, false, false);
                countChanged = true;
            }

            for (int i = 0; i < myMesh.NumVertices; i++)
            {
                maxMesh.SetVert(i, myMesh.Vertices[(i * 3) + 0], myMesh.Vertices[(i * 3) + 1], myMesh.Vertices[(i * 3) + 2]);
            }

            return countChanged;
        }

        public bool SetTextureVertices(IMesh maxMesh, MyMesh myMesh)
        {
            bool countChanged = false;

            if (maxMesh.NumTVerts != myMesh.NumTextureCoordinates)
            {
                maxMesh.SetNumTVerts(myMesh.NumTextureCoordinates, false);
                countChanged = true;
            }

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

            return countChanged;
        }

        public bool SetFaces(IMesh maxMesh, MyMesh myMesh)
        {
            bool countChanged = false;

            TriangulateFaces(myMesh);

            if (maxMesh.NumFaces != myMesh.TriangulatedFaces.Length)
            {
                maxMesh.SetNumFaces(myMesh.TriangulatedFaces.Length, false, false);
                maxMesh.SetNumTVFaces(myMesh.TriangulatedFaces.Length, false, 0);
                countChanged = true;
            }

            for (int i = 0; i < myMesh.TriangulatedFaces.Length; i++)
            {
                Face myFace = myMesh.TriangulatedFaces[i];
                maxMesh.Faces[i].SetVerts(myFace.PositionVertex1, myFace.PositionVertex2, myFace.PositionVertex3);
                maxMesh.TvFace[i].SetTVerts(myFace.TextureVertex1, myFace.TextureVertex2, myFace.TextureVertex3);
                maxMesh.Faces[i].MatID = (ushort)myFace.MaterialId;
                maxMesh.Faces[i].SetEdgeVisFlags(EdgeVisibility.Vis, EdgeVisibility.Vis, EdgeVisibility.Vis);
            }

            return countChanged;
        }

        public void SmoothMesh(IMesh maxMesh, MyMesh myMesh)
        {
            //todo: get angle from material for smoothing
            maxMesh.AutoSmooth((float)DegreeToRadian(30.0), false, true);
        }

        public void UpdateMesh(IMesh maxMesh, MyMesh myMesh)
        {
            bool countChanged = false;

            countChanged = SetPositionVertices(maxMesh, myMesh); //if the mesh is new or has been significantly altered, recreate the whole topology
            if (countChanged)
            {
                SetTextureVertices(maxMesh, myMesh);
                SetFaces(maxMesh, myMesh);

                maxMesh.EnableEdgeList(1);
                maxMesh.InvalidateTopologyCache();
            }

            SmoothMesh(maxMesh, myMesh);

            maxMesh.InvalidateGeomCache();
        }
        
    }
}
