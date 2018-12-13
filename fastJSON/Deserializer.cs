using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.IO;

namespace FastJSON
{
    internal class Deserializer
    {
        public Deserializer(Parameters param)
        {
            param.FixValues();
            Configuration = param.MakeCopy();
        }

        Parameters Configuration { get; set; }
        bool UseGlobals { get; set; } = false;
        Dictionary<object, int> CircularObject { get; set; } = new Dictionary<object, int>();
        Dictionary<int, object> ReverseCircularObject { get; set; } = new Dictionary<int, object>();

        public T ToObject<T>(string json)
        {
            Type type = typeof(T);
            object interpretedObject = ToObject(json, type);

            return type.IsArray ? (interpretedObject as ICollection).Count == 0 ? (T)(object)Array.CreateInstance(type.GetElementType(), 0) : (T)interpretedObject : (T)interpretedObject;
        }

        public object ToObject(string json) => ToObject(json, null);

        public object ToObject(string json, Type type)
        {
            Type actualType = type?.IsGenericType == true ? Reflection.Instance.GetGenericTypeDefinition(type) : null;

            UseGlobals = Configuration.UsingGlobalTypes && !(actualType == typeof(Dictionary<,>) || actualType == typeof(List<>));

            object data = new Parser(json, Configuration.AllowNonQuotedKeys).Decode();

            switch (data)
            {
                case object result when type == typeof(DataSet):
                    return CreateDataset(result as Dictionary<string, object>, null);
                case object result when type == typeof(DataTable):
                    return CreateDataTable(result as Dictionary<string, object>, null);
                case IDictionary result:
                    return type is Type && actualType == typeof(Dictionary<,>)
                        ? RootDictionary(result, type)
                        : ParseDictionary(result as Dictionary<string, object>, null, type, null);
                case List<object> result:
                    if (type is null)
                    {
                        if (result.Count > 0 && result[0].GetType() == typeof(Dictionary<string, object>))
                        {
                            Dictionary<string, object> globalTypes = new Dictionary<string, object> { };

                            // try to get $types
                            return result.Cast<Dictionary<string, object>>().Select(element => ParseDictionary(element, globalTypes, null, null)).ToList();
                        }
                        return result.ToArray();
                    }
                    else
                    {
                        if (actualType == typeof(Dictionary<,>)) // kv format
                            return RootDictionary(result, type);
                        else if (actualType == typeof(List<>)) // deserialize to generic list
                            return RootList(result, type);
                        else if (type.IsArray)
                            return RootArray(result, type);
                        else if (type == typeof(Hashtable))
                            return RootHashTable(result);
                    }
                    goto default;
                case object result when type is Type && result.GetType() != type:
                    return ChangeType(result, type);
                default:
                    return data;
            }
        }

        private object RootHashTable(List<object> tableData)
        {
            Hashtable h = new Hashtable { };

            foreach (Dictionary<string, object> valueSet in tableData)
                h.Add(valueSet["k"] is Dictionary<string, object> key ? ParseDictionary(key, null, typeof(object), null) : valueSet["k"], valueSet["v"] is Dictionary<string, object> value ? ParseDictionary(value, null, typeof(object), null) : valueSet["v"]);

            return h;
        }

        private object ChangeType(object value, Type conversionType)
        {
            if (conversionType == typeof(int))
                return !(value is string s) ? (int)(long)value : (object)Helper.CreateInteger(s, 0, s.Length);

            else if (conversionType == typeof(long))
                return !(value is string s) ? (long)value : (object)Helper.CreateLong(s, 0, s.Length);

            else if (conversionType == typeof(string))
                return (string)value;

            else if (conversionType.IsEnum)
                return Helper.CreateEnum(conversionType, value);

            else if (conversionType == typeof(DateTime))
                return Helper.CreateDateTime((string)value, Configuration.UseUTCDateTime);

            else if (conversionType == typeof(DateTimeOffset))
                return Helper.CreateDateTimeOffset((string)value);

            else if (Reflection.Instance.IsTypeRegistered(conversionType))
                return Reflection.Instance.CreateCustom((string)value, conversionType);

