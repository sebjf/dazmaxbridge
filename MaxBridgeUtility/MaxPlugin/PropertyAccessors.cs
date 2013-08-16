using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Max;


namespace MaxManagedBridge
{
    public partial class MaxPlugin : MaxBridge
    {
        #region Property Finding and Setting 

        public Parameter FindParameter(string Name, IReferenceMaker Object)
        {
            return EnumerateReferences(Object).FirstOrDefault(r => (r.Name == Name) || (r.InternalName == Name));
        }

        public bool SetParameter(IReferenceMaker Object, string Name, bool Value)
        {
            Parameter p = FindParameter(Name, Object);
            if (p == null)
            {
                System.Windows.Forms.MessageBox.Show("Unable to find parameter " + Name);
                return false;
            }
            if (!p.SetValue(Value))
            {
                System.Windows.Forms.MessageBox.Show("Setting parameter " + p.Name + " of type " + p.Type + " failed.");
                return false;
            }
            return true;            
        }

        public bool SetParameter(IReferenceMaker Object, string Name, float Value)
        {
            Parameter p = FindParameter(Name, Object);
            if (p == null)
            {
                System.Windows.Forms.MessageBox.Show("Unable to find parameter " + Name);
                return false;
            }
            if (!p.SetValue(Value))
            {
                System.Windows.Forms.MessageBox.Show("Setting parameter " + p.Name + " of type " + p.Type + " failed.");
                return false;
            }
            return true;    
        }

        public bool SetParameter(IReferenceMaker Object, string Name, ITexmap Value)
        {
            Parameter p = FindParameter(Name, Object);
            if (p == null)
            {
                System.Windows.Forms.MessageBox.Show("Unable to find parameter " + Name);
                return false;
            }
            if (!p.SetValue(Value))
            {
                System.Windows.Forms.MessageBox.Show("Setting parameter " + p.Name + " of type " + p.Type + " failed.");
                return false;
            }
            return true;    
        }

        public bool SetParameter(IReferenceMaker Object, string Name, IAColor Value)
        {
            Parameter p = FindParameter(Name, Object);
            if (p == null)
            {
                System.Windows.Forms.MessageBox.Show("Unable to find parameter " + Name);
                return false;
            }
            if (!p.SetValue(Value))
            {
                System.Windows.Forms.MessageBox.Show("Setting parameter " + p.Name + " of type " + p.Type + " failed.");
                return false;
            }
            return true;    
        }

        public bool SetParameter(IReferenceMaker Object, string Name, IColor Value)
        {
            Parameter p = FindParameter(Name, Object);
            if (p == null)
            {
                System.Windows.Forms.MessageBox.Show("Unable to find parameter " + Name);
                return false;
            }
            if (!p.SetValue(Value))
            {
                System.Windows.Forms.MessageBox.Show("Setting parameter " + p.Name + " of type " + p.Type + " failed.");
                return false;
            }
            return true;
        }

        public bool SetParameter(IReferenceMaker Object, string Name, string Value)
        {
            Parameter p = FindParameter(Name, Object);
            if (p == null)
            {
                System.Windows.Forms.MessageBox.Show("Unable to find parameter " + Name);
                return false;
            }
            if (!p.SetValue(Value))
            {
                System.Windows.Forms.MessageBox.Show("Setting parameter " + p.Name + " of type " + p.Type + " failed.");
                return false;
            }
            return true;    
        }

        #endregion

        #region Parameter Type

        public class Parameter
        {
            public IIParamBlock2 ParamBlock;

            public string Name;
            public string InternalName;

            public short Id;
            public int TableId;
            public ParamType2 Type;

            public Parameter(IIParamBlock2 blck, short idx, int tabid)
            {
                this.ParamBlock = blck;
                this.Name = blck.GetLocalName(idx, tabid);
                this.InternalName = blck.GetParamDef(idx).IntName;

                this.Id = idx;
                this.TableId = tabid;
                this.Type = blck.GetParameterType(idx);

            }

            public Parameter(IIParamBlock2 blck, IParamAlias alias)
            {
                this.ParamBlock = blck;
                this.Name = alias.Alias;
                //Internal names are not added for aliases

                this.Id = alias.Id;
                this.TableId = alias.TabIndex;
                this.Type = blck.GetParameterType(alias.Id);
            }

            public bool SetValue(bool value)
            {
               return ParamBlock.SetValue(Id, 0, value ? 1 : 0, TableId);
            }
            public bool SetValue(float value)
            {
                return ParamBlock.SetValue(Id, 0, value, TableId);
            }
            public bool SetValue(ITexmap value)
            {
                return ParamBlock.SetValue(Id, 0, value, TableId);
            }           
            public bool SetValue(IAColor value)
            {
                return ParamBlock.SetValue(Id, 0, value, TableId);
            }
            public bool SetValue(IColor value)
            {
                return ParamBlock.SetValue(Id, 0, value, TableId);
            }
            public bool SetValue(string value)
            {
                return ParamBlock.SetValue(Id, 0, value, TableId);
            }
            
        }

        #endregion

        #region Parameter Enumeration

        /* Based on http://forums.cgsociety.org/archive/index.php/t-1041239.html */

        public IEnumerable<Parameter> EnumerateParamBlock(IIParamBlock2 block)
        {
            for (short i = 0; i < block.NumParams; i++)
            {
                int tableLength = 1;

                if (Enum.GetName(typeof(ParamType2), block.GetParameterType(i)).Contains("Tab"))    //use subanimatables instead of tabs?
                {
                    tableLength = block.Count(i); //this throws an exception if it is called for an index that is not a table
                }

                for (int t = 0; t < tableLength; t++)
                {
                    yield return new Parameter(block, i, t);
                }
            }

            for (int i = 0; i < block.ParamAliasCount; i++)
            {
                yield return new Parameter(block, block.GetParamAlias(i));
            }
        }


        public IEnumerable<Parameter> EnumerateReferences(IReferenceMaker obj)
        {
            for (int i = 0; i < obj.NumRefs; i++)
            {
                IReferenceTarget iref = obj.GetReference(i);

                if (iref is IIParamBlock)
                {
                    //Until we can identify IParamBlock member names there is no point in adding them to the list
                    //foreach (var v in EnumerateParamBlock((IIParamBlock)iref))
                    //{
                    //    yield return v;
                    //}
                    continue;
                }
                if (iref is IIParamBlock2)
                {
                    foreach (var v in EnumerateParamBlock((IIParamBlock2)iref))
                    {
                        yield return v;
                    }
                    continue;
                }
                if (iref is IReferenceMaker)
                {
                    foreach (var v in EnumerateReferences(iref))
                    {
                        yield return v;
                    }
                    continue;
                }
            }

        }

        #endregion

    }
}
