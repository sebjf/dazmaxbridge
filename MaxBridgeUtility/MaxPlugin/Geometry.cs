using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Max;
using System.Runtime.InteropServices;

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

            IPoint3 p3 = maxMesh.GetVertPtr(0);
            Marshal.Copy(myMesh.Vertices.ToArray(), 0, p3.Handle, myMesh.NumVertices * 3);

            return countChanged;
        }

        unsafe public bool SetTextureVertices(IMesh maxMesh, MyMesh myMesh)
        {
            bool countChanged = false;

            if (maxMesh.NumTVerts != myMesh.NumTextureCoordinates)
            {
                maxMesh.SetNumTVerts(myMesh.NumTextureCoordinates, false);
                countChanged = true;
            }

            IntPtr p3h = maxMesh.GetTVertPtr(0).Handle;
            float* p3 = (float*)p3h.ToPointer();

            int elementsPerVertex = myMesh.TextureCoordinates.Count / myMesh.NumTextureCoordinates;
            switch (elementsPerVertex)
            {
                case 2:
                    for (int i = 0; i < myMesh.NumTextureCoordinates; i++)
                    {
                        p3[(i * 3) + 0] = myMesh.TextureCoordinates[(i * 2) + 0];
                        p3[(i * 3) + 1] = myMesh.TextureCoordinates[(i * 2) + 1];
                        p3[(i * 3) + 2] = 0.0f;
                    };
                    break;
                case 3:
                    Marshal.Copy(myMesh.TextureCoordinates.ToArray(), 0, p3h, myMesh.NumTextureCoordinates * 3);
                    break;
                default:
                    throw new NotImplementedException(("Unable to handle texture coordinates with " + elementsPerVertex + " coordinates."));
            }

            return countChanged;
        }

       unsafe struct MaxFace
        {
            fixed UInt32 v[3];
            UInt32 smGroup;
            UInt32 flags;
        }

        unsafe public bool SetFaces2(IMesh maxMesh, MyMesh myMesh)
        {
            bool countChanged = false;

            TriangulateFaces(myMesh);

            if (maxMesh.NumFaces != myMesh.TriangulatedFaces.Length)
            {
                maxMesh.SetNumFaces(myMesh.TriangulatedFaces.Length, false, false);
                maxMesh.SetNumTVFaces(myMesh.TriangulatedFaces.Length, false, 0);
                countChanged = true;
            }

            IFace referenceface = globalInterface.Face.Create();
            referenceface.SetEdgeVisFlags(EdgeVisibility.Vis, EdgeVisibility.Vis, EdgeVisibility.Vis);

            MaxFace template = new MaxFace();

            for (int i = 0; i < myMesh.TriangulatedFaces.Length; i++)
            {
                Face myFace = myMesh.TriangulatedFaces[i];
                maxMesh.Faces[i].SetVerts(myFace.PositionVertex1, myFace.PositionVertex2, myFace.PositionVertex3);
                maxMesh.TvFace[i].SetTVerts(myFace.TextureVertex1, myFace.TextureVertex2, myFace.TextureVertex3);
                maxMesh.Faces[i].MatID = (ushort)myFace.MaterialId;
                maxMesh.Faces[i].SetEdgeVisFlags(EdgeVisibility.Vis, EdgeVisibility.Vis, EdgeVisibility.Vis);
            };

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
            };

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

                SmoothMesh(maxMesh, myMesh);
            }

            maxMesh.InvalidateGeomCache();
            maxMesh.InvalidateTopologyCache();

            
        }
        
    }
}