            // 8-30-2014 - James Brooks - Added code for nullable types.
            if (Helper.IsNullable(conversionType))
            {
                if (value == null)
                    return value;
                conversionType = Helper.UnderlyingTypeOf(conversionType);
            }

            // 8-30-2014 - James Brooks - Nullable Guid is a special case so it was moved after the "IsNullable" check.
            if (conversionType == typeof(Guid))
                return Helper.CreateGuid((string)value);

            // 2016-04-02 - Enrico Padovani - proper conversion of byte[] back from string
            return conversionType == typeof(byte[])
                ? Convert.FromBase64String((string)value)
                : conversionType == typeof(TimeSpan)
                ? new TimeSpan((long)value)
                : Convert.ChangeType(value, conversionType, CultureInfo.InvariantCulture);
        }

        private object RootList(object target, Type type)
        {
            IList root = Reflection.Instance.FastCreateList(type, (target as IList).Count) as IList;
            ParseList(target as IList, Reflection.Instance.GetGenericArguments(type)[0], root);
            return root;
        }

        private void ParseList(IList target, Type it, IList root)
        {
            Dictionary<string, object> globals = new Dictionary<string, object> { };

            foreach (object key in target)
            {
                UseGlobals = false; // NOTE: This is set in each iteration because race conditions are more likely to be triggered if a different thread sets it to false.
                Dictionary<string, object> a = key as Dictionary<string, object>;

                root.Add(a != null ? ParseDictionary(a, globals, it, null) : ChangeType(key, it));
            }
        }

        private object RootArray(object target, Type type)
        {
            Type elementType = type.GetElementType();
            IList root = Reflection.Instance.FastCreateInstance(typeof(List<>).MakeGenericType(elementType)) as IList;
            ParseList(target as IList, elementType, root);
            Array result = Array.CreateInstance(elementType, root.Count);
            root.CopyTo(result, 0);

            return result;
        }

        private object RootDictionary(object target, Type type)
        {
            Type[] genericTypes = Reflection.Instance.GetGenericArguments(type);
            Type keyType = genericTypes[0], valueType = genericTypes[1];

            switch (target)
            {
                case Dictionary<string, object> dictionaryTarget:
                    IDictionary root = Reflection.Instance.FastCreateInstance(type) as IDictionary;

                    foreach (KeyValuePair<string, object> pair in target as Dictionary<string, object>)
                    {
                        object value = default;

                        if (valueType.Name.StartsWith("Dictionary")) // deserialize a dictionary
                            value = RootDictionary(pair.Value, valueType);
                        else
                            switch (pair.Value)
                            {
                                case Dictionary<string, object> rawValue:
                                    value = ParseDictionary(rawValue, null, valueType, null);
                                    break;
                                case List<object> rawValue when valueType.IsArray && valueType != typeof(byte[]):
                                    value = CreateArray(rawValue, valueType, valueType.GetElementType(), null);
                                    break;
                                case List<object> rawValue:
                                    value = CreateGenericList(rawValue, valueType, keyType, null);
                                    break;
                                default:
                                    value = ChangeType(pair.Value, valueType);
                                    break;
                            }

                        root.Add(ChangeType(pair.Key, keyType), value);
                    }

                    return root;
                case List<object> listTarget:
                    return CreateDictionary(listTarget as List<object>, type, genericTypes, null);
                default:
                    return null;
            }
        }

        internal object ParseDictionary(Dictionary<string, object> target, Dictionary<string, object> globalTypes, Type type, object input)
        {
            if (type == typeof(NameValueCollection))
                return Helper.CreateNV(target);
            if (type == typeof(StringDictionary))
                return Helper.CreateSD(target);

            if (target.TryGetValue("$i", out object tn))
            {
                ReverseCircularObject.TryGetValue((int)(long)tn, out object v);
                return v;
            }

            if (target.TryGetValue("$types", out tn))
            {
                UseGlobals = true;
                if (globalTypes == null)
                    globalTypes = new Dictionary<string, object>();
                foreach (KeyValuePair<string, object> kv in (Dictionary<string, object>)tn)
                {
                    globalTypes.Add((string)kv.Value, kv.Key);
                }
            }
            if (globalTypes != null)
                UseGlobals = true;

