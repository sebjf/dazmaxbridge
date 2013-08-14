using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MaxManagedBridge
{
    public partial class UtilityMainForm : MaxCustomControls.MaxForm
    {
        public UtilityMainForm(MaxBridgeUtility parent)
        {
            InitializeComponent();
            this.Utility = parent;
            this.Bridge = parent.Bridge;
        }

        protected MaxBridgeUtility Utility;
        protected MaxPlugin Bridge;

        private void connect_button_Click(object sender, EventArgs e)
        {
            Bridge.LoadFromFile(@"E:\Daz3D\Scripting\Scratch.dazmaxbridge");

            for (int i = 0; i < Bridge.Scene.Items.Count; i++ )
            {
                scene_explorer_listbox.Items.Add(Bridge.Scene.Items[i]);
            }
        }

    }
}
