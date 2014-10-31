using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MaxManagedBridge
{
    [System.AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class GuiPropertyAttribute : System.Attribute
    {
        public GuiPropertyAttribute(string displayName, ControlTypeEnum controlType)
        {
            DisplayName = displayName;
            ControlType = controlType;
        }

        public string DisplayName { get; set; }

        public enum ControlTypeEnum
        {
            Textbox,
            Checkbox,
            MaterialTemplateDropdown
        }

        public ControlTypeEnum ControlType { get; set; }


    }
}
