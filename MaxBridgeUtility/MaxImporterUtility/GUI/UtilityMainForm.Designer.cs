using ProgressBarSample;
namespace MaxManagedBridge
{
    partial class UtilityMainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.connect_button = new System.Windows.Forms.Button();
            this.scene_explorer_listbox = new System.Windows.Forms.ListBox();
            this.button1 = new System.Windows.Forms.Button();
            this.progressBar1 = new ProgressBarSample.CustomProgressBar();
            this.SuspendLayout();
            // 
            // connect_button
            // 
            this.connect_button.ForeColor = System.Drawing.Color.Black;
            this.connect_button.Location = new System.Drawing.Point(293, 12);
            this.connect_button.Name = "connect_button";
            this.connect_button.Size = new System.Drawing.Size(87, 28);
            this.connect_button.TabIndex = 0;
            this.connect_button.Text = "Refresh List";
            this.connect_button.UseVisualStyleBackColor = true;
            this.connect_button.Click += new System.EventHandler(this.connect_button_Click);
            // 
            // scene_explorer_listbox
            // 
            this.scene_explorer_listbox.FormattingEnabled = true;
            this.scene_explorer_listbox.Location = new System.Drawing.Point(12, 12);
            this.scene_explorer_listbox.Name = "scene_explorer_listbox";
            this.scene_explorer_listbox.Size = new System.Drawing.Size(275, 277);
            this.scene_explorer_listbox.TabIndex = 1;
            // 
            // button1
            // 
            this.button1.ForeColor = System.Drawing.Color.Black;
            this.button1.Location = new System.Drawing.Point(294, 261);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(86, 27);
            this.button1.TabIndex = 3;
            this.button1.Text = "Update";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.CustomText = null;
            this.progressBar1.DisplayStyle = ProgressBarSample.ProgressBarDisplayText.CustomText;
            this.progressBar1.Location = new System.Drawing.Point(13, 296);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(368, 23);
            this.progressBar1.TabIndex = 2;
            // 
            // UtilityMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(393, 327);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.scene_explorer_listbox);
            this.Controls.Add(this.connect_button);
            this.Name = "UtilityMainForm";
            this.Text = "UI";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button connect_button;
        private System.Windows.Forms.ListBox scene_explorer_listbox;
        private CustomProgressBar progressBar1;
        private System.Windows.Forms.Button button1;
    }
}