            bool found = target.TryGetValue("$type", out tn);
            if (found == false && type == typeof(object))
            {
                return target;   // CreateDataset(d, globaltypes);
            }
            if (found)
            {
                if (UseGlobals)
                {
                    if (globalTypes != null && globalTypes.TryGetValue((string)tn, out object tname))
                        tn = tname;
                }
                type = Reflection.Instance.GetTypeFromCache((string)tn);
            }

            if (type == null)
                throw new Exception("Cannot determine type");

            string typename = type.FullName;
            object o = input;
            if (o == null)
            {
                o = Configuration.ParametricConstructorOverride
                    ? System.Runtime.Serialization.FormatterServices.GetUninitializedObject(type)
                    : Reflection.Instance.FastCreateInstance(type);
            }
            if (CircularObject.TryGetValue(o, out int circount) == false)
            {
                circount = CircularObject.Count + 1;
                CircularObject.Add(o, circount);
                ReverseCircularObject.Add(circount, o);
            }

            Dictionary<string, PropInfo> props = Reflection.Instance.Getproperties(type, typename, Configuration.ShowReadOnlyProperties);
            foreach (KeyValuePair<string, object> kv in target)
            {
                string n = kv.Key;
                object v = kv.Value;

                string name = n;//.ToLower();
                if (name == "$map")
                {
                    ProcessMap(o, props, (Dictionary<string, object>)target[name]);
                    continue;
                }
                if (props.TryGetValue(name, out PropInfo pi) == false)
                    if (props.TryGetValue(name.ToLowerInvariant(), out pi) == false)
                        continue;

                if (pi.CanWrite)
                {
                    if (v != null)
                    {
                        object oset = null;

                        switch (pi.Type)
                        {
                            case PropInfoType.Int: oset = (int)Helper.AutoConv(v); break;
                            case PropInfoType.Long: oset = Helper.AutoConv(v); break;
                            case PropInfoType.String: oset = v.ToString(); break;
                            case PropInfoType.Bool: oset = Helper.BoolConv(v); break;
                            case PropInfoType.DateTime: oset = Helper.CreateDateTime((string)v, Configuration.UseUTCDateTime); break;
                            case PropInfoType.Enum: oset = Helper.CreateEnum(pi.pt, v); break;
                            case PropInfoType.Guid: oset = Helper.CreateGuid((string)v); break;
                            case PropInfoType.Array:
                                if (!pi.IsValueType)
                                    oset = CreateArray((List<object>)v, pi.pt, pi.bt, globalTypes);
                                // what about 'else'?
                                break;
                            case PropInfoType.ByteArray: oset = Convert.FromBase64String((string)v); break;
                            case PropInfoType.DataSet: oset = CreateDataset((Dictionary<string, object>)v, globalTypes); break;
                            case PropInfoType.DataTable: oset = CreateDataTable((Dictionary<string, object>)v, globalTypes); break;
                            case PropInfoType.Hashtable: // same case as Dictionary
                            case PropInfoType.Dictionary: oset = CreateDictionary((List<object>)v, pi.pt, pi.GenericTypes, globalTypes); break;
                            case PropInfoType.StringKeyDictionary: oset = CreateStringKeyDictionary((Dictionary<string, object>)v, pi.pt, pi.GenericTypes, globalTypes); break;
                            case PropInfoType.NameValue: oset = Helper.CreateNV((Dictionary<string, object>)v); break;
                            case PropInfoType.StringDictionary: oset = Helper.CreateSD((Dictionary<string, object>)v); break;
                            case PropInfoType.Custom: oset = Reflection.Instance.CreateCustom((string)v, pi.pt); break;
                            default:
                            {
                                oset = pi.IsGenericType && pi.IsValueType == false && v is List<object>
                                    ? CreateGenericList((List<object>)v, pi.pt, pi.bt, globalTypes)
                                    : (pi.IsClass || pi.IsStruct || pi.IsInterface) && v is Dictionary<string, object>
                                    ? ParseDictionary((Dictionary<string, object>)v, globalTypes, pi.pt, null)
                                    : v is List<object>
                                    ? CreateArray((List<object>)v, pi.pt, typeof(object), globalTypes)
                                    : pi.IsValueType ? ChangeType(v, pi.changeType) : v;
                            }
                            break;
                        }

                        o = pi.setter(o, oset);
                    }
                }
            }
            return o;
        }

