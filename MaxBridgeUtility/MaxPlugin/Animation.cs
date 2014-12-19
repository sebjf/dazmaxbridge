using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Max;
using System.Runtime.InteropServices;
using System.IO;

namespace MaxManagedBridge
{
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct PC2Header
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=12)]
        public byte[] cacheSignature;   // Will be 'POINTCACHE2' followed by a trailing null character.
        public int fileVersion;         // Currently 1

        public int numPoints;           // Number of points per sample
        public float startFrame;        // In Frames
        public float sampleRate;        // In Frames
        public int numSamples;          // Defines how many samples are stored in the file.

        public static PC2Header Create()
        {
            PC2Header header = new PC2Header();
            header.cacheSignature = System.Text.Encoding.ASCII.GetBytes("POINTCACHE2");
            header.fileVersion = 1;
            return header;
        }

        public byte[] getBytes()
        {
            int size = Marshal.SizeOf(this);
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(this, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            return arr;
        }
    }

    public partial class MaxPlugin : MaxBridge
    {
        public void UpdateMeshAnimation(IINode node, MyMesh mesh)
        {
            switch (mesh.AnimationType)
            {
                case AnimationType.None:
                    break;
                case AnimationType.Keyframes:
                    UpdateMeshAnimation((node.ObjectRef as ITriObject), mesh.Keyframes);
                    break;
                case AnimationType.PointCache:
                    UpdateMeshAnimation_PointCache(node, mesh.Keyframes);
                    break;
            }
        }

        public void UpdateMeshAnimation_PointCache(IINode maxObject, IList<MyMeshKeyframe> keyframes)
        {

        }

        public unsafe void CreatePC2(IList<MyMeshKeyframe> keyframes, Stream stream)
        {
            PC2Header header = PC2Header.Create();

            float ticksToFrames = ((float)Autodesk.Max.GlobalInterface.Instance.FrameRate / 4800f);     // equivalent to (1 / ( ticks per second / frames per second ))

            header.numPoints = keyframes[0].VertexPositions.Count / 3;
            header.numSamples = keyframes.Count;
            header.startFrame = keyframes[0].Time * ticksToFrames; 
            header.sampleRate = 1;

            if (keyframes.Count > 1)
            {
                float dt = keyframes[1].Time - keyframes[0].Time;
                header.sampleRate = dt * ticksToFrames;
            }

            BinaryWriter writer = new BinaryWriter(stream);
            
            writer.Write(header.getBytes());
            foreach (var f in keyframes)
            {
                foreach (var k in f.VertexPositions)    // http://stackoverflow.com/questions/619041/what-is-the-fastest-way-to-convert-a-float-to-a-byte
                {
                    writer.Write(k);
                }
            }            
        }

        public void UpdateMeshAnimation(ITriObject maxObject, IList<MyMeshKeyframe> keyframes)
        {
            if (keyframes.Count <= 0)
            {
                return;
            }

            Autodesk.Max.Wrappers.MasterPointControl masterPointController = null;
            for (int i = 0; i < maxObject.NumSubs; i++)
            {
                /* Find the master point controller */
                IAnimatable anim = maxObject.SubAnim(i);
                if (anim is Autodesk.Max.Wrappers.MasterPointControl)
                {
                    masterPointController = anim as Autodesk.Max.Wrappers.MasterPointControl;
                    break;
                }
            }

            if (masterPointController == null)
            {
                Log.Add("Could not find MasterPointController", LogLevel.Error);
                return;
            }

            int numberOfVerts = maxObject.Mesh.NumVerts;

            masterPointController.SetNumSubControllers(numberOfVerts, false);


            for (int v = 0; v < numberOfVerts; v++)
            {
                IControl controller = masterPointController.GetSubController(v);

                if (controller == null)
                {
                    /* no key controller -> add a new one */
                    IClassDesc defaultPoint3ControllerClass = gi.GetDefaultController(Autodesk.Max.SClass_ID.CtrlPoint3);
                    IControl newKeyController = gi.COREInterface.CreateInstance(SClass_ID.CtrlPoint3, defaultPoint3ControllerClass.ClassID) as IControl;
                    masterPointController.SetSubController(v, newKeyController);
                    controller = masterPointController.GetSubController(v);

                    /* When creating a new controller, assign it as a subanimatable as well as a subcontroller */
                    masterPointController.AssignController(controller, v);
                }

                Autodesk.Max.Wrappers.IKeyControl vertexKeyController = controller.GetInterface(InterfaceID.Keycontrol) as Autodesk.Max.Wrappers.IKeyControl;

                vertexKeyController.NumKeys = keyframes.Count;

                for (int k = 0; k < keyframes.Count; k++)
                {
                    IIBezPoint3Key key = gi.IBezPoint3Key.Create();

                    key.Time = (int)keyframes[k].Time;
                    key.InLength.Set(0.5f, 0.5f, 0.5f);
                    key.OutLength.Set(0.5f, 0.5f, 0.5f);
                    key.Intan.Set(0, 0, 0);
                    key.Outtan.Set(0, 0, 0);

                    key.Val.Set(
                        keyframes[k].VertexPositions[(v * 3) + 0],
                        keyframes[k].VertexPositions[(v * 3) + 1],
                        keyframes[k].VertexPositions[(v * 3) + 2]);

                    vertexKeyController.SetKey(k, key);
                }

            }
        }

    }
}
