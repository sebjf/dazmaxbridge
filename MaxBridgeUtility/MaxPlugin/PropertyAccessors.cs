using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Max;


namespace MaxManagedBridge
{
    public partial class MaxPlugin : MaxBridge
    {
        public Parameter FindParameter(string Name, IReferenceMaker Object)
        {
            return EnumerateReferences(Object).FirstOrDefault(r => r.Name == Name);
        }

        public class Parameter
        {
            public IIParamBlock2 ParamBlock;

            public string Name;

            public short Id;
            public int TableId;
            public ParamType2 Type;

            public Parameter(IIParamBlock2 blck, short idx, int tabid)
            {
                this.ParamBlock = blck;
                this.Name = blck.GetLocalName(idx, tabid);

                this.Id = idx;
                this.TableId = tabid;
                this.Type = blck.GetParameterType(idx);

            }

            public Parameter(IIParamBlock2 blck, IParamAlias alias)
            {
                this.ParamBlock = blck;
                this.Name = alias.Alias;

                this.Id = alias.Id;
                this.TableId = alias.TabIndex;
                this.Type = blck.GetParameterType(alias.Id);
            }

            public void SetValue(bool value)
            {
                ParamBlock.SetValue(Id, 0, value ? 1 : 0, TableId);
            }
            public void SetValue(float value)
            {
                ParamBlock.SetValue(Id, 0, value, TableId);
            }
            public void SetValue(ITexmap value)
            {
                ParamBlock.SetValue(Id, 0, value, TableId);
            }
            
        }

        /* Based on http://forums.cgsociety.org/archive/index.php/t-1041239.html */

        public IEnumerable<Parameter> EnumerateParamBlock(IIParamBlock2 block)
        {
            for (short i = 0; i < block.Desc.Count; i++)
            {
                IParamDef definition = block.Desc.GetParamDef(i);
            }

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
                    //         EnumerateParamBlock((IIParamBlock)iref);
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
                if (iref is ITexmap)
                {
                    foreach (var v in EnumerateReferences(iref))
                    {
                        yield return v;
                    }
                    continue;
                }
                if (iref is IShader)
                {
                    foreach (var v in EnumerateReferences(iref))
                    {
                        yield return v;
                    }
                    continue;
                }
                if (iref is ISampler)
                {
                    foreach (var v in EnumerateReferences(iref))
                    {
                        yield return v;
                    }
                    continue;
                }

                //Unknown type
            }
        }

    }
}