        private static void ProcessMap(object obj, Dictionary<string, PropInfo> props, Dictionary<string, object> dic)
        {
            foreach (KeyValuePair<string, object> kv in dic)
            {
                PropInfo p = props[kv.Key];
                object o = p.getter(obj);
                Type t = Type.GetType((string)kv.Value);
                if (t == typeof(Guid))
                    p.setter(obj, Helper.CreateGuid((string)o));
            }
        }

        private object CreateArray(List<object> data, Type pt, Type bt, Dictionary<string, object> globalTypes)
        {
            if (bt == null)
                bt = typeof(object);

            Array col = Array.CreateInstance(bt, data.Count);
            Type arraytype = bt.GetElementType();
            // create an array of objects
            for (int i = 0; i < data.Count; i++)
            {
                object ob = data[i];
                if (ob == null)
                {
                    continue;
                }
                if (ob is IDictionary)
                    col.SetValue(ParseDictionary((Dictionary<string, object>)ob, globalTypes, bt, null), i);
                else if (ob is ICollection)
                    col.SetValue(CreateArray((List<object>)ob, bt, arraytype, globalTypes), i);
                else
                    col.SetValue(ChangeType(ob, bt), i);
            }

            return col;
        }

        private object CreateGenericList(List<object> data, Type pt, Type bt, Dictionary<string, object> globalTypes)
        {
            if (pt != typeof(object))
            {
                IList col = (IList)Reflection.Instance.FastCreateList(pt, data.Count);
                Type it = Reflection.Instance.GetGenericArguments(pt)[0];// pt.GetGenericArguments()[0];
                // create an array of objects
                foreach (object ob in data)
                {
                    if (ob is IDictionary)
                        col.Add(ParseDictionary((Dictionary<string, object>)ob, globalTypes, it, null));

                    else if (ob is List<object>)
                    {
                        if (bt.IsGenericType)
                            col.Add((List<object>)ob);//).ToArray());
                        else
                            col.Add(((List<object>)ob).ToArray());
                    }
                    else
                        col.Add(ChangeType(ob, it));
                }
                return col;
            }
            return data;
        }

        private object CreateStringKeyDictionary(Dictionary<string, object> reader, Type pt, Type[] types, Dictionary<string, object> globalTypes)
        {
            IDictionary col = (IDictionary)Reflection.Instance.FastCreateInstance(pt);
            Type arraytype = null;
            Type t2 = null;
            if (types != null)
                t2 = types[1];

            Type generictype = null;
            Type[] ga = Reflection.Instance.GetGenericArguments(t2);// t2.GetGenericArguments();
            if (ga.Length > 0)
                generictype = ga[0];
            arraytype = t2.GetElementType();

            foreach (KeyValuePair<string, object> values in reader)
            {
                col.Add(values.Key, values.Value is Dictionary<string, object>
                    ? ParseDictionary((Dictionary<string, object>)values.Value, globalTypes, t2, null)
                    : types != null && t2.IsArray
                        ? values.Value is Array ? values.Value : CreateArray((List<object>)values.Value, t2, arraytype, globalTypes)
                        : values.Value is IList
                                        ? CreateGenericList((List<object>)values.Value, t2, generictype, globalTypes)
                                        : ChangeType(values.Value, t2));
            }

            return col;
        }

        private object CreateDictionary(List<object> reader, Type pt, Type[] types, Dictionary<string, object> globalTypes)
        {
            IDictionary col = (IDictionary)Reflection.Instance.FastCreateInstance(pt);
            Type t1 = null;
            Type t2 = null;
            if (types != null)
            {
                t1 = types[0];
                t2 = types[1];
            }

            foreach (Dictionary<string, object> values in reader)
            {
                object key = values["k"];
                object val = values["v"];

                key = key is Dictionary<string, object>
                    ? ParseDictionary((Dictionary<string, object>)key, globalTypes, t1, null)
                    : ChangeType(key, t1);

                val = typeof(IDictionary).IsAssignableFrom(t2)
                    ? RootDictionary(val, t2)
                    : val is Dictionary<string, object>
                    ? ParseDictionary((Dictionary<string, object>)val, globalTypes, t2, null)
                    : ChangeType(val, t2);

                col.Add(key, val);
            }

