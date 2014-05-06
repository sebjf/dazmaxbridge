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
    public partial class EditableMessageBox : MaxCustomControls.MaxForm
    {
        public EditableMessageBox()
        {
            InitializeComponent();
        }

        public EditableMessageBox(string content, string caption)
        {
            InitializeComponent();
            this.contentTextBox.Text = content;
        }
    }
}
