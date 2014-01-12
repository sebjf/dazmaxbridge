using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace MaxManagedBridge
{
    public partial class UtilityMainForm : MaxCustomControls.MaxForm
    {
        public UtilityMainForm(MaxBridgeUtility parent)
        {
            InitializeComponent();

            defaultSize = this.Size;

            this.Utility = parent;
            this.Plugin = parent.Plugin;

            //this.Plugin.ProgressChanged += new MaxPlugin.ProgressUpdateHandler(Bridge_ProgressChanged);
            this.Plugin.ProgressCallback = Bridge_ProgressChanged;

            this.Click += new EventHandler(UtilityMainForm_Click);
            sceneListbox.SelectedValueChanged += new EventHandler(sceneListbox_SelectedValueChanged);
            sceneListbox.DisplayMember = "Label";

            this.bumpScalarTextBox.Text = this.Plugin.BumpScalar.ToString();
            this.glossinessScalarTextBox.Text = this.Plugin.GlossinessScalar.ToString();

            this.bumpScalarTextBox.KeyPress += new KeyPressEventHandler(bumpScalarTextBox_KeyPress);
            this.glossinessScalarTextBox.KeyPress += new KeyPressEventHandler(glossinessScalarTextBox_KeyPress);

            this.materialSelectDropDown.Items.AddRange(Plugin.AvailableMaterials);
            this.materialSelectDropDown.SelectedIndex = 0;
        }

        void glossinessScalarTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                Plugin.GlossinessScalar = GetValidatedUpdateFromTextbox(sender as TextBox, Plugin.GlossinessScalar);
            }
        }

        void bumpScalarTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                Plugin.BumpScalar = GetValidatedUpdateFromTextbox(sender as TextBox, Plugin.BumpScalar);
            }
        }

        float GetValidatedUpdateFromTextbox(TextBox textbox, float original)
        {
            float value;
            if (!float.TryParse(textbox.Text, out value))
            {
                value = original;
            }

            textbox.Text = value.ToString();
            return value;
        }

        private Size defaultSize;

        void sceneListbox_SelectedValueChanged(object sender, EventArgs e)
        {
            if (sceneListbox.SelectedItems.Count > 0)
            {
                updateButton.Text = "Update Selected";
            }
            else
            {
                updateButton.Text = "Update All";
            }

            UpdateAllSelectedItems();
        }

        void UpdateAllSelectedItems()
        {
            foreach(MySceneViewModel sceneview in scenesView)
            {
                sceneview.SelectedItems.Clear();
            }

            foreach (var o in sceneListbox.SelectedItems)
            {
                MySceneItemViewModel item = o as MySceneItemViewModel;
                item.SceneView.SelectedItems.Add(item.ItemName);
            }
        }

        void UtilityMainForm_Click(object sender, EventArgs e)
        {
            sceneListbox.ClearSelected();
        }

        void Bridge_ProgressChanged(float progress, string message)
        {
            progressBar1.Value = (int)(progress * 100.0f);
            progressBar1.CustomText = message;
            progressBar1.Refresh();
        }

        protected MaxBridgeUtility Utility;
        protected MaxPlugin Plugin;

        protected List<MySceneViewModel> scenesView = new List<MySceneViewModel>();

        private void refreshButton_Click(object sender, EventArgs e)
        {
            Log.Add("[m] Refresh list clicked");

            scenesView.Clear();
            sceneListbox.Items.Clear();

            Plugin.DazClientManager.FindAllInstances();
            foreach (var client in Plugin.DazClientManager.Instances)
            {
                MySceneViewModel sceneView = new MySceneViewModel(client);
                scenesView.Add(sceneView);
                sceneListbox.Items.AddRange(sceneView.Items.ToArray());
            }
        }

        private void updateButton_Click(object sender, EventArgs e)
        {
            Log.Add("[m] (updateButton_Click()) Update meshes clicked");

            foreach (var sceneview in scenesView)
            {
                if (sceneListbox.SelectedItems.Count > 0)
                {
                    if (sceneview.SelectedItems.Count > 0)
                    {
                        Plugin.UpdateMeshes(sceneview.Client.GetScene(sceneview.SelectedItems));
                    }
                }
                else
                {
                    Plugin.UpdateMeshes(sceneview.Client.GetScene(new List<string>()));
                }
            }

        }

        private bool optionsVisible = false;

        private void optionsButton_Click(object sender, EventArgs e)
        {
            if (optionsVisible)
            {
                this.Size = defaultSize;
            }
            else
            {
                this.Size = new Size(600, defaultSize.Height);
            }
            optionsVisible = !optionsVisible;
        }

        private void materialSelectDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            Plugin.SelectedMaterial = (sender as ComboBox).SelectedIndex;
        }

        private void getMaterialProperties_button_Click(object sender, EventArgs e)
        {
            

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "TXT|*.txt";
            DialogResult result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                //System.IO.File.WriteAllText(dlg.FileName, Plugin.PrintMaterialProperties(selectedItemNames));
                throw new NotImplementedException();
            }
        }

    }
}

namespace ProgressBarSample
{

    public enum ProgressBarDisplayText
    {
        Percentage,
        CustomText
    }

    //http://stackoverflow.com/questions/3529928/how-do-i-put-text-on-progressbar
    class CustomProgressBar : ProgressBar
    {
        //Property to set to decide whether to print a % or Text
        public ProgressBarDisplayText DisplayStyle { get; set; }

        //Property to hold the custom text
        public String CustomText { get; set; }

        public CustomProgressBar()
        {
            // Modify the ControlStyles flags
            //http://msdn.microsoft.com/en-us/library/system.windows.forms.controlstyles.aspx
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            DisplayStyle = ProgressBarDisplayText.CustomText;
            Maximum = 100;
            Minimum = 0;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rect = ClientRectangle;
            Graphics g = e.Graphics;

            ProgressBarRenderer.DrawHorizontalBar(g, rect);
            rect.Inflate(-3, -3);
            if (Value > 0)
            {
                // As we doing this ourselves we need to draw the chunks on the progress bar
                Rectangle clip = new Rectangle(rect.X, rect.Y, (int)Math.Round(((float)Value / Maximum) * rect.Width), rect.Height);
                ProgressBarRenderer.DrawHorizontalChunks(g, clip);
            }

            // Set the Display text (Either a % amount or our custom text
            string text = DisplayStyle == ProgressBarDisplayText.Percentage ? Value.ToString() + '%' : CustomText;


            using (Font f = new Font(FontFamily.GenericSansSerif, 10))
            {
                SizeF len = g.MeasureString(text, f);
                // Calculate the location of the text (the middle of progress bar)
                Point location = new Point(Convert.ToInt32((rect.Width / 2) - (len.Width / 2)), Convert.ToInt32((rect.Height / 2) - (len.Height / 2)));
                // Draw the custom text
                g.DrawString(text, f, Brushes.Black, location);
            }
        }
    }
}