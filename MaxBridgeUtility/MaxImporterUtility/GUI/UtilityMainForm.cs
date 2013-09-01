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
            this.Plugin = parent.Plugin;

            this.Plugin.ProgressChanged += new MaxPlugin.ProgressUpdateHandler(Bridge_ProgressChanged);
        }

        void Bridge_ProgressChanged(float progress, string message)
        {
            progressBar1.Value = (int)(progress * 100.0f);
            progressBar1.CustomText = message;
        }

        protected MaxBridgeUtility Utility;
        protected MaxPlugin Plugin;

        private void connect_button_Click(object sender, EventArgs e)
        {
            MySceneInformation sceneItems = Plugin.DazClient.GetSceneInformation();
            scene_explorer_listbox.Items.Clear();
            scene_explorer_listbox.Items.AddRange(sceneItems.TopLevelItemNames.ToArray());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<string> itemNames = new List<string>();
            foreach (var item in scene_explorer_listbox.SelectedItems)
            {
                itemNames.Add(item.ToString());
            }
            Plugin.UpdateMeshes(itemNames);
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