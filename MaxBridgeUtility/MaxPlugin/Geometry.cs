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
            Marshal.Copy(myMesh.Vertices.ToArray(), 0, p3.NativePointer, myMesh.NumVertices * 3);

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

            IntPtr p3h = maxMesh.GetTVertPtr(0).NativePointer;
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
                    throw new NotImplementedException(("Unable to handle texture coordinates with " + elementsPerVertex + " elements."));
            }

            return countChanged;
        }

        unsafe public bool SetFaces(IMesh maxMesh, MyMesh myMesh)
        {
            bool countChanged = false;

            TriangulateFaces(myMesh);

            if (maxMesh.NumFaces != myMesh.TriangulatedFaces.Length)
            {
                maxMesh.SetNumFaces(myMesh.TriangulatedFaces.Length, false, false);
                maxMesh.SetNumTVFaces(myMesh.TriangulatedFaces.Length, false, 0);
                countChanged = true;
            }

            /* Get the default flags value */

            IFace referenceFace = gi.Face.Create();
            referenceFace.SetEdgeVisFlags(EdgeVisibility.Vis, EdgeVisibility.Vis, EdgeVisibility.Vis);
            Face referenceMaxFace = *(Face*)referenceFace.NativePointer.ToPointer();
            UInt32 referenceFlags = referenceMaxFace.flags;

            /* Create the faces that define the surface of the mesh */

            Face* faces = (Face*)maxMesh.Faces[0].NativePointer.ToPointer();
            TVFace* tvfaces = (TVFace*)maxMesh.TvFace[0].NativePointer.ToPointer();

            for (int i = 0; i < myMesh.TriangulatedFaces.Length; i++)
            {
                MyFace myFace = myMesh.TriangulatedFaces[i];

                faces[i].v.v1 = (UInt32)myFace.PositionVertex1;
                faces[i].v.v2 = (UInt32)myFace.PositionVertex2;
                faces[i].v.v3 = (UInt32)myFace.PositionVertex3;
                faces[i].flags = (UInt32)((ushort)myFace.MaterialId << 16) | (ushort)referenceFlags;

                tvfaces[i].t1 = (UInt32)myFace.TextureVertex1;
                tvfaces[i].t2 = (UInt32)myFace.TextureVertex2;
                tvfaces[i].t3 = (UInt32)myFace.TextureVertex3;

            };
            
            return countChanged;
        }

        public void SmoothMesh(IMesh maxMesh, MyMesh myMesh)
        {
            //todo: get angle from material for smoothing
            maxMesh.AutoSmooth((float)DegreeToRadian(30.0), false, true);
        }

        /* See this Max documentation page on how to build a mesh: http://docs.autodesk.com/3DSMAX/16/ENU/3ds-Max-SDK-Programmer-Guide/index.html?url=files/GUID-714885D1-B3D4-4F64-8EE5-0B22B689C95B.htm,topicNumber=d30e53726 */

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
