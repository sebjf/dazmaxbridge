using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using MsgPack;
using MsgPack.Serialization;

namespace MaxManagedBridge
{
    public partial class MaxBridge
    {
        public ClientManager DazClientManager = new ClientManager();

        #region Utilities

        public MyFace[] BlockCast(byte[] source)
        {
            int elements = source.Length / Marshal.SizeOf(typeof(MyFace));
            MyFace[] dst = new MyFace[elements];
            unsafe
            {
                fixed (byte* pSrc = source)
                {
                    for (int i = 0; i < dst.Length; i++)
                    {
                        dst[i] = ((MyFace*)pSrc)[i];
                    }
                }
            }

            return dst;
        }

        public double DegreeToRadian(double angle)
        {
            return ((Math.PI * angle) / 180.0);
        }

        #endregion

        #region Mesh Processing

        unsafe public void RemoveFacesByMaterialId(MyMesh myMesh, int materialId)
        {
            TriangulateFaces(myMesh);

            List<MyFace> filteredFaces = new List<MyFace>(myMesh.TriangulatedFaces.Length);
            foreach (MyFace f in myMesh.TriangulatedFaces)
            {
                if (f.MaterialId != materialId)
                {
                    filteredFaces.Add(f);
                }
            }

            myMesh.TriangulatedFaces = filteredFaces.ToArray();
        }

        unsafe public void TriangulateFaces(MyMesh myMesh)
        {
            if (myMesh.TriangulatedFaces != null){
                return;
            }

            MyFace[] quadFaces = BlockCast(myMesh.Faces);

            List<MyFace> triangulatedFaces = new List<MyFace>();
            foreach (MyFace f in quadFaces)
            {
                MyFace f1;
                f1.PositionVertex1 = f.PositionVertex1;
                f1.PositionVertex2 = f.PositionVertex2;
                f1.PositionVertex3 = f.PositionVertex3;
                f1.PositionVertex4 = -1;
                f1.TextureVertex1 = f.TextureVertex1;
                f1.TextureVertex2 = f.TextureVertex2;
                f1.TextureVertex3 = f.TextureVertex3;
                f1.TextureVertex4 = -1;
                f1.MaterialId = f.MaterialId;

                triangulatedFaces.Add(f1);

                if (f.PositionVertex4 >= 0)
                {
                    MyFace f2;
                    f2.PositionVertex1 = f.PositionVertex1;
                    f2.PositionVertex2 = f.PositionVertex3;
                    f2.PositionVertex3 = f.PositionVertex4;
                    f2.PositionVertex4 = -1;
                    f2.TextureVertex1 = f.TextureVertex1;
                    f2.TextureVertex2 = f.TextureVertex3;
                    f2.TextureVertex3 = f.TextureVertex4;
                    f2.TextureVertex4 = -1;
                    f2.MaterialId = f.MaterialId;

                    triangulatedFaces.Add(f2);
                }
            }

            myMesh.TriangulatedFaces = triangulatedFaces.ToArray();
        }

        #endregion
    }
}
