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
        public void UpdateMeshAnimation(ITriObject maxObject, IList<MyMeshKeyframe> keyframes)
        {
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
