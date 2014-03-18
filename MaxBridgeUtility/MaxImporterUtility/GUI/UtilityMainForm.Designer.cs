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
            this.progressBar1 = new ProgressBarSample.CustomProgressBar();
            this.optionsGroup = new System.Windows.Forms.GroupBox();
            this.disableMapFilteringCheckbox = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.ambientOcclusionEnableCheckbox = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.ambientOcclusionDistanceTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.getMaterialProperties_button = new System.Windows.Forms.Button();
            this.bumpScalarTextBox = new System.Windows.Forms.TextBox();
            this.glossinessScalarTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.materialSelectDropDown = new System.Windows.Forms.ComboBox();
            this.rebuildMaterialsCheckbox = new System.Windows.Forms.CheckBox();
            this.optionsGroup.SuspendLayout();
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
            this.sceneListbox.Size = new System.Drawing.Size(268, 277);
            this.sceneListbox.TabIndex = 1;
            // 
            // updateButton
            // 
            this.updateButton.ForeColor = System.Drawing.Color.Black;
            this.updateButton.Location = new System.Drawing.Point(286, 262);
            this.updateButton.Name = "updateButton";
            this.updateButton.Size = new System.Drawing.Size(100, 27);
            this.updateButton.TabIndex = 3;
            this.updateButton.Text = "Update All";
            this.updateButton.UseVisualStyleBackColor = true;
            this.updateButton.Click += new System.EventHandler(this.updateButton_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.CustomText = null;
            this.progressBar1.DisplayStyle = ProgressBarSample.ProgressBarDisplayText.CustomText;
            this.progressBar1.Location = new System.Drawing.Point(13, 296);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(373, 23);
            this.progressBar1.TabIndex = 2;
            // 
            // optionsGroup
            // 
            this.optionsGroup.Controls.Add(this.disableMapFilteringCheckbox);
            this.optionsGroup.Controls.Add(this.label5);
            this.optionsGroup.Controls.Add(this.ambientOcclusionEnableCheckbox);
            this.optionsGroup.Controls.Add(this.label4);
            this.optionsGroup.Controls.Add(this.ambientOcclusionDistanceTextBox);
            this.optionsGroup.Controls.Add(this.label3);
            this.optionsGroup.Controls.Add(this.getMaterialProperties_button);
            this.optionsGroup.Controls.Add(this.bumpScalarTextBox);
            this.optionsGroup.Controls.Add(this.glossinessScalarTextBox);
            this.optionsGroup.Controls.Add(this.label2);
            this.optionsGroup.Controls.Add(this.label1);
            this.optionsGroup.Controls.Add(this.materialSelectDropDown);
            this.optionsGroup.Location = new System.Drawing.Point(12, 344);
            this.optionsGroup.Name = "optionsGroup";
            this.optionsGroup.Size = new System.Drawing.Size(377, 232);
            this.optionsGroup.TabIndex = 4;
            this.optionsGroup.TabStop = false;
            this.optionsGroup.Text = "Options";
            // 
            // disableMapFilteringCheckbox
            // 
            this.disableMapFilteringCheckbox.AutoSize = true;
            this.disableMapFilteringCheckbox.Location = new System.Drawing.Point(153, 103);
            this.disableMapFilteringCheckbox.Name = "disableMapFilteringCheckbox";
            this.disableMapFilteringCheckbox.Size = new System.Drawing.Size(15, 14);
            this.disableMapFilteringCheckbox.TabIndex = 16;
            this.disableMapFilteringCheckbox.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 103);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(105, 13);
            this.label5.TabIndex = 15;
            this.label5.Text = "Disable Map Filtering";
            // 
            // ambientOcclusionEnableCheckbox
            // 
            this.ambientOcclusionEnableCheckbox.AutoSize = true;
            this.ambientOcclusionEnableCheckbox.Location = new System.Drawing.Point(153, 126);
            this.ambientOcclusionEnableCheckbox.Name = "ambientOcclusionEnableCheckbox";
            this.ambientOcclusionEnableCheckbox.Size = new System.Drawing.Size(15, 14);
            this.ambientOcclusionEnableCheckbox.TabIndex = 14;
            this.ambientOcclusionEnableCheckbox.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 126);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(131, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "Ambient Occlusion Enable";
            // 
            // ambientOcclusionDistanceTextBox
            // 
            this.ambientOcclusionDistanceTextBox.Location = new System.Drawing.Point(153, 146);
            this.ambientOcclusionDistanceTextBox.Name = "ambientOcclusionDistanceTextBox";
            this.ambientOcclusionDistanceTextBox.Size = new System.Drawing.Size(67, 20);
            this.ambientOcclusionDistanceTextBox.TabIndex = 12;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 149);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(140, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Ambient Occlusion Distance";
            // 
            // getMaterialProperties_button
            // 
            this.getMaterialProperties_button.ForeColor = System.Drawing.Color.Black;
            this.getMaterialProperties_button.Location = new System.Drawing.Point(10, 190);
            this.getMaterialProperties_button.Name = "getMaterialProperties_button";
            this.getMaterialProperties_button.Size = new System.Drawing.Size(140, 23);
            this.getMaterialProperties_button.TabIndex = 8;
            this.getMaterialProperties_button.Text = "View Material Properties";
            this.getMaterialProperties_button.UseVisualStyleBackColor = true;
            this.getMaterialProperties_button.Click += new System.EventHandler(this.getMaterialProperties_button_Click);
            // 
            // bumpScalarTextBox
            // 
            this.bumpScalarTextBox.Location = new System.Drawing.Point(153, 76);
            this.bumpScalarTextBox.Name = "bumpScalarTextBox";
            this.bumpScalarTextBox.Size = new System.Drawing.Size(67, 20);
            this.bumpScalarTextBox.TabIndex = 6;
            // 
            // glossinessScalarTextBox
            // 
            this.glossinessScalarTextBox.Location = new System.Drawing.Point(153, 50);
            this.glossinessScalarTextBox.Name = "glossinessScalarTextBox";
            this.glossinessScalarTextBox.Size = new System.Drawing.Size(67, 20);
            this.glossinessScalarTextBox.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 79);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Bump Scalar";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 53);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(90, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Glossiness Scalar";
            // 
            // materialSelectDropDown
            // 
            this.materialSelectDropDown.FormattingEnabled = true;
            this.materialSelectDropDown.Location = new System.Drawing.Point(7, 20);
            this.materialSelectDropDown.Name = "materialSelectDropDown";
            this.materialSelectDropDown.Size = new System.Drawing.Size(361, 21);
            this.materialSelectDropDown.TabIndex = 0;
            this.materialSelectDropDown.SelectedIndexChanged += new System.EventHandler(this.materialSelectDropDown_SelectedIndexChanged);
            // 
            // rebuildMaterialsCheckbox
            // 
            this.rebuildMaterialsCheckbox.AutoSize = true;
            this.rebuildMaterialsCheckbox.Location = new System.Drawing.Point(13, 325);
            this.rebuildMaterialsCheckbox.Name = "rebuildMaterialsCheckbox";
            this.rebuildMaterialsCheckbox.Size = new System.Drawing.Size(117, 17);
            this.rebuildMaterialsCheckbox.TabIndex = 5;
            this.rebuildMaterialsCheckbox.Text = "Re-Import Materials";
            this.rebuildMaterialsCheckbox.UseVisualStyleBackColor = true;
            // 
            // UtilityMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(396, 588);
            this.Controls.Add(this.rebuildMaterialsCheckbox);
            this.Controls.Add(this.optionsGroup);
            this.Controls.Add(this.updateButton);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.sceneListbox);
            this.Controls.Add(this.refreshButton);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(423, 626);
            this.MinimumSize = new System.Drawing.Size(412, 370);
            this.Name = "UtilityMainForm";
            this.Text = "Daz Studio 4 Bridge";
            this.optionsGroup.ResumeLayout(false);
            this.optionsGroup.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.ListBox sceneListbox;
        private CustomProgressBar progressBar1;
        private System.Windows.Forms.Button updateButton;
        private System.Windows.Forms.GroupBox optionsGroup;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox materialSelectDropDown;
        private System.Windows.Forms.TextBox bumpScalarTextBox;
        private System.Windows.Forms.TextBox glossinessScalarTextBox;
        private System.Windows.Forms.Button getMaterialProperties_button;
        private System.Windows.Forms.TextBox ambientOcclusionDistanceTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox ambientOcclusionEnableCheckbox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox disableMapFilteringCheckbox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox rebuildMaterialsCheckbox;
    }
}