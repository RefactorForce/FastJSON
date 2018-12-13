using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Collections;
using System.Text;
using System.Runtime.Serialization;
using System.Linq;
using System.Data;
using System.Collections.Specialized;

namespace FastJSON
{
    public struct Getters
    {
        public string Name;
        public string lcName;
        public string memberName;
        public Reflection.GenericGetter Getter;
    }

    public enum PropInfoType
    {
        Int,
        Long,
        String,
        Bool,
        DateTime,
        Enum,
        Guid,
        Array,
        ByteArray,
        Dictionary,
        StringKeyDictionary,
        NameValue,
        StringDictionary,
        Hashtable,
        DataSet,
        DataTable,
        Custom,
        Unknown,
    }

    public class PropInfo
    {
        public Type pt;
        public Type bt;
        public Type changeType;
        public Reflection.GenericSetter setter;
        public Reflection.GenericGetter getter;
        public Type[] GenericTypes;
        public string Name;
        public string memberName;
        public PropInfoType Type;
        public bool CanWrite;

        public bool IsClass;
        public bool IsValueType;
        public bool IsGenericType;
        public bool IsStruct;
        public bool IsInterface;
    }

    public sealed class Reflection
    {
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static Reflection() { }
        private Reflection() { }
        public static Reflection Instance { get; } = new Reflection();

        public static bool RDBMode = false;

        public delegate string Serialize(object data);
        public delegate object Deserialize(string data);

        public delegate object GenericSetter(object target, object value);
        public delegate object GenericGetter(object obj);
        private delegate object CreateObject();
        private delegate object CreateList(int capacity);

        private SafeDictionary<Type, string> _tyname = new SafeDictionary<Type, string>(10);
        private SafeDictionary<string, Type> _typecache = new SafeDictionary<string, Type>(10);
        private SafeDictionary<Type, CreateObject> _constrcache = new SafeDictionary<Type, CreateObject>(10);
        private SafeDictionary<Type, CreateList> _conlistcache = new SafeDictionary<Type, CreateList>(10);
        private SafeDictionary<Type, Getters[]> _getterscache = new SafeDictionary<Type, Getters[]>(10);
        private SafeDictionary<string, Dictionary<string, PropInfo>> _propertycache = new SafeDictionary<string, Dictionary<string, PropInfo>>(10);
        private SafeDictionary<Type, Type[]> _genericTypes = new SafeDictionary<Type, Type[]>(10);
        private SafeDictionary<Type, Type> _genericTypeDef = new SafeDictionary<Type, Type>(10);
        private static SafeDictionary<short, OpCode> _opCodes;

        private static bool TryGetOpCode(short code, out OpCode opCode)
        {
            if (_opCodes != null)
                return _opCodes.TryGetValue(code, out opCode);
            SafeDictionary<short, OpCode> dict = new SafeDictionary<short, OpCode>();
            foreach (FieldInfo fi in typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (!typeof(OpCode).IsAssignableFrom(fi.FieldType)) continue;
                OpCode innerOpCode = (OpCode)fi.GetValue(null);
                if (innerOpCode.OpCodeType != OpCodeType.Nternal)
                    dict.Add(innerOpCode.Value, innerOpCode);
            }
            _opCodes = dict;
            return _opCodes.TryGetValue(code, out opCode);
        }

        #region bjson custom types
        //internal UnicodeEncoding unicode = new UnicodeEncoding();
        private static UTF8Encoding utf8 = new UTF8Encoding();

        // TODO : optimize utf8 
        public static byte[] UTF8GetBytes(string str) => utf8.GetBytes(str);

        public static string UTF8GetString(byte[] bytes, int offset, int len) => utf8.GetString(bytes, offset, len);

        public unsafe static byte[] UnicodeGetBytes(string str)
        {
            int len = str.Length * 2;
            byte[] b = new byte[len];
            fixed (void* ptr = str)
            {
                System.Runtime.InteropServices.Marshal.Copy(new IntPtr(ptr), b, 0, len);
            }
            return b;
        }

        public static string UnicodeGetString(byte[] b) => UnicodeGetString(b, 0, b.Length);

        public unsafe static string UnicodeGetString(byte[] bytes, int offset, int buflen)
        {
            string str = "";
            fixed (byte* bptr = bytes)
            {
                char* cptr = (char*)(bptr + offset);
                str = new string(cptr, 0, buflen / 2);
            }
            return str;
        }
        #endregion

