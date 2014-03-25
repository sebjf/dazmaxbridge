using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Max;

/* The methods in this file are for setting properties of Max objects by name. This is intended to allow materials to be built without the 
 * generated MaxScript. They are not used anywhere currently but remain for reference in case we pursue building materials entirely within C# */

namespace MaxManagedBridge
{
    public static class MaxExtensions
    {
        public static bool EqualsClassID(this IClass_ID classA, int a, int b)
        {
            return ((classA.PartA == a) && (classA.PartB == b));
        }
    }

    public static class LinqExtensions
    {
        //http://stackoverflow.com/questions/489258/linq-distinct-on-a-particular-property

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }

    public static class IReferenceMakerExtensions
    {
        #region Parameter Enumeration

        /* Based on http://forums.cgsociety.org/archive/index.php/t-1041239.html */

        private static IEnumerable<Parameter> EnumerateParamBlock(IIParamBlock2 block)
        {
            for (short i = 0; i < block.NumParams; i++)
            {
                int tableLength = 1;

                string paramTypeName = Enum.GetName(typeof(ParamType2), block.GetParameterType(i));

                if (paramTypeName == null)
                {
                    continue; //if the paramtype is unknown, then just skip it
                }

                if (paramTypeName.Contains("Tab"))    //use subanimatables instead of tabs?
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

        //check references! eq. to what maxscript uses to find all calsses
        private static IEnumerable<Parameter> EnumerateReferences(IReferenceMaker obj, bool recursive = false)
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
                if (recursive)
                {
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

        }

        public static IEnumerable<Parameter> EnumerateProperties(this IReferenceMaker obj)
        {
            return EnumerateReferences(obj).DistinctBy(p => p.Name);
        }

        public static Parameter FindPropertyByName(this IReferenceMaker obj, string name)
        {
            return EnumerateReferences(obj).Where(p => (p.Name == name || p.InternalName == name)).FirstOrDefault();
        }

        #endregion
        
    }

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

        public bool IsMapType
        {
            get
            {
                switch (Type)
                {
                    case ParamType2.Texmap:
                    case ParamType2.TexmapTab:
                    case ParamType2.Bitmap:
                    case ParamType2.BitmapTab:

                        return true;
                    default:
                        return false;
                }
            }
        }

        public bool IsValueType
        {
            get { return !IsMapType; }
        }

        public bool SetValue(bool value)
        {
            return ParamBlock.SetValue(Id, 0, value ? 1 : 0, TableId);
        }
        public bool SetValue(float value)
        {
            return ParamBlock.SetValue(Id, 0, value, TableId);
        }
        public bool SetValue(int value)
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
        public bool SetValue(IMtl value)
        {
            return ParamBlock.SetValue(Id, 0, value, TableId);
        }

        public string GetString()
        {
            return ParamBlock.GetStr(Id, 0, TableId);
        }

        public float GetFloat() //can also return integer types
        {
            switch (Type)
            {
                case ParamType2.Int:
                case ParamType2.Int64:
                case ParamType2.Int64Tab:
                    return ParamBlock.GetInt(Id, 0, TableId);
                default:
                    return ParamBlock.GetFloat(Id, 0, TableId);
            }
        }

        public bool GetBool()
        {
            return (ParamBlock.GetInt(Id, 0, TableId) > 0);
        }

        public object GetValue()
        {
            switch (Type)
            {
                case ParamType2.Bool:
                case ParamType2.Bool2:
                case ParamType2.BoolTab:
                case ParamType2.BoolTab2:
                    return GetBool();

                case ParamType2.PcntFrac:
                case ParamType2.PcntFracTab:
                case ParamType2.Float:
                case ParamType2.FloatTab:
                case ParamType2.Int:
                case ParamType2.Int64:
                case ParamType2.IntTab:
                    return GetFloat();

                case ParamType2.Bitmap:
                case ParamType2.BitmapTab:
                case ParamType2.Texmap:
                case ParamType2.TexmapTab:


                case ParamType2.Filename:
                case ParamType2.FilenameTab:
                case ParamType2.String:
                case ParamType2.StringTab:
                    return GetString();

                default:
                    //throw new Exception("Don't know type for ParamType2: " + Type.ToString());
                    Log.Add("Don't know how to get ParamType2: " + Type.ToString());
                    return null;
            }
        }

        public string GetValueAsString()
        {
            return GetValue().ToString();
        }
    }

    #endregion

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

        #region Parameter Enumeration

        /* Based on http://forums.cgsociety.org/archive/index.php/t-1041239.html */

        protected IEnumerable<Parameter> EnumerateParamBlock(IIParamBlock2 block)
        {
            for (short i = 0; i < block.NumParams; i++)
            {
                int tableLength = 1;

                string paramTypeName = Enum.GetName(typeof(ParamType2), block.GetParameterType(i));

                if (paramTypeName == null)
                {
                    continue; //if the paramtype is unknown, then just skip it
                }

                if (paramTypeName.Contains("Tab"))    //use subanimatables instead of tabs?
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

        //check references! eq. to what maxscript uses to find all calsses
        protected IEnumerable<Parameter> EnumerateReferences(IReferenceMaker obj, bool recursive = false)
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
                if (recursive)
                {
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

        }

        public IEnumerable<Parameter> EnumerateProperties(IReferenceMaker obj)
        {
            return EnumerateReferences(obj).DistinctBy(p => p.Name);
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

            public bool IsMapType
            {
                get
                {
                    switch (Type)
                    {
                        case ParamType2.Texmap:
                        case ParamType2.TexmapTab:
                        case ParamType2.Bitmap:
                        case ParamType2.BitmapTab:

                            return true;
                        default:
                            return false;
                    }
                }
            }

            public bool IsValueType
            {
                get { return !IsMapType; }
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

            public string GetString()
            {
                return ParamBlock.GetStr(Id, 0, TableId);
            }

            public float GetFloat() //can also return integer types
            {
                switch (Type)
                {
                    case ParamType2.Int:
                    case ParamType2.Int64:
                    case ParamType2.Int64Tab:
                        return ParamBlock.GetInt(Id, 0, TableId);
                    default:
                        return ParamBlock.GetFloat(Id, 0, TableId);
                }
            }

            public bool GetBool()
            {
                return (ParamBlock.GetInt(Id, 0, TableId) > 0);
            }

            public object GetValue()
            {
                switch (Type)
                {
                    case ParamType2.Bool:
                    case ParamType2.Bool2:
                    case ParamType2.BoolTab:
                    case ParamType2.BoolTab2:
                        return GetBool();

                    case ParamType2.PcntFrac:
                    case ParamType2.PcntFracTab:
                    case ParamType2.Float:
                    case ParamType2.FloatTab:
                    case ParamType2.Int:
                    case ParamType2.Int64:
                    case ParamType2.IntTab:
                        return GetFloat();

                    case ParamType2.Bitmap:
                    case ParamType2.BitmapTab:
                    case ParamType2.Texmap:
                    case ParamType2.TexmapTab:


                    case ParamType2.Filename:
                    case ParamType2.FilenameTab:
                    case ParamType2.String:
                    case ParamType2.StringTab:
                        return GetString();

                    default:
                        //throw new Exception("Don't know type for ParamType2: " + Type.ToString());
                        Log.Add("Don't know how to get ParamType2: " + Type.ToString());
                        return null;
                }
            }

            public string GetValueAsString()
            {
                return GetValue().ToString();
            }
        }

        #endregion

    }
}
