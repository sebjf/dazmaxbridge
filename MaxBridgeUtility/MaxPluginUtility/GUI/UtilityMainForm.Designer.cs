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
            this.refreshButton = new System.Windows.Forms.Button();
            this.sceneListbox = new System.Windows.Forms.ListBox();
            this.updateButton = new System.Windows.Forms.Button();
            this.optionsGroup = new System.Windows.Forms.GroupBox();
            this.getMaterialProperties_button = new System.Windows.Forms.Button();
            this.materialControlsPanel = new System.Windows.Forms.Panel();
            this.materialSelectDropDown = new System.Windows.Forms.ComboBox();
            this.removeTransparentFacesCheckbox = new System.Windows.Forms.CheckBox();
            this.rebuildMaterialsCheckbox = new System.Windows.Forms.CheckBox();
            this.progressBar1 = new ProgressBarSample.CustomProgressBar();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.animationTypeCombo = new System.Windows.Forms.ComboBox();
            this.optionsGroup.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // refreshButton
            // 
            this.refreshButton.ForeColor = System.Drawing.Color.Black;
            this.refreshButton.Location = new System.Drawing.Point(286, 12);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(100, 28);
            this.refreshButton.TabIndex = 0;
            this.refreshButton.Text = "Refresh List";
            this.refreshButton.UseVisualStyleBackColor = true;
            this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
            // 
            // sceneListbox
            // 
            this.sceneListbox.FormattingEnabled = true;
            this.sceneListbox.Location = new System.Drawing.Point(12, 12);
            this.sceneListbox.Name = "sceneListbox";
            this.sceneListbox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.sceneListbox.Size = new System.Drawing.Size(268, 199);
            this.sceneListbox.TabIndex = 1;
            // 
            // updateButton
            // 
            this.updateButton.ForeColor = System.Drawing.Color.Black;
            this.updateButton.Location = new System.Drawing.Point(286, 184);
            this.updateButton.Name = "updateButton";
            this.updateButton.Size = new System.Drawing.Size(100, 27);
            this.updateButton.TabIndex = 3;
            this.updateButton.Text = "Update All";
            this.updateButton.UseVisualStyleBackColor = true;
            this.updateButton.Click += new System.EventHandler(this.updateButton_Click);
            // 
            // optionsGroup
            // 
            this.optionsGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.optionsGroup.Controls.Add(this.getMaterialProperties_button);
            this.optionsGroup.Controls.Add(this.materialControlsPanel);
            this.optionsGroup.Controls.Add(this.materialSelectDropDown);
            this.optionsGroup.Location = new System.Drawing.Point(13, 327);
            this.optionsGroup.Name = "optionsGroup";
            this.optionsGroup.Size = new System.Drawing.Size(377, 338);
            this.optionsGroup.TabIndex = 4;
            this.optionsGroup.TabStop = false;
            this.optionsGroup.Text = "Material Options";
            // 
            // getMaterialProperties_button
            // 
            this.getMaterialProperties_button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.getMaterialProperties_button.ForeColor = System.Drawing.Color.Black;
            this.getMaterialProperties_button.Location = new System.Drawing.Point(10, 309);
            this.getMaterialProperties_button.Name = "getMaterialProperties_button";
            this.getMaterialProperties_button.Size = new System.Drawing.Size(140, 23);
            this.getMaterialProperties_button.TabIndex = 8;
            this.getMaterialProperties_button.Text = "View Material Properties";
            this.getMaterialProperties_button.UseVisualStyleBackColor = true;
            this.getMaterialProperties_button.Click += new System.EventHandler(this.getMaterialProperties_button_Click);
            // 
            // materialControlsPanel
            // 
            this.materialControlsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.materialControlsPanel.Location = new System.Drawing.Point(7, 47);
            this.materialControlsPanel.Name = "materialControlsPanel";
            this.materialControlsPanel.Size = new System.Drawing.Size(361, 256);
            this.materialControlsPanel.TabIndex = 25;
            // 
            // materialSelectDropDown
            // 
            this.materialSelectDropDown.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.materialSelectDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.materialSelectDropDown.FormattingEnabled = true;
            this.materialSelectDropDown.Location = new System.Drawing.Point(7, 20);
            this.materialSelectDropDown.Name = "materialSelectDropDown";
            this.materialSelectDropDown.Size = new System.Drawing.Size(361, 21);
            this.materialSelectDropDown.TabIndex = 0;
            this.materialSelectDropDown.SelectedIndexChanged += new System.EventHandler(this.materialSelectDropDown_SelectedIndexChanged);
            // 
            // removeTransparentFacesCheckbox
            // 
            this.removeTransparentFacesCheckbox.AutoSize = true;
            this.removeTransparentFacesCheckbox.Location = new System.Drawing.Point(136, 246);
            this.removeTransparentFacesCheckbox.Name = "removeTransparentFacesCheckbox";
            this.removeTransparentFacesCheckbox.Size = new System.Drawing.Size(158, 17);
            this.removeTransparentFacesCheckbox.TabIndex = 17;
            this.removeTransparentFacesCheckbox.Text = "Remove Transparent Faces";
            this.removeTransparentFacesCheckbox.UseVisualStyleBackColor = true;
            // 
            // rebuildMaterialsCheckbox
            // 
            this.rebuildMaterialsCheckbox.AutoSize = true;
            this.rebuildMaterialsCheckbox.Location = new System.Drawing.Point(13, 246);
            this.rebuildMaterialsCheckbox.Name = "rebuildMaterialsCheckbox";
            this.rebuildMaterialsCheckbox.Size = new System.Drawing.Size(117, 17);
            this.rebuildMaterialsCheckbox.TabIndex = 5;
            this.rebuildMaterialsCheckbox.Text = "Re-Import Materials";
            this.rebuildMaterialsCheckbox.UseVisualStyleBackColor = true;
            // 
            // progressBar1
            // 
            this.progressBar1.CustomText = null;
            this.progressBar1.DisplayStyle = ProgressBarSample.ProgressBarDisplayText.CustomText;
            this.progressBar1.Location = new System.Drawing.Point(13, 217);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(373, 23);
            this.progressBar1.TabIndex = 2;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.animationTypeCombo);
            this.groupBox1.Location = new System.Drawing.Point(13, 269);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(376, 52);
            this.groupBox1.TabIndex = 20;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Animation";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 13);
            this.label1.TabIndex = 21;
            this.label1.Text = "Type";
            // 
            // animationTypeCombo
            // 
            this.animationTypeCombo.FormattingEnabled = true;
            this.animationTypeCombo.Location = new System.Drawing.Point(43, 18);
            this.animationTypeCombo.Name = "animationTypeCombo";
            this.animationTypeCombo.Size = new System.Drawing.Size(121, 21);
            this.animationTypeCombo.TabIndex = 20;
            // 
            // UtilityMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(406, 677);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.removeTransparentFacesCheckbox);
            this.Controls.Add(this.optionsGroup);
            this.Controls.Add(this.updateButton);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.rebuildMaterialsCheckbox);
            this.Controls.Add(this.sceneListbox);
            this.Controls.Add(this.refreshButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(412, 370);
            this.Name = "UtilityMainForm";
            this.Text = "Daz Studio 4 Bridge";
            this.optionsGroup.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.ListBox sceneListbox;
        private CustomProgressBar progressBar1;
        private System.Windows.Forms.Button updateButton;
        private System.Windows.Forms.GroupBox optionsGroup;
        private System.Windows.Forms.ComboBox materialSelectDropDown;
        private System.Windows.Forms.Button getMaterialProperties_button;
        private System.Windows.Forms.CheckBox rebuildMaterialsCheckbox;
        private System.Windows.Forms.CheckBox removeTransparentFacesCheckbox;
        private System.Windows.Forms.Panel materialControlsPanel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox animationTypeCombo;
    }
}