        #region json custom types
        // JSON custom
        internal SafeDictionary<Type, Serialize> _customSerializer = new SafeDictionary<Type, Serialize>();
        internal SafeDictionary<Type, Deserialize> _customDeserializer = new SafeDictionary<Type, Deserialize>();

        internal object CreateCustom(string v, Type type)
        {
            _customDeserializer.TryGetValue(type, out Deserialize d);
            return d(v);
        }

        internal void RegisterCustomType(Type type, Serialize serializer, Deserialize deserializer)
        {
            if (type != null && serializer != null && deserializer != null)
            {
                _customSerializer.Add(type, serializer);
                _customDeserializer.Add(type, deserializer);
                // reset property cache
                Instance.ResetPropertyCache();
            }
        }

        internal bool IsTypeRegistered(Type t) => _customSerializer.Count() == 0 ? false : _customSerializer.TryGetValue(t, out Serialize s);
        #endregion

        public Type GetGenericTypeDefinition(Type t)
        {
            if (_genericTypeDef.TryGetValue(t, out Type tt))
                return tt;
            else
            {
                tt = t.GetGenericTypeDefinition();
                _genericTypeDef.Add(t, tt);
                return tt;
            }
        }

        public Type[] GetGenericArguments(Type t)
        {
            if (_genericTypes.TryGetValue(t, out Type[] tt))
                return tt;
            else
            {
                tt = t.GetGenericArguments();
                _genericTypes.Add(t, tt);
                return tt;
            }
        }

        public Dictionary<string, PropInfo> Getproperties(Type type, string typename, bool ShowReadOnlyProperties)
        {
            if (_propertycache.TryGetValue(typename, out Dictionary<string, PropInfo> sd))
            {
                return sd;
            }
            else
            {
                sd = new Dictionary<string, PropInfo>(10);
                BindingFlags bf = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
                PropertyInfo[] pr = type.GetProperties(bf);
                foreach (PropertyInfo p in pr)
                {
                    if (p.GetIndexParameters().Length > 0)// Property is an indexer
                        continue;

                    PropInfo d = CreateMyProp(p.PropertyType, p.Name);
                    d.setter = CreateSetMethod(type, p, ShowReadOnlyProperties);
                    if (d.setter != null)
                        d.CanWrite = true;
                    d.getter = CreateGetMethod(type, p);
                    object[] att = p.GetCustomAttributes(true);
                    foreach (object at in att)
                    {
                        if (at is DataMemberAttribute dm)
                        {
                            if (dm.Name != "")
                                d.memberName = dm.Name;
                        }
                    }
                    if (d.memberName != null)
                        sd.Add(d.memberName, d);
                    else
                    sd.Add(p.Name.ToLowerInvariant(), d);
                }
                FieldInfo[] fi = type.GetFields(bf);
                foreach (FieldInfo f in fi)
                {
                    PropInfo d = CreateMyProp(f.FieldType, f.Name);
                    if (f.IsLiteral == false)
                    {
                        d.setter = CreateSetField(type, f);
                        if (d.setter != null)
                            d.CanWrite = true;
                        d.getter = CreateGetField(type, f);
                        object[] att = f.GetCustomAttributes(true);
                        foreach (object at in att)
                        {
                            if (at is DataMemberAttribute dm)
                            {
                                if (dm.Name != "")
                                    d.memberName = dm.Name;
                            }
                        }
                        if (d.memberName != null)
                            sd.Add(d.memberName, d);
                        else
                        sd.Add(f.Name.ToLowerInvariant(), d);
                    }
                }

                _propertycache.Add(typename, sd);
                return sd;
            }
        }

