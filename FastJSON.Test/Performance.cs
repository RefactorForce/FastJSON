using FastJSON.Test.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Data;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using static FastJSON.Test.Resources.Utilities;

namespace FastJSON.Test
{
    [TestClass]
    public class Performance
    {
        [ClassInitialize]
        public static void Initialize(TestContext test) => Data = CreateDataset();

        static DataSet Data { get; set; }

        static int Count { get; set; } = 1000;

        [TestMethod]
        public void a_new_serializer()
        {
            DateTime st = DateTime.Now;
            Column c;
            string jsonText = null;
            c = CreateObject();
            for (int i = 0; i < Count; i++)
            {
                jsonText = Utility.ToJSON(c);
            }
            
            st.LogDifference();
        }

        [TestMethod]
        public void b_new_deserializer()
        {
            DateTime st = DateTime.Now;
            Column c;
            string jsonText = null;
            c = CreateObject();
            object o;
            jsonText = Utility.ToJSON(c);
            for (int i = 0; i < Count; i++)
            {
                o = Utility.ToObject(jsonText);
            }

            st.LogDifference();
        }

        [TestMethod]
        public void a_Stack_Serializer()
        {
            DateTime st = DateTime.Now;
            Column c;
            string jsonText = null;
            c = CreateObject();
            for (int i = 0; i < Count; i++)
            {
                jsonText = ServiceStack.Text.JsonSerializer.SerializeToString(c);
            }

            st.LogDifference();
        }


        [TestMethod]
        public void a_Lit_Serializer()
        {
            DateTime st = DateTime.Now;
            Column c;
            string jsonText = null;
            c = CreateObject();
            for (int i = 0; i < Count; i++)
            {
                jsonText = Utility.ToJSON(c);
            }
            
            st.LogDifference();
        }

        [TestMethod]
        public void a_nJson_Serializer()
        {
            DateTime st = DateTime.Now;
            Column c;
            JsonSerializerSettings s = null;
            string jsonText = null;
            s = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
            c = CreateObject();

            for (int i = 0; i < Count; i++)
            {
                jsonText = JsonConvert.SerializeObject(c, Formatting.Indented, s);
            }

            st.LogDifference();
        }


        [TestMethod]
        public void b_nJson_DeSerializer()
        {
            DateTime st = DateTime.Now;
            Column c;
            Column deserializedStore = null;
            JsonSerializerSettings s = null;
            string jsonText = null;
            c = CreateObject();
            s = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
            jsonText = JsonConvert.SerializeObject(c, Formatting.Indented, s);
            for (int i = 0; i < Count; i++)
            {
                deserializedStore = (Column)JsonConvert.DeserializeObject(jsonText, typeof(Column), s);
            }

            st.LogDifference();
        }

        [TestMethod]
        public void b_bin_DeSerializer()
        {
            DateTime st = DateTime.Now;
            Column c;
            Column deserializedStore = null;
            c = CreateObject();
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, c);

            for (int i = 0; i < Count; i++)
            {
                ms.Seek(0L, SeekOrigin.Begin);
                deserializedStore = (Column)bf.Deserialize(ms);
            }

            st.LogDifference();
        }

        [TestMethod]
        public void a_bin_Serializer()
        {
            DateTime st = DateTime.Now;
            Column c;
            c = CreateObject();
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            for (int i = 0; i < Count; i++)
            {
                ms = new MemoryStream();
                bf.Serialize(ms, c);
            }

            st.LogDifference();
        }

        [TestMethod]
        public void b_Stack_DeSerializer()
        {
            DateTime st = DateTime.Now;
            Column c;
            Column deserializedStore = null;
            string jsonText = null;
            c = CreateObject();
            jsonText = ServiceStack.Text.JsonSerializer.SerializeToString(c);
            for (int i = 0; i < Count; i++)
            {
                deserializedStore = ServiceStack.Text.JsonSerializer.DeserializeFromString<Column>(jsonText);
            }

            st.LogDifference();
        }

        [TestMethod]
        public void b_Lit_DeSerializer()
        {
            DateTime st = DateTime.Now;
            Column c;
            Column deserializedStore = null;
            string jsonText = null;
            c = CreateObject();
            jsonText = Utility.ToJSON(c);
            for (int i = 0; i < Count; i++)
            {
                deserializedStore = (Column)Utility.ToObject(jsonText);
            }

            st.LogDifference();
        }

        public static DataSet CreateDataset()
        {
            DataSet ds = new DataSet();
            for (int j = 1; j < 3; j++)
            {
                DataTable dt = new DataTable
                {
                    TableName = "Table" + j
                };
                dt.Columns.Add("col1", typeof(int));
                dt.Columns.Add("col2", typeof(string));
                dt.Columns.Add("col3", typeof(Guid));
                dt.Columns.Add("col4", typeof(string));
                dt.Columns.Add("col5", typeof(bool));
                dt.Columns.Add("col6", typeof(string));
                dt.Columns.Add("col7", typeof(string));
                ds.Tables.Add(dt);
                Random rrr = new Random();
                for (int i = 0; i < 100; i++)
                {
                    DataRow dr = dt.NewRow();
                    dr[0] = rrr.Next(Int32.MaxValue);
                    dr[1] = "" + rrr.Next(Int32.MaxValue);
                    dr[2] = Guid.NewGuid();
                    dr[3] = "" + rrr.Next(Int32.MaxValue);
                    dr[4] = true;
                    dr[5] = "" + rrr.Next(Int32.MaxValue);
                    dr[6] = "" + rrr.Next(Int32.MaxValue);

                    dt.Rows.Add(dr);
                }
            }
            return ds;
        }
    }
}
