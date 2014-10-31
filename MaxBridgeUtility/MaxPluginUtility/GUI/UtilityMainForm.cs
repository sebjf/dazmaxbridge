using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Autodesk.Max;

namespace MaxManagedBridge
{
    public partial class UtilityMainForm : MaxCustomControls.MaxForm
    {
        protected MaxPlugin Plugin;
        protected List<MySceneViewModel> scenesView = new List<MySceneViewModel>();

        public UtilityMainForm(MaxBridgeUtility parent)
        {
            InitializeComponent();

            Plugin = parent.Plugin;

            FormClosing += new FormClosingEventHandler(UtilityMainForm_FormClosing);

            rebuildMaterialsCheckbox.DataBindings.Add(new Binding("Checked", Plugin, "RebuildMaterials"));
            removeTransparentFacesCheckbox.DataBindings.Add(new Binding("Checked", Plugin, "RemoveTransparentFaces"));

            Plugin.ProgressCallback = Bridge_ProgressChanged;

            Click += new EventHandler(UtilityMainForm_Click);
            sceneListbox.SelectedValueChanged += new EventHandler(sceneListbox_SelectedValueChanged);
            sceneListbox.DisplayMember = "Label";

            materialSelectDropDown.Items.AddRange(Plugin.AvailableMaterialCreators);
            materialSelectDropDown.DisplayMember = "MaterialName";

            materialSelectDropDown.SelectedIndex = 0;

        }

        void UtilityMainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                Hide();
                e.Cancel = true;
            }
        }

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

        private void refreshButton_Click(object sender, EventArgs e)
        {
            Log.Add("Refresh list clicked", LogLevel.Debug);

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
            Log.Add("(updateButton_Click()) Update meshes clicked", LogLevel.Debug);

            foreach (var update in GetSelectedItemsUpdates)
            {
                Plugin.UpdateMeshes(update);
            }
        }

        class ControlCache
        {
            public TableLayoutPanel controlsTable;
        }

        private void createControlsForMaterialCreator(IMaterialCreator creator)
        {
            if (creator.GuiControlCache is ControlCache)
            {
                return;
            }

            var properties = creator.GetType().GetProperties().Where( property => property.IsDefined(typeof(GuiPropertyAttribute), true) ).ToList();

            // http://stackoverflow.com/questions/721669/winforms-variable-number-of-dynamic-textbox-controls
            // http://stackoverflow.com/questions/1142873/winforms-tablelayoutpanel-adding-rows-programatically

            TableLayoutPanel panel = new TableLayoutPanel();
            panel.Dock = DockStyle.Fill;
            panel.AutoScroll = true;
            panel.AutoScrollMargin = new Size(1, 1);
            panel.AutoScrollMinSize = new Size(1, 1);

            panel.ColumnCount = 2;
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            panel.RowCount = properties.Count + 1;

            for(int i = 0; i < properties.Count; i++)
            {
                GuiPropertyAttribute attribute = properties[i].GetCustomAttributes(typeof(GuiPropertyAttribute), true).FirstOrDefault() as GuiPropertyAttribute;
                string dataMember = properties[i].Name;

                panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 25));

                Label label = new Label();
                label.Text = attribute.DisplayName;
                label.Margin = new Padding(0);
                label.Dock = DockStyle.Fill;
                label.TextAlign = ContentAlignment.MiddleLeft;
                panel.Controls.Add(label, 0, i);

                switch (attribute.ControlType)
                {
                    case GuiPropertyAttribute.ControlTypeEnum.Textbox:
                        TextBox textbox = new TextBox();
                        textbox.Margin = new Padding(0);
                        textbox.Dock = DockStyle.Fill;
                        textbox.DataBindings.Add(new Binding("Text", creator, dataMember));
                        panel.Controls.Add(textbox, 1, i);
                        break;

                    case GuiPropertyAttribute.ControlTypeEnum.Checkbox:
                        CheckBox checkbox = new CheckBox();
                        checkbox.Margin = new Padding(0);
                        checkbox.Dock = DockStyle.Fill;
                        checkbox.DataBindings.Add(new Binding("Checked", creator, dataMember));
                        panel.Controls.Add(checkbox, 1, i);
                        break;

                    case GuiPropertyAttribute.ControlTypeEnum.MaterialTemplateDropdown:
                        ComboBox combobox = new ComboBox();
                        combobox.DisplayMember = "Name";
                        combobox.DataSource = new BindingSource(Plugin.Templates, null);
                        combobox.DropDownStyle = ComboBoxStyle.DropDownList;
                        combobox.Dock = DockStyle.Fill;
                        combobox.Margin = new Padding(0);
                        combobox.DataBindings.Add(new Binding("SelectedItem", creator, dataMember, true, DataSourceUpdateMode.OnPropertyChanged));
                        panel.Controls.Add(combobox, 1, i);
                        break;
                }
            }

            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            creator.GuiControlCache = new ControlCache() { controlsTable = panel };

        }

        private void materialSelectDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            IMaterialCreator materialCreator = (sender as ComboBox).SelectedItem as IMaterialCreator;

            Plugin.MaterialCreator = materialCreator;
               
            createControlsForMaterialCreator(materialCreator);
       
            materialControlsPanel.Controls.Clear();
            materialControlsPanel.Controls.Add((materialCreator.GuiControlCache as ControlCache).controlsTable);
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