        private PropInfo CreateMyProp(Type t, string name)
        {
            PropInfo d = new PropInfo();
            PropInfoType d_type = PropInfoType.Unknown;

            if (t == typeof(int) || t == typeof(int?)) d_type = PropInfoType.Int;
            else if (t == typeof(long) || t == typeof(long?)) d_type = PropInfoType.Long;
            else if (t == typeof(string)) d_type = PropInfoType.String;
            else if (t == typeof(bool) || t == typeof(bool?)) d_type = PropInfoType.Bool;
            else if (t == typeof(DateTime) || t == typeof(DateTime?)) d_type = PropInfoType.DateTime;
            else if (t.IsEnum) d_type = PropInfoType.Enum;
            else if (t == typeof(Guid) || t == typeof(Guid?)) d_type = PropInfoType.Guid;
            else if (t == typeof(StringDictionary)) d_type = PropInfoType.StringDictionary;
            else if (t == typeof(NameValueCollection)) d_type = PropInfoType.NameValue;
            else if (t.IsArray)
            {
                d.bt = t.GetElementType();
                d_type = t == typeof(byte[]) ? PropInfoType.ByteArray : PropInfoType.Array;
            }
            else if (t.Name.Contains("Dictionary"))
            {
                d.GenericTypes = Instance.GetGenericArguments(t);
                d_type = d.GenericTypes.Length > 0 && d.GenericTypes[0] == typeof(string) ? PropInfoType.StringKeyDictionary : PropInfoType.Dictionary;
            }
            else if (t == typeof(Hashtable)) d_type = PropInfoType.Hashtable;
            else if (t == typeof(DataSet)) d_type = PropInfoType.DataSet;
            else if (t == typeof(DataTable)) d_type = PropInfoType.DataTable;
            else if (IsTypeRegistered(t))
                d_type = PropInfoType.Custom;

            if (t.IsValueType && !t.IsPrimitive && !t.IsEnum && t != typeof(decimal))
                d.IsStruct = true;

            d.IsInterface = t.IsInterface;
            d.IsClass = t.IsClass;
            d.IsValueType = t.IsValueType;
            if (t.IsGenericType)
            {
                d.IsGenericType = true;
                d.bt = Instance.GetGenericArguments(t)[0];
            }

            d.pt = t;
            d.Name = name;
            d.changeType = GetChangeType(t);
            d.Type = d_type;

            return d;
        }

