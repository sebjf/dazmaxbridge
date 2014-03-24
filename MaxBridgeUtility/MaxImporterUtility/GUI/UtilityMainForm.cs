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

            rebuildMaterialsCheckbox.DataBindings.Add(new Binding("Checked", this.Plugin, "RebuildMaterials"));
            removeTransparentFacesCheckbox.DataBindings.Add(new Binding("Checked", this.Plugin, "RemoveTransparentFaces"));

            //this.Plugin.ProgressChanged += new MaxPlugin.ProgressUpdateHandler(Bridge_ProgressChanged);
            this.Plugin.ProgressCallback = Bridge_ProgressChanged;

            this.Click += new EventHandler(UtilityMainForm_Click);
            sceneListbox.SelectedValueChanged += new EventHandler(sceneListbox_SelectedValueChanged);
            sceneListbox.DisplayMember = "Label";

            this.materialSelectDropDown.Items.AddRange(Plugin.AvailableMaterials);
            this.materialSelectDropDown.DisplayMember = "MaterialName";
            
        }

        protected override void OnLoad(EventArgs e)
        {
            /* The selection must be changed after the form loads completely, otherwise the bindings for the material options will be added but not evaluated until after it loads - that is - outside
             * the try/catch statement which marks them as enabled or disabled. */
            this.materialSelectDropDown.SelectedIndex = 0;
            base.OnLoad(e);
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

        private IEnumerable<MyScene> GetSelectedItemsUpdates
        {
            get
            {
                if (sceneListbox.SelectedItems.Count > 0)
                {
                    foreach (var sceneview in scenesView)
                    {
                        if (sceneview.SelectedItems.Count > 0)
                        {
                            yield return sceneview.Client.GetScene(sceneview.SelectedItems);
                        }
                    }
                }
                else
                {
                    foreach (var sceneview in scenesView)
                    {
                        yield return sceneview.Client.GetScene(new List<string>());
                    }
                }
            }
        }

        private void updateButton_Click(object sender, EventArgs e)
        {
            Log.Add("[m] (updateButton_Click()) Update meshes clicked");

            foreach (var update in GetSelectedItemsUpdates)
            {
                Plugin.UpdateMeshes(update);
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

        private class BindingCache
        {
            public BindingCache(Control control, Binding binding)
            {
                this.control = control;
                this.binding = binding;
                this.enabled = true;
            }

            private Binding binding;
            private bool enabled;
            private Control control;

            public void RemakeBinding()
            {
                control.DataBindings.Clear();

                if (enabled)
                {
                    try
                    {
                        control.DataBindings.Add(binding);
                    }
                    catch
                    {
                        enabled = false;
                    }
                }

                control.Enabled = enabled;

                if (control.Enabled == false)
                {
                    if (control is TextBox) { control.ResetText(); }
                    if (control is CheckBox) { (control as CheckBox).Checked = false; }
                }

            }
        }

        private void materialSelectDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            IMaterialCreationOptions MaterialOptions = (sender as ComboBox).SelectedItem as IMaterialCreationOptions;

            Plugin.MaterialOptions = MaterialOptions;

            /* We create and use the array of cached bindings in this function only so we can use hardcoded indices safely */

            if((MaterialOptions.BindingInfo as BindingCache[]) == null){
                MaterialOptions.BindingInfo = new BindingCache[5];

                /* When this material is first selected, we attempt to bind all the possible UI controls. If the binding fails (because there is no appropriate property for that control)
                 * the binding is disabled in the BindingCache object from then on, and the control is disabled. This way, we only throw an exception once at the first use of the material. */

                (MaterialOptions.BindingInfo as BindingCache[])[0] = new BindingCache(glossinessScalarTextBox, new Binding("Text", MaterialOptions, "GlossScalar"));
                (MaterialOptions.BindingInfo as BindingCache[])[1] = new BindingCache(bumpScalarTextBox, new Binding("Text", MaterialOptions, "BumpScalar"));
                (MaterialOptions.BindingInfo as BindingCache[])[2] = new BindingCache(disableMapFilteringCheckbox, new Binding("Checked", MaterialOptions, "MapFilteringDisable"));
                (MaterialOptions.BindingInfo as BindingCache[])[3] = new BindingCache(ambientOcclusionEnableCheckbox, new Binding("Checked", MaterialOptions, "AOEnable"));
                (MaterialOptions.BindingInfo as BindingCache[])[4] = new BindingCache(ambientOcclusionDistanceTextBox, new Binding("Text", MaterialOptions, "AODistance"));
            }

            BindingCache[] bindingInfo = MaterialOptions.BindingInfo as BindingCache[];

            foreach (BindingCache binding in bindingInfo)
            {
                binding.RemakeBinding();
            }
        }

        private void getMaterialProperties_button_Click(object sender, EventArgs e)
        {
            string materialContents = "Press CTRL + C with this window in focus to copy the content.\n\n";
                       
            foreach (var update in GetSelectedItemsUpdates)
            {
                materialContents += Plugin.PrintMaterialProperties(update);
            }

            EditableMessageBox messagebox = new EditableMessageBox(materialContents, "Material Properties in Update from Daz");
            messagebox.Show();
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