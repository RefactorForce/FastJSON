using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using static FastJSON.ConsoleTest.DataObjects;

namespace FastJSON.ConsoleTest
{
    class Program
    {
        public static int Count { get; } = 1000;

        public static int TCount { get; } = 5;

        public static DataSet Data { get; set; } = new DataSet();

        public static bool Exotic { get; set; } = false;

        public static bool DSSER { get; set; } = false;

        private static void fastjson_deserialize(int count)
        {
            Console.WriteLine();
            Console.WriteLine("fastjson deserialize");
            List<double> times = new List<double>();
            List<TestClass> data = TestClass.CreateList(20000);
            string jsonText = Utility.ToJSON(data, new Parameters { UseExtensions = false });

            Stopwatch s = new Stopwatch();
            for (int tests = 0; tests < count; tests++)
            {
                s.Start();
                List<TestClass> result = Utility.ToObject<List<TestClass>>(jsonText);
                s.Stop();
                times.Add(s.ElapsedMilliseconds);
                s.Reset();
                if (tests % 10 == 0)
                    Console.Write(".");
            }

            Console.WriteLine();
            Console.WriteLine($"Min: {times.Min()} Max: {times.Max()} Average: {times.Average()}");
        }

        private static void fastjson_serialize(int count)
        {
            Console.WriteLine();
            Console.WriteLine("fastjson serialize");
            List<double> times = new List<double>();
            List<TestClass> data = TestClass.CreateList(20000);
            for (int tests = 0; tests < count; tests++)
            {
                DateTime st = DateTime.Now;
                string jsonText = Utility.ToJSON(data, new Parameters { UseExtensions = false });

                times.Add(DateTime.Now.Subtract(st).TotalMilliseconds);
                if (tests % 10 == 0)
                    Console.Write(".");
            }
            Console.WriteLine();
            Console.WriteLine($"Min: {times.Min()} Max: {times.Max()} Average: {times.Average()}");

        }

        public static void Main(string[] args)
        {   Console.WriteLine($".NET Version {Environment.Version}.");
            Console.WriteLine("Press E to enable (E)xotic mode, otherwise, press any other key.");
            if (Console.ReadKey().Key == ConsoleKey.E)
                Exotic = true;

            Data = CreateDataset();
            Console.WriteLine("-dataset");
            DSSER = false;
            fastjson_serialize();
            fastjson_deserialize();

            DSSER = true;
            Console.WriteLine();
            Console.WriteLine("+dataset");
            fastjson_serialize();
            fastjson_deserialize();

            Console.WriteLine();
        }

        public static Column CreateObject()
        {
            Column c = new Column
            {
                BooleanValue = true,
                OrdinaryDecimal = 3
            };

            if (Exotic)
            {
                c.NullableGuid = Guid.NewGuid();
                c.Hash = new Hashtable();
                c.Bytes = new byte[1024];
                c.StringDictionary = new Dictionary<string, Base>();
                c.ObjectDictionary = new Dictionary<Base, Base>();
                c.IntDictionary = new Dictionary<int, Base>();
                c.NullableDouble = 100.003;

                if (DSSER)
                    c.Dataset = Data;
                c.NullableDecimal = 3.14M;

                c.Hash.Add(new ClassA("0", "hello", Guid.NewGuid()), new ClassB("1", "code", "desc"));
                c.Hash.Add(new ClassB("0", "hello", "pppp"), new ClassA("1", "code", Guid.NewGuid()));

                c.StringDictionary.Add("name1", new ClassB("1", "code", "desc"));
                c.StringDictionary.Add("name2", new ClassA("1", "code", Guid.NewGuid()));

                c.IntDictionary.Add(1, new ClassB("1", "code", "desc"));
                c.IntDictionary.Add(2, new ClassA("1", "code", Guid.NewGuid()));

                c.ObjectDictionary.Add(new ClassA("0", "hello", Guid.NewGuid()), new ClassB("1", "code", "desc"));
                c.ObjectDictionary.Add(new ClassB("0", "hello", "pppp"), new ClassA("1", "code", Guid.NewGuid()));

                c.ArrayType = new Base[2];
                c.ArrayType[0] = new ClassA();
                c.ArrayType[1] = new ClassB();
            }


            c.Items.Add(new ClassA("1", "1", Guid.NewGuid()));
            c.Items.Add(new ClassB("2", "2", "desc1"));
            c.Items.Add(new ClassA("3", "3", Guid.NewGuid()));
            c.Items.Add(new ClassB("4", "4", "desc2"));

            c.Laststring = "" + DateTime.Now;

            return c;
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

        private static void fastjson_deserialize()
        {
            Console.WriteLine();
            Console.Write("fastjson deserialize");
            Column c = CreateObject();
            Stopwatch stopwatch = new Stopwatch();
            for (int pp = 0; pp < TCount; pp++)
            {
                Column deserializedStore;
                string jsonText = null;

                stopwatch.Restart();
                jsonText = Utility.ToJSON(c);

                for (int i = 0; i < Count; i++)
                {
                    deserializedStore = (Column)Utility.ToObject(jsonText);
                }
                stopwatch.Stop();
                Console.Write("\t" + stopwatch.ElapsedMilliseconds);
            }
        }

        private static void fastjson_serialize()
        {
            Console.WriteLine();
            Console.Write("fastjson serialize");
            Column c = CreateObject();
            Stopwatch stopwatch = new Stopwatch();
            for (int pp = 0; pp < TCount; pp++)
            {
                string jsonText = null;
                stopwatch.Restart();
                for (int i = 0; i < Count; i++)
                {
                    jsonText = Utility.ToJSON(c);
                }
                stopwatch.Stop();
                Console.Write("\t" + stopwatch.ElapsedMilliseconds);
            }
        }

        private static void bin_deserialize()
        {
            Console.WriteLine();
            Console.Write("bin deserialize");
            Column c = CreateObject();
            Stopwatch stopwatch = new Stopwatch();
            for (int pp = 0; pp < TCount; pp++)
            {
                BinaryFormatter bf = new BinaryFormatter();
                MemoryStream ms = new MemoryStream();
                Column deserializedStore = null;
                stopwatch.Restart();
                bf.Serialize(ms, c);

                for (int i = 0; i < Count; i++)
                {
                    stopwatch.Stop(); // we stop then resume the stopwatch here so we don't factor in Seek()'s execution
                    ms.Seek(0L, SeekOrigin.Begin);
                    stopwatch.Start();
                    deserializedStore = (Column)bf.Deserialize(ms);
                }
                stopwatch.Stop();
                Console.Write("\t" + stopwatch.ElapsedMilliseconds);
            }
        }

        private static void bin_serialize()
        {
            Console.Write("\r\nbin serialize");
            Column c = CreateObject();
            Stopwatch stopwatch = new Stopwatch();
            for (int pp = 0; pp < TCount; pp++)
            {
                BinaryFormatter bf = new BinaryFormatter();
                MemoryStream ms = new MemoryStream();
                stopwatch.Restart();
                for (int i = 0; i < Count; i++)
                {
                    stopwatch.Stop(); // we stop then resume the stop watch here so we don't factor in the MemoryStream()'s execution
                    ms = new MemoryStream();
                    stopwatch.Start();
                    bf.Serialize(ms, c);
                }
                stopwatch.Stop();
                Console.Write("\t" + stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
