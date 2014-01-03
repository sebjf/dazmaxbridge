using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MaxManagedBridge
{
    public class MySceneItemViewModel
    {
        public MySceneViewModel SceneView;
        public string ItemName;
        public string Label { get; private set; }

        public override string  ToString()
        {
 	        return Label;
        }

        public MySceneItemViewModel(MySceneViewModel sceneview, string name)
        {
            this.SceneView = sceneview;
            this.ItemName = name;

            Label = string.Format("{0}: {1}", sceneview.Client.DazInstanceName, name);
        }
    }

    public class MySceneViewModel
    {
        public MySceneViewModel(SceneClient client)
        {
            this.Client = client;

            MySceneInformation sceneInfo = client.GetSceneInformation();
            foreach (var name in sceneInfo.TopLevelItemNames)
            {
                Items.Add(new MySceneItemViewModel(this, name));
            }
        }

        public SceneClient Client;
        public List<MySceneItemViewModel> Items = new List<MySceneItemViewModel>();
        public List<string> SelectedItems = new List<string>();
    }
}