            return col;
        }

        private DataSet CreateDataset(Dictionary<string, object> reader, Dictionary<string, object> globalTypes)
        {
            DataSet ds = new DataSet
            {
                EnforceConstraints = false
            };
            ds.BeginInit();

            // read dataset schema here
            object schema = reader["$schema"];

            if (schema is string)
            {
                TextReader tr = new StringReader((string)schema);
                ds.ReadXmlSchema(tr);
            }
            else
            {
                Schema ms = (Schema)ParseDictionary((Dictionary<string, object>)schema, globalTypes, typeof(Schema), null);
                ds.DataSetName = ms.Name;
                for (int i = 0; i < ms.Info.Count; i += 3)
                {
                    if (ds.Tables.Contains(ms.Info[i]) == false)
                        ds.Tables.Add(ms.Info[i]);
                    ds.Tables[ms.Info[i]].Columns.Add(ms.Info[i + 1], Type.GetType(ms.Info[i + 2]));
                }
            }

            foreach (KeyValuePair<string, object> pair in reader)
            {
                if (pair.Key == "$type" || pair.Key == "$schema") continue;

                List<object> rows = (List<object>)pair.Value;
                if (rows == null) continue;

                DataTable dt = ds.Tables[pair.Key];
                ReadDataTable(rows, dt);
            }

            ds.EndInit();

            return ds;
        }

        private void ReadDataTable(List<object> rows, DataTable dt)
        {
            dt.BeginInit();
            dt.BeginLoadData();
            List<int> guidcols = new List<int>();
            List<int> datecol = new List<int>();
            List<int> bytearraycol = new List<int>();

            foreach (DataColumn c in dt.Columns)
            {
                if (c.DataType == typeof(Guid) || c.DataType == typeof(Guid?))
                    guidcols.Add(c.Ordinal);
                if (Configuration.UseUTCDateTime && (c.DataType == typeof(DateTime) || c.DataType == typeof(DateTime?)))
                    datecol.Add(c.Ordinal);
                if (c.DataType == typeof(byte[]))
                    bytearraycol.Add(c.Ordinal);
            }

            foreach (List<object> row in rows)
            {
                object[] v = new object[row.Count];
                row.CopyTo(v, 0);
                foreach (int i in guidcols)
                {
                    string s = (string)v[i];
                    if (s != null && s.Length < 36)
                        v[i] = new Guid(Convert.FromBase64String(s));
                }
                foreach (int i in bytearraycol)
                {
                    string s = (string)v[i];
                    if (s != null)
                        v[i] = Convert.FromBase64String(s);
                }
                if (Configuration.UseUTCDateTime)
                {
                    foreach (int i in datecol)
                    {
                        string s = (string)v[i];
                        if (s != null)
                            v[i] = Helper.CreateDateTime(s, Configuration.UseUTCDateTime);
                    }
                }
                dt.Rows.Add(v);
            }

            dt.EndLoadData();
            dt.EndInit();
        }

        DataTable CreateDataTable(Dictionary<string, object> reader, Dictionary<string, object> globalTypes)
        {
            DataTable dt = new DataTable();

            // read dataset schema here
            object schema = reader["$schema"];

            if (schema is string)
            {
                TextReader tr = new StringReader((string)schema);
                dt.ReadXmlSchema(tr);
            }
            else
            {
                Schema ms = (Schema)ParseDictionary((Dictionary<string, object>)schema, globalTypes, typeof(Schema), null);
                dt.TableName = ms.Info[0];
                for (int i = 0; i < ms.Info.Count; i += 3)
                {
                    dt.Columns.Add(ms.Info[i + 1], Type.GetType(ms.Info[i + 2]));
                }
            }

            foreach (KeyValuePair<string, object> pair in reader)
            {
                if (pair.Key == "$type" || pair.Key == "$schema")
                    continue;

                List<object> rows = (List<object>)pair.Value;
                if (rows == null)
                    continue;

                if (!dt.TableName.Equals(pair.Key, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                ReadDataTable(rows, dt);
            }

            return dt;
        }
    }
}