﻿using System;
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
        public MyScene Scene { get; private set; }

        public void LoadFromFile(string filename, bool triangulate = true)
        {
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(fs);

            MessagePackSerializer<MyScene> c = MessagePackSerializer.Create<MyScene>();
            Scene = c.Unpack(fs);

            reader.Close();

            if (triangulate)
            {
                foreach (MyMesh m in Scene.Items)
                {
                    TriangulateFaces(m);
                }
            }
        }

        #region Utilities

        public T[] BlockCast<T>(byte[] source)
        {
            var array = new T[source.Length / Marshal.SizeOf(typeof(T))];
            Buffer.BlockCopy(source, 0, array, 0, source.Length);
            return array;
        }

        public Face[] BlockCast(byte[] source)
        {
            int elements = source.Length / Marshal.SizeOf(typeof(Face));
            Face[] dst = new Face[elements];
            unsafe
            {
                fixed (byte* pSrc = source)
                {
                    for (int i = 0; i < dst.Length; i++)
                    {
                        dst[i] = ((Face*)pSrc)[i];
                    }
                }
            }

            return dst;
        }

        protected int[] BlockCast(Face[] faces)
        {
            int dstElements = (Marshal.SizeOf(typeof(Face)) / Marshal.SizeOf(typeof(int))) * faces.Length;
            int[] dst = new int[dstElements];

            unsafe{
                fixed( int* pDst = dst)
                {
                    for(int i = 0; i < faces.Length; i++)
                    {
                        ((Face*)pDst)[i] = faces[i];
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

        unsafe public void TriangulateFaces(MyMesh myMesh)
        {
            if (myMesh.TriangulatedFaces != null){
                return;
            }

            Face[] quadFaces = BlockCast(myMesh.Faces);

            List<Face> triangulatedFaces = new List<Face>();
            foreach (Face f in quadFaces)
            {
                Face f1;
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
                    Face f2;
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