        private Type GetChangeType(Type conversionType) => conversionType.IsGenericType && conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>))
                ? Instance.GetGenericArguments(conversionType)[0]
                : conversionType;

        public string GetTypeAssemblyName(Type t)
        {
            if (_tyname.TryGetValue(t, out string val))
                return val;
            else
            {
                string s = t.AssemblyQualifiedName;
                _tyname.Add(t, s);
                return s;
            }
        }

        internal Type GetTypeFromCache(string typename)
        {
            if (_typecache.TryGetValue(typename, out Type val))
                return val;
            else
            {
                Type t = Type.GetType(typename);
                if (RDBMode)
                {
                    if (t == null) // RaptorDB : loading runtime assemblies
                    {
                        t = Type.GetType(typename, (name) =>
                        {
                            return AppDomain.CurrentDomain.GetAssemblies().Where(z => z.FullName == name.FullName).FirstOrDefault();
                        }, null, true);
                    }
                }
                _typecache.Add(typename, t);
                return t;
            }
        }

        internal object FastCreateList(Type objtype, int capacity)
        {
            try
            {
                int count = 10;
                if (capacity > 10)
                    count = capacity;
                if (_conlistcache.TryGetValue(objtype, out CreateList c))
                {
                    return c != null ? c(count) : FastCreateInstance(objtype);
                }
                else
                {
                    ConstructorInfo cinfo = objtype.GetConstructor(new Type[] { typeof(int) });
                    if (cinfo != null)
                    {
                        DynamicMethod dynMethod = new DynamicMethod("_fcil", objtype, new Type[] { typeof(int) }, true);
                        ILGenerator ilGen = dynMethod.GetILGenerator();
                        ilGen.Emit(OpCodes.Ldarg_0);
                        ilGen.Emit(OpCodes.Newobj, objtype.GetConstructor(new Type[] { typeof(int) }));
                        ilGen.Emit(OpCodes.Ret);
                        c = (CreateList)dynMethod.CreateDelegate(typeof(CreateList));
                        _conlistcache.Add(objtype, c);
                        return c(count);
                    }
                    else
                    {
                        _conlistcache.Add(objtype, null);// kludge : non capacity lists
                        return FastCreateInstance(objtype);
                    }
                }
            }
            catch (Exception exc)
            {
                throw new Exception(String.Format("Failed to fast create instance for type '{0}' from assembly '{1}'",
                    objtype.FullName, objtype.AssemblyQualifiedName), exc);
            }
        }

        internal object FastCreateInstance(Type objtype)
        {
            try
            {
                if (_constrcache.TryGetValue(objtype, out CreateObject c))
                {
                    return c();
                }
                else
                {
                    if (objtype.IsClass)
                    {
                        DynamicMethod dynMethod = new DynamicMethod("_fcic", objtype, null, true);
                        ILGenerator ilGen = dynMethod.GetILGenerator();
                        ilGen.Emit(OpCodes.Newobj, objtype.GetConstructor(Type.EmptyTypes));
                        ilGen.Emit(OpCodes.Ret);
                        c = (CreateObject)dynMethod.CreateDelegate(typeof(CreateObject));
                        _constrcache.Add(objtype, c);
                    }
                    else // structs
                    {
                        DynamicMethod dynMethod = new DynamicMethod("_fcis", typeof(object), null, true);
                        ILGenerator ilGen = dynMethod.GetILGenerator();
                        LocalBuilder lv = ilGen.DeclareLocal(objtype);
                        ilGen.Emit(OpCodes.Ldloca_S, lv);
                        ilGen.Emit(OpCodes.Initobj, objtype);
                        ilGen.Emit(OpCodes.Ldloc_0);
                        ilGen.Emit(OpCodes.Box, objtype);
                        ilGen.Emit(OpCodes.Ret);
                        c = (CreateObject)dynMethod.CreateDelegate(typeof(CreateObject));
                        _constrcache.Add(objtype, c);
                    }
                    return c();
                }
            }
            catch (Exception exc)
            {
                throw new Exception(String.Format("Failed to fast create instance for type '{0}' from assembly '{1}'",
                    objtype.FullName, objtype.AssemblyQualifiedName), exc);
            }
        }

        internal static GenericSetter CreateSetField(Type type, FieldInfo fieldInfo)
        {
            Type[] arguments = new Type[2];
            arguments[0] = arguments[1] = typeof(object);

            DynamicMethod dynamicSet = new DynamicMethod("_csf", typeof(object), arguments, type, true);

            ILGenerator il = dynamicSet.GetILGenerator();

            if (!type.IsClass) // structs
            {
                LocalBuilder lv = il.DeclareLocal(type);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Unbox_Any, type);
                il.Emit(OpCodes.Stloc_0);
                il.Emit(OpCodes.Ldloca_S, lv);
                il.Emit(OpCodes.Ldarg_1);
                if (fieldInfo.FieldType.IsClass)
                    il.Emit(OpCodes.Castclass, fieldInfo.FieldType);
                else
                    il.Emit(OpCodes.Unbox_Any, fieldInfo.FieldType);
                il.Emit(OpCodes.Stfld, fieldInfo);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Box, type);
                il.Emit(OpCodes.Ret);
            }
            else
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                if (fieldInfo.FieldType.IsValueType)
                    il.Emit(OpCodes.Unbox_Any, fieldInfo.FieldType);
                il.Emit(OpCodes.Stfld, fieldInfo);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ret);
            }
            return (GenericSetter)dynamicSet.CreateDelegate(typeof(GenericSetter));
        }

        internal static FieldInfo GetGetterBackingField(PropertyInfo autoProperty)
        {
            MethodInfo getMethod = autoProperty.GetGetMethod();
            // Restrict operation to auto properties to avoid risking errors if a getter does not contain exactly one field read instruction (such as with calculated properties).
            if (!getMethod.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false)) return null;

            byte[] byteCode = getMethod.GetMethodBody()?.GetILAsByteArray() ?? new byte[0];
            //var byteCode = getMethod.GetMethodBody().GetILAsByteArray();
            int pos = 0;
            // Find the first LdFld instruction and parse its operand to a FieldInfo object.
            while (pos < byteCode.Length)
            {
                // Read and parse the OpCode (it can be 1 or 2 bytes in size).
                byte code = byteCode[pos++];
                if (!(TryGetOpCode(code, out OpCode opCode) || pos < byteCode.Length && TryGetOpCode((short)(code * 0x100 + byteCode[pos++]), out opCode)))
                    throw new NotSupportedException("Unknown IL code detected.");
                // If it is a LdFld, read its operand, parse it to a FieldInfo and return it.
                if (opCode == OpCodes.Ldfld && opCode.OperandType == OperandType.InlineField && pos + sizeof(int) <= byteCode.Length)
                {
                    return getMethod.Module.ResolveMember(BitConverter.ToInt32(byteCode, pos), getMethod.DeclaringType?.GetGenericArguments(), null) as FieldInfo;
                }
                // Otherwise, set the current position to the start of the next instruction, if any (we need to know how much bytes are used by operands).
                pos += opCode.OperandType == OperandType.InlineNone
                            ? 0
                            : opCode.OperandType == OperandType.ShortInlineBrTarget ||
                              opCode.OperandType == OperandType.ShortInlineI ||
                              opCode.OperandType == OperandType.ShortInlineVar
                                ? 1
                                : opCode.OperandType == OperandType.InlineVar
                                    ? 2
                                    : opCode.OperandType == OperandType.InlineI8 ||
                                      opCode.OperandType == OperandType.InlineR
                                        ? 8
                                        : opCode.OperandType == OperandType.InlineSwitch
                                            ? 4 * (BitConverter.ToInt32(byteCode, pos) + 1)
                                            : 4;
            }
            return null;
        }

        internal static GenericSetter CreateSetMethod(Type type, PropertyInfo propertyInfo, bool ShowReadOnlyProperties)
        {
            MethodInfo setMethod = propertyInfo.GetSetMethod(ShowReadOnlyProperties);
            if (setMethod == null)
            {
                if (!ShowReadOnlyProperties) return null;
                // If the property has no setter and it is an auto property, try and create a setter for its backing field instead 
                FieldInfo fld = GetGetterBackingField(propertyInfo);
                return fld != null ? CreateSetField(type, fld) : null;
            }

            Type[] arguments = new Type[2];
            arguments[0] = arguments[1] = typeof(object);

            DynamicMethod setter = new DynamicMethod("_csm", typeof(object), arguments, true);// !setMethod.IsPublic); // fix: skipverify
            ILGenerator il = setter.GetILGenerator();

            if (!type.IsClass) // structs
            {
                LocalBuilder lv = il.DeclareLocal(type);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Unbox_Any, type);
                il.Emit(OpCodes.Stloc_0);
                il.Emit(OpCodes.Ldloca_S, lv);
                il.Emit(OpCodes.Ldarg_1);
                if (propertyInfo.PropertyType.IsClass)
                    il.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
                else
                    il.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
                il.EmitCall(OpCodes.Call, setMethod, null);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Box, type);
            }
            else
            {
                if (!setMethod.IsStatic)
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
                    il.Emit(OpCodes.Ldarg_1);
                    if (propertyInfo.PropertyType.IsClass)
                        il.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
                    else
                        il.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
                    il.EmitCall(OpCodes.Callvirt, setMethod, null);
                    il.Emit(OpCodes.Ldarg_0);
                }
                else
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    if (propertyInfo.PropertyType.IsClass)
                        il.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
                    else
                        il.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
                    il.Emit(OpCodes.Call, setMethod);
                }
            }

            il.Emit(OpCodes.Ret);

            return (GenericSetter)setter.CreateDelegate(typeof(GenericSetter));
        }

        internal static GenericGetter CreateGetField(Type type, FieldInfo fieldInfo)
        {
            DynamicMethod dynamicGet = new DynamicMethod("_cgf", typeof(object), new Type[] { typeof(object) }, type, true);

            ILGenerator il = dynamicGet.GetILGenerator();

            if (!type.IsClass) // structs
            {
                LocalBuilder lv = il.DeclareLocal(type);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Unbox_Any, type);
                il.Emit(OpCodes.Stloc_0);
                il.Emit(OpCodes.Ldloca_S, lv);
                il.Emit(OpCodes.Ldfld, fieldInfo);
                if (fieldInfo.FieldType.IsValueType)
                    il.Emit(OpCodes.Box, fieldInfo.FieldType);
            }
            else
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, fieldInfo);
                if (fieldInfo.FieldType.IsValueType)
                    il.Emit(OpCodes.Box, fieldInfo.FieldType);
            }

            il.Emit(OpCodes.Ret);

            return (GenericGetter)dynamicGet.CreateDelegate(typeof(GenericGetter));
        }

        internal static GenericGetter CreateGetMethod(Type type, PropertyInfo propertyInfo)
        {
            MethodInfo getMethod = propertyInfo.GetGetMethod();
            if (getMethod == null)
                return null;

            DynamicMethod getter = new DynamicMethod("_cgm", typeof(object), new Type[] { typeof(object) }, type, true);

            ILGenerator il = getter.GetILGenerator();

            if (!type.IsClass) // structs
            {
                LocalBuilder lv = il.DeclareLocal(type);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Unbox_Any, type);
                il.Emit(OpCodes.Stloc_0);
                il.Emit(OpCodes.Ldloca_S, lv);
                il.EmitCall(OpCodes.Call, getMethod, null);
                if (propertyInfo.PropertyType.IsValueType)
                    il.Emit(OpCodes.Box, propertyInfo.PropertyType);
            }
            else
            {
                if (!getMethod.IsStatic)
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
                    il.EmitCall(OpCodes.Callvirt, getMethod, null);
                }
                else
                    il.Emit(OpCodes.Call, getMethod);

                if (propertyInfo.PropertyType.IsValueType)
                    il.Emit(OpCodes.Box, propertyInfo.PropertyType);
            }

            il.Emit(OpCodes.Ret);

            return (GenericGetter)getter.CreateDelegate(typeof(GenericGetter));
        }

        public Getters[] GetGetters(Type type, bool ShowReadOnlyProperties, List<Type> IgnoreAttributes)
        {
            if (_getterscache.TryGetValue(type, out Getters[] val))
                return val;

            BindingFlags bf = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
            //if (ShowReadOnlyProperties)
            //    bf |= BindingFlags.NonPublic;
            PropertyInfo[] props = type.GetProperties(bf);
            List<Getters> getters = new List<Getters>();
            foreach (PropertyInfo p in props)
            {
                if (p.GetIndexParameters().Length > 0)
                {// Property is an indexer
                    continue;
                }
                if (!p.CanWrite && ShowReadOnlyProperties == false)//|| isAnonymous == false))
                    continue;
                if (IgnoreAttributes != null)
                {
                    bool found = false;
                    foreach (Type ignoreAttr in IgnoreAttributes)
                    {
                        if (p.IsDefined(ignoreAttr, false))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found)
                        continue;
                }
                string mName = null;
                object[] att = p.GetCustomAttributes(true);
                foreach (object at in att)
                {
                    if (at is DataMemberAttribute dm)
                    {
                        if (dm.Name != "")
                        {
                            mName = dm.Name;
                        }
                    }
                }
                GenericGetter g = CreateGetMethod(type, p);
                if (g != null)
                    getters.Add(new Getters { Getter = g, Name = p.Name, lcName = p.Name.ToLowerInvariant(), memberName = mName });
            }

            FieldInfo[] fi = type.GetFields(bf);
            foreach (FieldInfo f in fi)
            {
                if (IgnoreAttributes != null)
                {
                    bool found = false;
                    foreach (Type ignoreAttr in IgnoreAttributes)
                    {
                        if (f.IsDefined(ignoreAttr, false))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found)
                        continue;
                }
                string mName = null;
                object[] att = f.GetCustomAttributes(true);
                foreach (object at in att)
                {
                    if (at is DataMemberAttribute dm)
                    {
                        if (dm.Name != "")
                        {
                            mName = dm.Name;
                        }
                    }
                }
                if (f.IsLiteral == false)
                {
                    GenericGetter g = CreateGetField(type, f);
                    if (g != null)
                        getters.Add(new Getters { Getter = g, Name = f.Name, lcName = f.Name.ToLowerInvariant(), memberName = mName });
                }
            }
            val = getters.ToArray();
            _getterscache.Add(type, val);
            return val;
        }

        //private static bool IsAnonymousType(Type type)
        //{
        //    // may break in the future if compiler defined names change...
        //    const string CS_ANONYMOUS_PREFIX = "<>f__AnonymousType";
        //    const string VB_ANONYMOUS_PREFIX = "VB$AnonymousType";

        //    if (type == null)
        //        throw new ArgumentNullException("type");

        //    if (type.Name.StartsWith(CS_ANONYMOUS_PREFIX, StringComparison.Ordinal) || type.Name.StartsWith(VB_ANONYMOUS_PREFIX, StringComparison.Ordinal))
        //    {
        //        return type.IsDefined(typeof(CompilerGeneratedAttribute), false);
        //    }

        //    return false;
        //}

        internal void ResetPropertyCache() => _propertycache = new SafeDictionary<string, Dictionary<string, PropInfo>>();

        internal void ClearReflectionCache()
        {
            _tyname = new SafeDictionary<Type, string>(10);
            _typecache = new SafeDictionary<string, Type>(10);
            _constrcache = new SafeDictionary<Type, CreateObject>(10);
            _getterscache = new SafeDictionary<Type, Getters[]>(10);
            _propertycache = new SafeDictionary<string, Dictionary<string, PropInfo>>(10);
            _genericTypes = new SafeDictionary<Type, Type[]>(10);
            _genericTypeDef = new SafeDictionary<Type, Type>(10);
        }
    }
}
