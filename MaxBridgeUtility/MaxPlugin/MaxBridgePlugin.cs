using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Max;

namespace MaxManagedBridge
{
    public partial class MaxPlugin : MaxBridge
    {
        public IINode MakeMeshNode(MyMesh myMesh)
        {
            IINode myEntity = GlobalInterface.Instance.COREInterface.CreateObjectNode(MakeMesh(myMesh));
            myEntity.Name = myMesh.Name;
            return myEntity;

            //ManagedServices.MaxscriptSDK. //http://lonerobot.net/?p=472
        }

        public ITriObject MakeMesh(MyMesh myMesh)
        {
            ITriObject obj = GlobalInterface.Instance.CreateNewTriObject();
            AddMeshData(obj.Mesh, myMesh);
            return obj;
        }

        protected Dictionary<string, ITriObject> LoadedObjects = new Dictionary<string, ITriObject>();
    }
}
