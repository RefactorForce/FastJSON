using FastJSON.Test.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Threading;

using static FastJSON.Test.Resources.Utilities;
using static FastJSON.Test.Resources.Miscellaneous;
using static FastJSON.Test.General;

namespace FastJSON.Test
{
    [TestClass]
    public class General
    {
        [ClassInitialize]
        public static void Initialize(TestContext context) => Utility.Parameters.FixValues();

        [TestMethod]
        public void RegenerateObjectArray()
        {
            object[] referenceData = new object[] { 1, "sdaffs", DateTime.Now };
            object[] interpretedData = Utility.ToObject<object[]>(Utility.ToJSON(referenceData));

            // Integer strings are always deserialized into 64-bit integers, regardless of their size.
            Assert.AreEqual(referenceData[0], (int)(long)interpretedData[0]);
            Assert.AreEqual(referenceData[1], interpretedData[1]);
            Assert.AreEqual(((DateTime)referenceData[2]).Date, Utility.ToObject<DateTime>($@"""{interpretedData[2]}""").Date);
        }

        [TestMethod]
        public void ClassTest()
        {
            ReturnClass target = new ReturnClass
            {
                Name = "hello",
                Field1 = "dsasdF",
                Field2 = 2312,
                Date = DateTime.Now,
                Data = CreateDataset().Tables[0]
            };

            Assert.AreEqual(target.Field1, Utility.ToObject<ReturnClass>(Utility.ToJSON(target)).Field1);
        }

        [TestMethod]
        public void StructTest()
        {
            ReturnStruct r = new ReturnStruct
            {
                Name = "hello",
                Field1 = "dsasdF",
                Field2 = 2312,
                Date = DateTime.Now,
                Data = CreateDataset().Tables[0]
            };

            string s = Utility.ToJSON(r);
            Console.WriteLine(s);
            object o = Utility.ToObject(s);
            Assert.IsNotNull(o);
            Assert.AreEqual(2312, ((ReturnStruct)o).Field2);
        }

        [TestMethod]
        public void ParseTest()
        {
            ReturnClass r = new ReturnClass
            {
                Name = "hello",
                Field1 = "dsasdF",
                Field2 = 2312,
                Date = DateTime.Now,
                Data = CreateDataset().Tables[0]
            };

            string s = Utility.ToJSON(r);
            Console.WriteLine(s);
            object o = Utility.Parse(s);

            Assert.IsNotNull(o);
        }

        [TestMethod]
        public void StringListTest()
        {
            List<string> ls = new List<string>();
            ls.AddRange(new string[] { "a", "b", "c", "d" });

            string s = Utility.ToJSON(ls);
            Console.WriteLine(s);
            object o = Utility.ToObject(s);

            Assert.IsNotNull(o);
        }

        [TestMethod]
        public void IntListTest()
        {
            List<int> ls = new List<int>();
            ls.AddRange(new int[] { 1, 2, 3, 4, 5, 10 });

            string s = Utility.ToJSON(ls);
            Console.WriteLine(s);
            object p = Utility.Parse(s);
            object o = Utility.ToObject(s); // long[] {1,2,3,4,5,10}

            Assert.IsNotNull(o);
        }

        [TestMethod]
        public void List_int()
        {
            List<int> ls = new List<int>();
            ls.AddRange(new int[] { 1, 2, 3, 4, 5, 10 });

            string s = Utility.ToJSON(ls);
            Console.WriteLine(s);
            object p = Utility.Parse(s);
            List<int> o = Utility.ToObject<List<int>>(s);

            Assert.IsNotNull(o);
        }

        [TestMethod]
        public void Variables()
        {
            string s = Utility.ToJSON(42);
            object o = Utility.ToObject(s);
            Assert.AreEqual(42L, o);

            s = Utility.ToJSON("hello");
            o = Utility.ToObject(s);
            Assert.AreEqual(o, "hello");

            s = Utility.ToJSON(42.42M);
            o = Utility.ToObject(s);
            Assert.AreEqual(42.42M, o);
        }

        [TestMethod]
        public void Dictionary_String_RetClass()
        {
            Dictionary<string, ReturnClass> r = new Dictionary<string, ReturnClass>
            {
                ["11"] = new ReturnClass { Field1 = "111", Field2 = 2, Date = DateTime.Now },
                ["12"] = new ReturnClass { Field1 = "111", Field2 = 2, Date = DateTime.Now }
            };

            string s = Utility.ToJSON(r);
            Console.WriteLine(Utility.Beautify(s));
            Dictionary<string, ReturnClass> o = Utility.ToObject<Dictionary<string, ReturnClass>>(s);
            Assert.AreEqual(2, o.Count);
        }

        [TestMethod]
        public void Dictionary_String_RetClass_noextensions()
        {
            Dictionary<string, ReturnClass> r = new Dictionary<string, ReturnClass>
            {
                ["11"] = new ReturnClass { Field1 = "111", Field2 = 2, Date = DateTime.Now },
                ["12"] = new ReturnClass { Field1 = "111", Field2 = 2, Date = DateTime.Now }
            };

            string s = Utility.ToJSON(r, new Parameters { UseExtensions = false });
            Console.WriteLine(Utility.Beautify(s));
            Dictionary<string, ReturnClass> o = Utility.ToObject<Dictionary<string, ReturnClass>>(s);
            Assert.AreEqual(2, o.Count);
        }

        [TestMethod]
        public void Dictionary_int_RetClass()
        {
            Dictionary<int, ReturnClass> r = new Dictionary<int, ReturnClass>
            {
                [11] = new ReturnClass { Field1 = "111", Field2 = 2, Date = DateTime.Now },
                [12] = new ReturnClass { Field1 = "111", Field2 = 2, Date = DateTime.Now }
            };

            string s = Utility.ToJSON(r);
            Console.WriteLine(Utility.Beautify(s));
            Dictionary<int, ReturnClass> o = Utility.ToObject<Dictionary<int, ReturnClass>>(s);
            Assert.AreEqual(2, o.Count);
        }

        [TestMethod]
        public void Dictionary_int_RetClass_noextensions()
        {
            Dictionary<int, ReturnClass> r = new Dictionary<int, ReturnClass>
            {
                [11] = new ReturnClass { Field1 = "111", Field2 = 2, Date = DateTime.Now },
                [12] = new ReturnClass { Field1 = "111", Field2 = 2, Date = DateTime.Now }
            };

            string s = Utility.ToJSON(r, new Parameters { UseExtensions = false });
            Console.WriteLine(Utility.Beautify(s));
            Dictionary<int, ReturnClass> o = Utility.ToObject<Dictionary<int, ReturnClass>>(s);
            Assert.AreEqual(2, o.Count);
        }

        [TestMethod]
        public void Dictionary_Retstruct_RetClass()
        {
            Dictionary<ReturnStruct, ReturnClass> r = new Dictionary<ReturnStruct, ReturnClass>
            {
                [new ReturnStruct { Field1 = "111", Field2 = 1, Date = DateTime.Now }] = new ReturnClass { Field1 = "111", Field2 = 2, Date = DateTime.Now },
                [new ReturnStruct { Field1 = "222", Field2 = 2, Date = DateTime.Now }] = new ReturnClass { Field1 = "111", Field2 = 2, Date = DateTime.Now }
            };

            string s = Utility.ToJSON(r);
            Console.WriteLine(Utility.Beautify(s));
            Dictionary<ReturnStruct, ReturnClass> o = Utility.ToObject<Dictionary<ReturnStruct, ReturnClass>>(s);
            Assert.AreEqual(2, o.Count);
        }

        [TestMethod]
        public void Dictionary_Retstruct_RetClass_noextentions()
        {
            Dictionary<ReturnStruct, ReturnClass> r = new Dictionary<ReturnStruct, ReturnClass>
            {
                [new ReturnStruct { Field1 = "111", Field2 = 1, Date = DateTime.Now }] = new ReturnClass { Field1 = "111", Field2 = 2, Date = DateTime.Now },
                [new ReturnStruct { Field1 = "222", Field2 = 2, Date = DateTime.Now }] = new ReturnClass { Field1 = "111", Field2 = 2, Date = DateTime.Now }
            };

            string s = Utility.ToJSON(r, new Parameters { UseExtensions = false });
            Console.WriteLine(Utility.Beautify(s));
            Dictionary<ReturnStruct, ReturnClass> o = Utility.ToObject<Dictionary<ReturnStruct, ReturnClass>>(s);
            Assert.AreEqual(2, o.Count);
        }

        [TestMethod]
        public void List_RetClass()
        {
            List<ReturnClass> r = new List<ReturnClass>
            {
                new ReturnClass { Field1 = "111", Field2 = 2, Date = DateTime.Now },
                new ReturnClass { Field1 = "222", Field2 = 3, Date = DateTime.Now }
            };

            string s = Utility.ToJSON(r);
            Console.WriteLine(Utility.Beautify(s));
            List<ReturnClass> o = Utility.ToObject<List<ReturnClass>>(s);
            Assert.AreEqual(2, o.Count);
        }

        [TestMethod]
        public void List_RetClass_noextensions()
        {
            List<ReturnClass> r = new List<ReturnClass>
            {
                new ReturnClass { Field1 = "111", Field2 = 2, Date = DateTime.Now },
                new ReturnClass { Field1 = "222", Field2 = 3, Date = DateTime.Now }
            };

            string s = Utility.ToJSON(r, new Parameters { UseExtensions = false });
            Console.WriteLine(Utility.Beautify(s));
            List<ReturnClass> o = Utility.ToObject<List<ReturnClass>>(s);
            Assert.AreEqual(2, o.Count);
        }

        [TestMethod]
        public void Perftest()
        {
            string s = "123456";

            DateTime dt = DateTime.Now;
            int c = 1000000;

            for (int i = 0; i < c; i++)
            {
                long o = CreateLong(s);
            }

            Console.WriteLine("convertlong (ms): " + DateTime.Now.Subtract(dt).TotalMilliseconds);

            dt = DateTime.Now;

            for (int i = 0; i < c; i++)
            {
                long o = Int64.Parse(s);
            }

            Console.WriteLine("long.parse (ms): " + DateTime.Now.Subtract(dt).TotalMilliseconds);

            dt = DateTime.Now;

            for (int i = 0; i < c; i++)
            {
                long o = Convert.ToInt64(s);
            }

            Console.WriteLine("convert.toint64 (ms): " + DateTime.Now.Subtract(dt).TotalMilliseconds);
        }

        [TestMethod]
        public void FillObject()
        {
            NoExtension ne = new NoExtension
            {
                Name = "hello",
                Address = "here",
                Age = 10,
                Dictionary = new Dictionary<string, ClassA>()
            };
            ne.Dictionary.Add("hello", new ClassA("asda", "asdas", Guid.NewGuid()));
            ne.Objects = new Base[] { new ClassA("a", "1", Guid.NewGuid()), new ClassB("b", "2", "desc") };

            string str = Utility.ToJSON(ne, new Parameters { UseExtensions = false, UsingGlobalTypes = false });
            string strr = Utility.Beautify(str);
            Console.WriteLine(strr);
            object dic = Utility.Parse(str);
            object oo = Utility.ToObject<NoExtension>(str);

            NoExtension nee = new NoExtension
            {
                InternalData = new NoExtension { Name = "aaa" }
            };
            Utility.FillObject(nee, strr);
        }

        [TestMethod]
        public void AnonymousTypes()
        {
            var q = new { Name = "asassa", Address = "asadasd", Age = 12 };
            string sq = Utility.ToJSON(q, new Parameters { EnableAnonymousTypes = true });
            Console.WriteLine(sq);
            Assert.AreEqual("{\"Name\":\"asassa\",\"Address\":\"asadasd\",\"Age\":12}", sq);
        }

        [TestMethod]
        public void Speed_Test_Deserialize()
        {
            Console.Write("fastjson deserialize");
            Utility.Parameters = new Parameters();
            Column c = CreateObject(false, false);
            double t = 0;
            Stopwatch stopwatch = new Stopwatch();
            for (int pp = 0; pp < Five; pp++)
            {
                stopwatch.Restart();
                Column deserializedStore;
                string jsonText = Utility.ToJSON(c);
                //Console.WriteLine(" size = " + jsonText.Length);
                for (int i = 0; i < OneThousand; i++)
                {
                    deserializedStore = (Column)Utility.ToObject(jsonText);
                }
                stopwatch.Stop();
                t += stopwatch.ElapsedMilliseconds;
                Console.Write("\t" + stopwatch.ElapsedMilliseconds);
            }
            Console.WriteLine("\tAVG = " + t / Five);
        }

        [TestMethod]
        public void Speed_Test_Serialize()
        {
            Console.Write("fastjson serialize");
            Utility.Parameters = new Parameters();
            //Utility.Parameters.UsingGlobalTypes = false;
            Column c = CreateObject(false, false);
            double t = 0;
            Stopwatch stopwatch = new Stopwatch();
            for (int pp = 0; pp < Five; pp++)
            {
                stopwatch.Restart();
                string jsonText = null;
                for (int i = 0; i < OneThousand; i++)
                {
                    jsonText = Utility.ToJSON(c);
                }
                stopwatch.Stop();
                t += stopwatch.ElapsedMilliseconds;
                Console.Write("\t" + stopwatch.ElapsedMilliseconds);
            }
            Console.WriteLine("\tAVG = " + t / Five);
        }

        [TestMethod]
        public void List_NestedRetClass() => Assert.AreEqual(2, Utility.ToObject<List<RetNestedclass>>(Utility.ToJSON(new List<RetNestedclass>
        {
            new RetNestedclass { Nested = new ReturnClass { Field1 = "111", Field2 = 2, Date = DateTime.Now } },
            new RetNestedclass { Nested = new ReturnClass { Field1 = "222", Field2 = 3, Date = DateTime.Now } }
        })).Count);

        [TestMethod]
        public void NullTest()
        {
            string s = Utility.ToJSON(null);
            Assert.AreEqual("null", s);
            object o = Utility.ToObject(s);
            Assert.AreEqual(null, o);
            o = Utility.ToObject<ClassA>(s);
            Assert.AreEqual(null, o);
        }

        [TestMethod]
        public void DisableExtensions()
        {
            Parameters p = new Parameters { UseExtensions = false, SerializeNullValues = false };
            string s = Utility.ToJSON(new ReturnClass { Date = DateTime.Now, Name = "aaaaaaa" }, p);
            Console.WriteLine(Utility.Beautify(s));
            ReturnClass o = Utility.ToObject<ReturnClass>(s);
            Assert.AreEqual("aaaaaaa", o.Name);
        }

        [TestMethod]
        public void ZeroArray()
        {
            string s = Utility.ToJSON(new object[] { });
            object o = Utility.ToObject(s);
            object[] a = o as object[];
            Assert.AreEqual(0, a.Length);
        }

        [TestMethod]
        public void BigNumber()
        {
            double d = 4.16366160299608e18;
            string s = Utility.ToJSON(d);
            double o = Utility.ToObject<double>(s);
            Assert.AreEqual(d, o);
        }

        [TestMethod]
        public void GermanNumbers()
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("de");
            decimal d = 3.141592654M;
            string s = Utility.ToJSON(d);
            decimal o = Utility.ToObject<decimal>(s);
            Assert.AreEqual(d, o);

            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en");
        }

        private void GenerateJsonForAandB(out string jsonA, out string jsonB)
        {
            Console.WriteLine("Begin constructing the original objects. Please ignore trace information until I'm done.");

            // set all parameters to false to produce pure JSON
            Utility.Parameters = new Parameters { EnableAnonymousTypes = false, SerializeNullValues = false, UseExtensions = false, UseFastGuid = false, UseOptimizedDatasetSchema = false, UseUTCDateTime = false, UsingGlobalTypes = false };

            ConcurrentClassA a = new ConcurrentClassA { PayloadA = new PayloadA() };
            ConcurrentClassB b = new ConcurrentClassB { PayloadB = new PayloadB() };

            // A is serialized with extensions and global types
            jsonA = Utility.ToJSON(a, new Parameters { EnableAnonymousTypes = false, SerializeNullValues = false, UseExtensions = true, UseFastGuid = false, UseOptimizedDatasetSchema = false, UseUTCDateTime = false, UsingGlobalTypes = true });
            // B is serialized using the above defaults
            jsonB = Utility.ToJSON(b);

            Console.WriteLine("Ok, I'm done constructing the objects. Below is the generated Utility. Trace messages that follow below are the result of deserialization and critical for understanding the timing.");
            Console.WriteLine(jsonA);
            Console.WriteLine(jsonB);
        }

        [TestMethod]
        public void UsingGlobalsBug_singlethread()
        {
            Parameters p = Utility.Parameters;
            GenerateJsonForAandB(out string jsonA, out string jsonB);

            object ax = Utility.ToObject(jsonA); // A has type information in JSON-extended
            ConcurrentClassB bx = Utility.ToObject<ConcurrentClassB>(jsonB); // B needs external type info

            Assert.IsNotNull(ax);
            Assert.IsInstanceOfType(ax, typeof(ConcurrentClassA));
            Assert.IsNotNull(bx);
            Assert.IsInstanceOfType(bx, typeof(ConcurrentClassB));
            Utility.Parameters = p;
        }

        [TestMethod]
        public void NullOutput() => Assert.IsFalse(Utility.ToJSON(new ConcurrentClassA(), new Parameters { UseExtensions = false, SerializeNullValues = false }).Contains(","));

        [TestMethod]
        public void UsingGlobalsBug_multithread()
        {
            Parameters p = Utility.Parameters;
            GenerateJsonForAandB(out string jsonA, out string jsonB);

            object ax = null;
            object bx = null;

            /*
             * Intended timing to force CannotGetType bug in 2.0.5:
             * the outer class ConcurrentClassA is deserialized first from json with extensions+global types. It reads the global types and sets _usingglobals to true.
             * The constructor contains a sleep to force parallel deserialization of ConcurrentClassB while in A's constructor.
             * The deserialization of B sets _usingglobals back to false.
             * After B is done, A continues to deserialize its PayloadA. It finds type "2" but since _usingglobals is false now, it fails with "Cannot get type".
             */

            Exception exception = null;

            Thread thread = new Thread(() =>
            {
                try
                {
                    Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " A begins deserialization");
                    ax = Utility.ToObject(jsonA); // A has type information in JSON-extended
                    Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " A is done");
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            });

            thread.Start();

            Thread.Sleep(500); // wait to allow A to begin deserialization first

            Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " B begins deserialization");
            bx = Utility.ToObject<ConcurrentClassB>(jsonB); // B needs external type info
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " B is done");

            Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " waiting for A to continue");
            thread.Join(); // wait for completion of A due to Sleep in A's constructor
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " threads joined.");

            Assert.IsNull(exception, exception == null ? "" : exception.Message + " " + exception.StackTrace);

            Assert.IsNotNull(ax);
            Assert.IsInstanceOfType(ax, typeof(ConcurrentClassA));
            Assert.IsNotNull(bx);
            Assert.IsInstanceOfType(bx, typeof(ConcurrentClassB));
            Utility.Parameters = p;
        }



        public class ConcurrentClassA
        {
            public ConcurrentClassA()
            {
                Console.WriteLine("ctor ConcurrentClassA. I will sleep for 2 seconds.");
                Thread.Sleep(2000);
                Thread.MemoryBarrier(); // just to be sure the caches on multi-core processors do not hide the bug. For me, the bug is present without the memory barrier, too.
                Console.WriteLine("ctor ConcurrentClassA. I am done sleeping.");
            }

            public PayloadA PayloadA { get; set; }
        }

        public class ConcurrentClassB
        {
            public ConcurrentClassB() => Console.WriteLine("ctor ConcurrentClassB.");

            public PayloadB PayloadB { get; set; }
        }

        public class PayloadA
        {
            public PayloadA() => Console.WriteLine("ctor PayLoadA.");
        }

        public class PayloadB
        {
            public PayloadB() => Console.WriteLine("ctor PayLoadB.");
        }

        public class commaclass
        {
            public string Name = "aaa";
        }

        public class arrayclass
        {
            public int[] ints { get; set; }
            public string[] strs;
        }
        [TestMethod]
        public void ArrayTest()
        {
            arrayclass a = new arrayclass
            {
                ints = new int[] { 3, 1, 4 },
                strs = new string[] { "a", "b", "c" }
            };
            string s = Utility.ToJSON(a);
            object o = Utility.ToObject(s);
        }
        
        [TestMethod]
        public void SingleCharNumber() => Assert.AreEqual((Int64)0, Utility.ToObject(Utility.ToJSON(0)));
        
        [TestMethod]
        public void Datasets()
        {
            DataSet ds = CreateDataset();

            string s = Utility.ToJSON(ds);

            DataSet o = Utility.ToObject<DataSet>(s);
            object p = Utility.ToObject(s, typeof(DataSet));

            Assert.AreEqual(typeof(DataSet), o.GetType());
            Assert.IsNotNull(o);
            Assert.AreEqual(2, o.Tables.Count);


            s = Utility.ToJSON(ds.Tables[0]);
            DataTable oo = Utility.ToObject<DataTable>(s);
            Assert.IsNotNull(oo);
            Assert.AreEqual(typeof(DataTable), oo.GetType());
            Assert.AreEqual(100, oo.Rows.Count);
        }


        [TestMethod]
        public void DynamicTest()
        {
            string s = "{\"Name\":\"aaaaaa\",\"Age\":10,\"dob\":\"2000-01-01 00:00:00Z\",\"inner\":{\"prop\":30},\"arr\":[1,{\"a\":2},3,4,5,6]}";
            dynamic d = Utility.ToDynamic(s);
            dynamic ss = d.Name;
            dynamic oo = d.Age;
            dynamic dob = d.dob;
            dynamic inp = d.inner.prop;
            dynamic i = d.arr[1].a;

            Assert.AreEqual("aaaaaa", ss);
            Assert.AreEqual(10, oo);
            Assert.AreEqual(30, inp);
            Assert.AreEqual("2000-01-01 00:00:00Z", dob);

            s = "{\"ints\":[1,2,3,4,5]}";

            d = Utility.ToDynamic(s);
            dynamic o = d.ints[0];
            Assert.AreEqual(1, o);

            s = "[1,2,3,4,5,{\"key\":90}]";
            d = Utility.ToDynamic(s);
            o = d[2];
            Assert.AreEqual(3, o);
            dynamic p = d[5].key;
            Assert.AreEqual(90, p);
        }

        [TestMethod]
        public void GetDynamicMemberNamesTests()
        {
            string s = "{\"Name\":\"aaaaaa\",\"Age\":10,\"dob\":\"2000-01-01 00:00:00Z\",\"inner\":{\"prop\":30},\"arr\":[1,{\"a\":2},3,4,5,6]}";
            dynamic d = Utility.ToDynamic(s);
            Assert.AreEqual(5, d.GetDynamicMemberNames().Count);
            Assert.AreEqual(6, d.arr.Count);
            Assert.AreEqual("aaaaaa", d["Name"]);
        }

        [TestMethod]
        public void CommaTests()
        {
            string s = Utility.ToJSON(new commaclass(), new Parameters());
            Console.WriteLine(Utility.Beautify(s));
            Assert.AreEqual(true, s.Contains("\"$type\":\"1\","));

            var objTest = new
            {
                A = "foo",
                B = (object)null,
                C = (object)null,
                D = "bar",
                E = 12,
                F = (object)null
            };

            Parameters p = new Parameters
            {
                EnableAnonymousTypes = true,
                SerializeNullValues = false,
                UseExtensions = false,
                UseFastGuid = true,
                UseOptimizedDatasetSchema = true,
                UseUTCDateTime = false,
                UsingGlobalTypes = false,
                UseEscapedUnicode = false
            };

            string json = Utility.ToJSON(objTest, p);
            Console.WriteLine(Utility.Beautify(json));
            Assert.AreEqual("{\"A\":\"foo\",\"D\":\"bar\",\"E\":12}", json);

            var o2 = new { A = "foo", B = "bar", C = (object)null };
            json = Utility.ToJSON(o2, p);
            Console.WriteLine(Utility.Beautify(json));
            Assert.AreEqual("{\"A\":\"foo\",\"B\":\"bar\"}", json);

            var o3 = new { A = (object)null };
            json = Utility.ToJSON(o3, p);
            Console.WriteLine(Utility.Beautify(json));
            Assert.AreEqual("{}", json);

            var o4 = new { A = (object)null, B = "foo" };
            json = Utility.ToJSON(o4, p);
            Console.WriteLine(Utility.Beautify(json));
            Assert.AreEqual("{\"B\":\"foo\"}", json);
        }

        [TestMethod]
        public void embedded_list()
        {
            string s = Utility.ToJSON(new { list = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, } });//.Where(i => i % 2 == 0) });
        }

        [TestMethod]
        public void Formatter()
        {
            string s = "[{\"foo\":\"'[0]\\\"{}\\u1234\\r\\n\",\"bar\":12222,\"coo\":\"some' string\",\"dir\":\"C:\\\\folder\\\\\"}]";
            string o = Utility.Beautify(s);
            Console.WriteLine(o);
            string x = @"[
   {
      ""foo"" : ""'[0]\""{}\u1234\r\n"",
      ""bar"" : 12222,
      ""coo"" : ""some' string"",
      ""dir"" : ""C:\\folder\\""
   }
]";
            Assert.AreEqual(x, o);
        }

        [TestMethod]
        public void EmptyArray()
        {
            string str = "[]";
            List<ClassA> o = Utility.ToObject<List<ClassA>>(str);
            Assert.AreEqual(typeof(List<ClassA>), o.GetType());
            ClassA[] d = Utility.ToObject<ClassA[]>(str);
            Assert.AreEqual(typeof(ClassA[]), d.GetType());
        }

        public class diclist
        {
            public Dictionary<string, List<string>> d;
        }

        [TestMethod]
        public void DictionaryWithListValue()
        {
            diclist dd = new diclist
            {
                d = new Dictionary<string, List<string>>()
            };
            dd.d.Add("a", new List<string> { "1", "2", "3" });
            dd.d.Add("b", new List<string> { "4", "5", "7" });
            string s = Utility.ToJSON(dd, new Parameters { UseExtensions = false });
            diclist o = Utility.ToObject<diclist>(s);
            Assert.AreEqual(3, o.d["a"].Count);

            s = Utility.ToJSON(dd.d, new Parameters { UseExtensions = false });
            Dictionary<string, List<string>> oo = Utility.ToObject<Dictionary<string, List<string>>>(s);
            Assert.AreEqual(3, oo["a"].Count);
            Dictionary<string, string[]> ooo = Utility.ToObject<Dictionary<string, string[]>>(s);
            Assert.AreEqual(3, ooo["b"].Length);
        }

        [TestMethod]
        public void HashtableTest()
        {
            Hashtable h = new Hashtable
            {
                [1] = "dsjfhksa",
                ["dsds"] = new ClassA { }
            };

            string s = Utility.ToNiceJSON(h, new Parameters());

            Hashtable o = Utility.ToObject<Hashtable>(s);
            Assert.AreEqual(typeof(Hashtable), o.GetType());
            Assert.AreEqual(typeof(ClassA), o["dsds"].GetType());
        }


        public abstract class AbstractClass
        {
            public string myConcreteType { get; set; }

            public AbstractClass() { }

            public AbstractClass(string type) => myConcreteType = type;
        }

        public abstract class AbstractClass<T> : AbstractClass
        {
            public T Value { get; set; }

            public AbstractClass() { }

            public AbstractClass(T value, string type) : base(type) => Value = value;
        }
        public class OneConcreteClass : AbstractClass<int>
        {
            public OneConcreteClass() { }

            public OneConcreteClass(int value) : base(value, "INT") { }
        }
        public class OneOtherConcreteClass : AbstractClass<string>
        {
            public OneOtherConcreteClass() { }

            public OneOtherConcreteClass(string value) : base(value, "STRING") { }
        }

        [TestMethod]
        public void AbstractTest()
        {
            OneConcreteClass intField = new OneConcreteClass(1);
            OneOtherConcreteClass stringField = new OneOtherConcreteClass("lol");
            List<AbstractClass> list = new List<AbstractClass>() { intField, stringField };

            string json = Utility.ToJSON(list);
            List<AbstractClass> objects = Utility.ToObject<List<AbstractClass>>(json);
        }

        [TestMethod]
        public void NestedDictionary()
        {
            Dictionary<string, int> dict = new Dictionary<string, int>
            {
                ["123"] = 12345
            };

            Dictionary<string, object> table = new Dictionary<string, object>
            {
                ["dict"] = dict
            };

            string st = Utility.ToJSON(table);
            Console.WriteLine(Utility.Beautify(st));
            Dictionary<string, object> tableDst = Utility.ToObject<Dictionary<string, object>>(st);
            Console.WriteLine(Utility.Beautify(Utility.ToJSON(tableDst)));
        }

        public class ignorecase
        {
            public string Name;
            public int Age;
        }
        public class ignorecase2
        {
            public string name;
            public int age;
        }
        [TestMethod]
        public void IgnoreCase()
        {
            string json = "{\"name\":\"aaaa\",\"age\": 42}";

            ignorecase o = Utility.ToObject<ignorecase>(json);
            Assert.AreEqual("aaaa", o.Name);
            ignorecase2 oo = Utility.ToObject<ignorecase2>(json.ToUpper());
            Assert.AreEqual("AAAA", oo.name);
        }

        public class coltest
        {
            public string name;
            public NameValueCollection nv;
            public StringDictionary sd;
        }

        [TestMethod]
        public void SpecialCollections()
        {
            NameValueCollection nv = new NameValueCollection
            {
                ["1"] = "a",
                ["2"] = "b"
            };
            string s = Utility.ToJSON(nv);
            NameValueCollection oo = Utility.ToObject<NameValueCollection>(s);
            Assert.AreEqual("a", oo["1"]);
            StringDictionary sd = new StringDictionary
            {
                ["1"] = "a",
                ["2"] = "b"
            };

            s = Utility.ToJSON(sd);
            StringDictionary o = Utility.ToObject<StringDictionary>(s);
            Assert.AreEqual("b", o["2"]);

            coltest c = new coltest
            {
                name = "aaa",
                nv = nv,
                sd = sd
            };
            s = Utility.ToJSON(c);
            object ooo = Utility.ToObject(s);
            Assert.AreEqual("a", (ooo as coltest).nv["1"]);
            Assert.AreEqual("b", (ooo as coltest).sd["2"]);
        }

        public class constch
        {
            public enumt e = enumt.B;
            public string Name = "aa";
            public const int age = 11;
        }

        [TestMethod]
        public void consttest()
        {
            string s = Utility.ToJSON(new constch());
            object o = Utility.ToObject(s);
        }


        public enum enumt
        {
            A = 65,
            B = 90,
            C = 100
        }

        [TestMethod]
        public void enumtest()
        {
            string s = Utility.ToJSON(new constch { }, new Parameters { UseValuesOfEnums = true });
            Console.WriteLine(s);
            object o = Utility.ToObject(s);
        }

        public class IgnoredAttribute : Attribute { }

        public class IgnoredAttributeTestClass
        {
            public string Name { get; set; }

            [System.Xml.Serialization.XmlIgnore]
            public int Age1 { get; set; }

            [Ignored]
            public int Age2;
        }
        public class IgnoredAttributeTestSubclass : IgnoredAttributeTestClass { }

        [TestMethod]
        public void IgnoreAttributes()
        {
            IgnoredAttributeTestClass i = new IgnoredAttributeTestClass { Age1 = 10, Age2 = 20, Name = "aa" };
            string s = Utility.ToJSON(i);
            Console.WriteLine(s);
            Assert.IsFalse(s.Contains("Age1"));
            i = new IgnoredAttributeTestSubclass { Age1 = 10, Age2 = 20, Name = "bb" };
            Parameters j = new Parameters();
            j.IgnoreAttributes.Add(typeof(IgnoredAttribute));
            s = Utility.ToJSON(i, j);
            Console.WriteLine(s);
            Assert.IsFalse(s.Contains("Age1"));
            Assert.IsFalse(s.Contains("Age2"));
        }

        public class NonDefaultConstructorTestClass
        {
            public NonDefaultConstructorTestClass(int age) => Age = age;

            public int Age { get; set; }
        }

        [TestMethod]
        public void NonDefaultConstructor()
        {
            NonDefaultConstructorTestClass o = new NonDefaultConstructorTestClass(10);
            string s = Utility.ToJSON(o);
            Console.WriteLine(s);
            Assert.AreEqual(10, Utility.ToObject<NonDefaultConstructorTestClass>(s, new Parameters { ParametricConstructorOverride = true, UsingGlobalTypes = true }).Age);
            Console.WriteLine("list of objects");
            s = Utility.ToJSON(new List<NonDefaultConstructorTestClass> { o, o, o });
            Console.WriteLine(s);
            List<NonDefaultConstructorTestClass> obj2 = Utility.ToObject<List<NonDefaultConstructorTestClass>>(s, new Parameters { ParametricConstructorOverride = true, UsingGlobalTypes = true });
            Assert.AreEqual(3, obj2.Count);
            Assert.AreEqual(10, obj2[1].Age);
        }

        delegate object CreateObj();

        static SafeDictionary<Type, CreateObj> _constrcache = new SafeDictionary<Type, CreateObj>();

        internal static object FastCreateInstance(Type objtype)
        {
            try
            {
                if (_constrcache.TryGetValue(objtype, out CreateObj c))
                {
                    return c();
                }
                else
                {
                    if (objtype.IsClass)
                    {
                        DynamicMethod dynMethod = new DynamicMethod("_fcc", objtype, null, true);
                        ILGenerator ilGen = dynMethod.GetILGenerator();
                        ilGen.Emit(OpCodes.Newobj, objtype.GetConstructor(Type.EmptyTypes));
                        ilGen.Emit(OpCodes.Ret);
                        c = (CreateObj)dynMethod.CreateDelegate(typeof(CreateObj));
                        _constrcache.Add(objtype, c);
                    }
                    else // structs
                    {
                        DynamicMethod dynMethod = new DynamicMethod("_fcs", typeof(object), null, true);
                        ILGenerator ilGen = dynMethod.GetILGenerator();
                        LocalBuilder lv = ilGen.DeclareLocal(objtype);
                        ilGen.Emit(OpCodes.Ldloca_S, lv);
                        ilGen.Emit(OpCodes.Initobj, objtype);
                        ilGen.Emit(OpCodes.Ldloc_0);
                        ilGen.Emit(OpCodes.Box, objtype);
                        ilGen.Emit(OpCodes.Ret);
                        c = (CreateObj)dynMethod.CreateDelegate(typeof(CreateObj));
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

        static SafeDictionary<Type, Func<object>> lamdic = new SafeDictionary<Type, Func<object>>();

        static object lambdaCreateInstance(Type type)
        {
            if (lamdic.TryGetValue(type, out Func<object> o))
                return o();
            else
            {
                o = Expression.Lambda<Func<object>>(
                   Expression.Convert(Expression.New(type), typeof(object)))
                   .Compile();
                lamdic.Add(type, o);
                return o();
            }
        }

        [TestMethod]
        public void CreateObjPerfTest()
        {
            int count = 100000;
            Console.WriteLine("count = " + count.ToString("#,#"));
            DateTime dt = DateTime.Now;
            for (int i = 0; i < count; i++)
            {
                object o = new Column();
            }
            Console.WriteLine("normal new T() time ms = " + DateTime.Now.Subtract(dt).TotalMilliseconds);

            dt = DateTime.Now;
            for (int i = 0; i < count; i++)
            {
                object o = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(Column));
            }
            Console.WriteLine("FormatterServices time ms = " + DateTime.Now.Subtract(dt).TotalMilliseconds);

            dt = DateTime.Now;
            for (int i = 0; i < count; i++)
            {
                object o = FastCreateInstance(typeof(Column));
            }
            Console.WriteLine("IL newobj time ms = " + DateTime.Now.Subtract(dt).TotalMilliseconds);

            dt = DateTime.Now;
            for (int i = 0; i < count; i++)
            {
                object o = lambdaCreateInstance(typeof(Column));
            }
            Console.WriteLine("lambda time ms = " + DateTime.Now.Subtract(dt).TotalMilliseconds);
        }


        public class o1
        {
            public int o1int;
            public o2 o2obj;
            public o3 child;
        }
        public class o2
        {
            public int o2int;
            public o1 parent;
        }
        public class o3
        {
            public int o3int;
            public o2 child;
        }


        [TestMethod]
        public void CircularReferences()
        {
            o1 o = new o1 { o1int = 1, child = new o3 { o3int = 3 }, o2obj = new o2 { o2int = 2 } };
            o.o2obj.parent = o;
            o.child.child = o.o2obj;

            string s = Utility.ToJSON(o, new Parameters());
            Console.WriteLine(Utility.Beautify(s));
            o1 p = Utility.ToObject<o1>(s);
            Assert.AreEqual(p, p.o2obj.parent);
            Assert.AreEqual(p.o2obj, p.child.child);
        }

        public class lol
        {
            public List<List<object>> r;
        }
        public class lol2
        {
            public List<object[]> r;
        }
        [TestMethod]
        public void ListOfList()
        {
            List<List<object>> o = new List<List<object>> { new List<object> { 1, 2, 3 }, new List<object> { "aa", 3, "bb" } };
            string s = Utility.ToJSON(o);
            Console.WriteLine(s);
            object i = Utility.ToObject(s);
            lol p = new lol { r = o };
            s = Utility.ToJSON(p);
            Console.WriteLine(s);
            i = Utility.ToObject(s);
            Assert.AreEqual(3, (i as lol).r[0].Count);

            List<object[]> oo = new List<object[]> { new object[] { 1, 2, 3 }, new object[] { "a", 4, "b" } };
            s = Utility.ToJSON(oo);
            Console.WriteLine(s);
            object ii = Utility.ToObject(s);
            lol2 l = new lol2() { r = oo };

            s = Utility.ToJSON(l);
            Console.WriteLine(s);
            object iii = Utility.ToObject(s);
            Assert.AreEqual(3, (iii as lol2).r[0].Length);
        }
        //[TestMethod]
        //public void Exception()
        //{
        //    var e = new Exception("hello");

        //    var s = Utility.ToJSON(e);
        //    Console.WriteLine(s);
        //    var o = Utility.ToObject(s);
        //    Assert.AreEqual("hello", (o as Exception).Message);
        //}
        //public class ilistclass
        //{
        //    public string name;
        //    public IList<colclass> list { get; set; }
        //}

        //[TestMethod]
        //public void ilist()
        //{
        //    ilistclass i = new ilistclass();
        //    i.name = "aa";
        //    i.list = new List<colclass>();
        //    i.list.Add(new colclass() { gender = Gender.Female, date = DateTime.Now, isNew = true });

        //    var s = Utility.ToJSON(i);
        //    Console.WriteLine(s);
        //    var o = Utility.ToObject(s);
        //}


        //[TestMethod]
        //public void listdic()
        //{ 
        //    string s = @"[{""1"":""a""},{""2"":""b""}]";
        //    var o = Utility.ToDynamic(s);// ToObject<List<Dictionary<string, object>>>(s);
        //    var d = o[0].Count;
        //    Console.WriteLine(d.ToString());
        //}


        public class Y
        {
            public byte[] BinaryData;
        }

        public class A
        {
            public int DataA;
            public A NextA;
        }

        public class B : A
        {
            public string DataB;
        }

        public class C : A
        {
            public DateTime DataC;
        }

        public class Root
        {
            public Y TheY;
            public List<A> ListOfAs = new List<A>();
            public string UnicodeText;
            public Root NextRoot;
            public int MagicInt { get; set; }
            public A TheReferenceA;

            public void SetMagicInt(int value)
            {
                MagicInt = value;
            }
        }

        [TestMethod]
        public void complexobject()
        {
            Root r = new Root
            {
                TheY = new Y { BinaryData = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF } }
            };
            r.ListOfAs.Add(new A { DataA = 10 });
            r.ListOfAs.Add(new B { DataA = 20, DataB = "Hello" });
            r.ListOfAs.Add(new C { DataA = 30, DataC = DateTime.Today });
            r.UnicodeText = "Žlutý kůň ∊ WORLD";
            r.ListOfAs[2].NextA = r.ListOfAs[1];
            r.ListOfAs[1].NextA = r.ListOfAs[2];
            r.TheReferenceA = r.ListOfAs[2];
            r.NextRoot = r;

            Parameters jsonParams = new Parameters
            {
                UseEscapedUnicode = false
            };

            Console.WriteLine("JSON:\n---\n{0}\n---", Utility.ToJSON(r, jsonParams));

            Console.WriteLine();

            Console.WriteLine("Nice JSON:\n---\n{0}\n---", Utility.ToNiceJSON(Utility.ToObject<Root>(Utility.ToNiceJSON(r, jsonParams)), jsonParams));
        }

        [TestMethod]
        public void TestMilliseconds()
        {
            Parameters jpar = new Parameters
            {
                DateTimeMilliseconds = false
            };
            DateTime dt = DateTime.Now;
            string s = Utility.ToJSON(dt, jpar);
            Console.WriteLine(s);
            DateTime o = Utility.ToObject<DateTime>(s, jpar);
            Assert.AreNotEqual(dt.Millisecond, o.Millisecond);

            jpar.DateTimeMilliseconds = true;
            s = Utility.ToJSON(dt, jpar);
            Console.WriteLine(s);
            o = Utility.ToObject<DateTime>(s, jpar);
            Assert.AreEqual(dt.Millisecond, o.Millisecond);
        }

        public struct Foo
        {
            public string name;
        };

        public class Bar
        {
            public Foo foo;
        };

        [TestMethod]
        public void StructProperty()
        {
            Bar b = new Bar
            {
                foo = new Foo
                {
                    name = "Buzz"
                }
            };
            string json = Utility.ToJSON(b);
            Bar bar = Utility.ToObject<Bar>(json);
        }

        [TestMethod]
        public void NullVariable()
        {
            int? i = Utility.ToObject<int?>("10");
            Assert.AreEqual(10, i);
            long? l = Utility.ToObject<long?>("100");
            Assert.AreEqual(100L, l);
            DateTime? d = Utility.ToObject<DateTime?>("\"2000-01-01 10:10:10\"");
            Assert.AreEqual(2000, d.Value.Year);
        }

        public class readonlyclass
        {
            public readonlyclass()
            {
                ROName = "bb";
                Age = 10;
            }
            private string _ro = "aa";
            public string ROAddress { get { return _ro; } }
            public string ROName { get; private set; }
            public int Age { get; set; }
        }

        [TestMethod]
        public void ReadonlyTest()
        {
            string s = Utility.ToJSON(new readonlyclass(), new Parameters { ShowReadOnlyProperties = true });
            readonlyclass o = Utility.ToObject<readonlyclass>(s.Replace("aa", "cc"));
            Assert.AreEqual("aa", o.ROAddress);
        }

        public class container
        {
            public string name = "aa";
            public List<inline> items = new List<inline>();
        }
        public class inline
        {
            public string aaaa = "1111";
            public int id = 1;
        }

        [TestMethod]
        public void InlineCircular()
        {
            container o = new container();
            inline i = new inline();
            o.items.Add(i);
            o.items.Add(i);

            string s = Utility.ToNiceJSON(o, Utility.Parameters);
            Console.WriteLine("*** circular replace");
            Console.WriteLine(s);

            s = Utility.ToNiceJSON(o, new Parameters { InlineCircularReferences = true });
            Console.WriteLine("*** inline objects");
            Console.WriteLine(s);
        }


        [TestMethod]
        public void lowercaseSerilaize()
        {
            ReturnClass r = new ReturnClass
            {
                Name = "Hello",
                Field1 = "dsasdF",
                Field2 = 2312,
                Date = DateTime.Now
            };
            string s = Utility.ToNiceJSON(r, new Parameters { SerializeToLowerCaseNames = true });
            Console.WriteLine(s);
            object o = Utility.ToObject(s);
            Assert.IsNotNull(o);
            Assert.AreEqual("Hello", (o as ReturnClass).Name);
            Assert.AreEqual(2312, (o as ReturnClass).Field2);
        }


        public class nulltest
        {
            public string A;
            public int b;
            public DateTime? d;
        }

        [TestMethod]
        public void null_in_dictionary()
        {
            Dictionary<string, object> d = new Dictionary<string, object>
            {
                { "a", null },
                { "b", 12 },
                { "c", null }
            };

            string s = Utility.ToJSON(d);
            Console.WriteLine(s);
            s = Utility.ToJSON(d, new Parameters() { SerializeNullValues = false });
            Console.WriteLine(s);
            Assert.AreEqual("{\"b\":12}", s);

            s = Utility.ToJSON(new nulltest(), new Parameters { SerializeNullValues = false, UseExtensions = false });
            Console.WriteLine(s);
            Assert.AreEqual("{\"b\":0}", s);
        }


        public class InstrumentSettings
        {
            public string dataProtocol { get; set; }
            public static bool isBad { get; set; }
            public static bool isOk;

            public InstrumentSettings() => dataProtocol = "Wireless";
        }

        [TestMethod]
        public void statictest()
        {
            InstrumentSettings s = new InstrumentSettings();
            Parameters pa = new Parameters
            {
                UseExtensions = false
            };
            InstrumentSettings.isOk = true;
            InstrumentSettings.isBad = true;

            string jsonStr = Utility.ToNiceJSON(s, pa);

            InstrumentSettings o = Utility.ToObject<InstrumentSettings>(jsonStr);
        }

        public class arrayclass2
        {
            public int[] ints { get; set; }
            public string[] strs;
            public int[][] int2d { get; set; }
            public int[][][] int3d;
            public Base[][] class2d;
        }

        [TestMethod]
        public void ArrayTest2()
        {
            arrayclass2 a = new arrayclass2
            {
                ints = new int[] { 3, 1, 4 },
                strs = new string[] { "a", "b", "c" },
                int2d = new int[][] { new int[] { 1, 2, 3 }, new int[] { 2, 3, 4 } },
                int3d = new int[][][] {        new int[][] {
            new int[] { 0, 0, 1 },
            new int[] { 0, 1, 0 }
        },
        null,
        new int[][] {
            new int[] { 0, 0, 2 },
            new int[] { 0, 2, 0 },
            null
        }
    },
                class2d = new Base[][]{
        new Base[] {
            new Base () { Name = "a", Code = "A" },
            new Base () { Name = "b", Code = "B" }
        },
        new Base[] {
            new Base () { Name = "c" }
        },
        null
    }
            };
            string s = Utility.ToJSON(a);
            arrayclass2 o = Utility.ToObject<arrayclass2>(s);
            CollectionAssert.AreEqual(a.ints, o.ints);
            CollectionAssert.AreEqual(a.strs, o.strs);
            CollectionAssert.AreEqual(a.int2d[0], o.int2d[0]);
            CollectionAssert.AreEqual(a.int2d[1], o.int2d[1]);
            CollectionAssert.AreEqual(a.int3d[0][0], o.int3d[0][0]);
            CollectionAssert.AreEqual(a.int3d[0][1], o.int3d[0][1]);
            Assert.AreEqual(null, o.int3d[1]);
            CollectionAssert.AreEqual(a.int3d[2][0], o.int3d[2][0]);
            CollectionAssert.AreEqual(a.int3d[2][1], o.int3d[2][1]);
            CollectionAssert.AreEqual(a.int3d[2][2], o.int3d[2][2]);
            for (int i = 0; i < a.class2d.Length; i++)
            {
                Base[] ai = a.class2d[i];
                Base[] oi = o.class2d[i];
                if (ai == null && oi == null)
                {
                    continue;
                }
                for (int j = 0; j < ai.Length; j++)
                {
                    Base aii = ai[j];
                    Base oii = oi[j];
                    if (aii == null && oii == null)
                    {
                        continue;
                    }
                    Assert.AreEqual(aii.Name, oii.Name);
                    Assert.AreEqual(aii.Code, oii.Code);
                }
            }
        }

        [TestMethod]
        public void Dictionary_String_Object_WithList()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>
            {
                { "C", new List<float>() { 1.1f, 2.2f, 3.3f } }
            };
            string json = Utility.ToJSON(dict);

            Dictionary<string, List<float>> des = Utility.ToObject<Dictionary<string, List<float>>>(json);
            Assert.IsInstanceOfType(des["C"], typeof(List<float>));
        }

        [TestMethod]
        public void exotic_deserialize()
        {
            Console.WriteLine();
            Console.Write("fastjson deserialize");
            Column c = CreateObject(true, true);
            Stopwatch stopwatch = new Stopwatch();
            for (int pp = 0; pp < Five; pp++)
            {
                Column deserializedStore;
                string jsonText = null;

                stopwatch.Restart();
                jsonText = Utility.ToJSON(c);
                //Console.WriteLine(" size = " + jsonText.Length);
                for (int i = 0; i < OneThousand; i++)
                {
                    deserializedStore = (Column)Utility.ToObject(jsonText);
                }
                stopwatch.Stop();
                Console.Write("\t" + stopwatch.ElapsedMilliseconds);
            }
        }

        [TestMethod]
        public void exotic_serialize()
        {
            Console.WriteLine();
            Console.Write("fastjson serialize");
            Column c = CreateObject(true, true);
            Stopwatch stopwatch = new Stopwatch();
            for (int pp = 0; pp < Five; pp++)
            {
                string jsonText = null;
                stopwatch.Restart();
                for (int i = 0; i < OneThousand; i++)
                {
                    jsonText = Utility.ToJSON(c);
                }
                stopwatch.Stop();
                Console.Write("\t" + stopwatch.ElapsedMilliseconds);
            }
        }

        [TestMethod]
        public void BigData()
        {
            Console.WriteLine();
            Console.Write("fastjson bigdata serialize");
            Column c = CreateBigdata();
            Console.WriteLine("\r\ntest obj created");
            double t = 0;
            Stopwatch stopwatch = new Stopwatch();
            for (int pp = 0; pp < Five; pp++)
            {
                string jsonText = null;
                stopwatch.Restart();

                jsonText = Utility.ToJSON(c);

                stopwatch.Stop();
                t += stopwatch.ElapsedMilliseconds;
                Console.Write("\t" + stopwatch.ElapsedMilliseconds);
            }
            Console.WriteLine("\tAVG = " + t / Five);
        }

        private static Column CreateBigdata()
        {
            Column c = new Column();
            Random r = new Random((int)DateTime.Now.Ticks);

            for (int i = 0; i < 200 * OneThousand; i++)
            {
                c.Items.Add(new ClassA(r.Next().ToString(), r.Next().ToString(), Guid.NewGuid()));
            }
            return c;
        }

        [TestMethod]
        public void comments()
        {
            string s = @"
{
    // help
    ""property"" : 2,
    // comment
    ""str"":""hello"" //hello
}
";
            object o = Utility.Parse(s);
            Assert.AreEqual(2, (o as IDictionary).Count);
        }

        public class ctype
        {
            public System.Net.IPAddress ip;
        }
        [TestMethod]
        public void CustomTypes()
        {
            ctype ip = new ctype
            {
                ip = System.Net.IPAddress.Loopback
            };

            Utility.RegisterCustomType(typeof(System.Net.IPAddress),
                (x) => { return x.ToString(); },
                (x) => { return System.Net.IPAddress.Parse(x); });

            string s = Utility.ToJSON(ip);

            ctype o = Utility.ToObject<ctype>(s);
            Assert.AreEqual(ip.ip, o.ip);
        }

        [TestMethod]
        public void stringint()
        {
            long o = Utility.ToObject<long>("\"42\"");
        }

        [TestMethod]
        public void anonymoustype()
        {
            Parameters Parameters = new Parameters { EnableAnonymousTypes = true };
            List<DateTimeOffset> data = new List<DateTimeOffset>
            {
                new DateTimeOffset(DateTime.Now)
            };

            var anonTypeWithDateTimeOffset = data.Select(entry => new { DateTimeOffset = entry }).ToList();
            string json = Utility.ToJSON(anonTypeWithDateTimeOffset.First(), Parameters); // this will throw

            var obj = new
            {
                Name = "aa",
                Age = 42,
                Code = "007"
            };

            json = Utility.ToJSON(obj, Parameters);
            Assert.IsTrue(json.Contains("\"Name\""));
        }

        [TestMethod]
        public void Expando()
        {
            dynamic obj = new ExpandoObject();
            obj.UserView = "10080";
            obj.UserCatalog = "test";
            obj.UserDate = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            obj.UserBase = "";

            string s = Utility.ToJSON(obj);
            Assert.IsTrue(s.Contains("UserView\":\"10080"));
        }


        public class item
        {
            public string name;
            public int age;
        }

        [TestMethod]
        public void array()
        {
            string j = @"
[
{""name"":""Tom"",""age"":1},
{""name"":""Dick"",""age"":1},
{""name"":""Harry"",""age"":3}
]
";

            List<item> o = Utility.ToObject<List<item>>(j);
            Assert.AreEqual(3, o.Count);

            item[] oo = Utility.ToObject<item[]>(j);
            Assert.AreEqual(3, oo.Count());
        }

        [TestMethod]
        public void NaN()
        {
            double d = Double.NaN;
            float f = Single.NaN;


            string s = Utility.ToJSON(d);
            double o = Utility.ToObject<double>(s);
            Assert.AreEqual(d, o);

            s = Utility.ToJSON(f);
            float oo = Utility.ToObject<float>(s);
            Assert.AreEqual(f, oo);

            float pp = Utility.ToObject<Single>(s);
        }

        [TestMethod]
        public void nonstandardkey()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>
            {
                ["With \"Quotes\""] = "With \"Quotes\""
            };
            Parameters p = new Parameters
            {
                EnableAnonymousTypes = false,
                SerializeNullValues = false,
                UseExtensions = false
            };
            string s = Utility.ToJSON(dict, p);
            Dictionary<string, string> d = Utility.ToObject<Dictionary<string, string>>(s);
            Assert.AreEqual(1, d.Count);
            Assert.AreEqual("With \"Quotes\"", d.Keys.First());
        }

        [TestMethod]
        public void bytearrindic()
        {
            string s = Utility.ToJSON(new Dictionary<string, byte[]>
                {
                    { "Test", new byte[10] },
                    { "Test 2", new byte[0] }
                });

            Dictionary<string, byte[]> d = Utility.ToObject<Dictionary<string, byte[]>>(s);
        }

        #region twitter
        public class Twitter
        {
            public Query query { get; set; }
            public Result result { get; set; }

            public class Query
            {
                public Parameters @params { get; set; }
                public string type { get; set; }
                public string url { get; set; }
            }

            public class Parameters
            {
                public int accuracy { get; set; }
                public bool autocomplete { get; set; }
                public string granularity { get; set; }
                public string query { get; set; }
                public bool trim_place { get; set; }
            }

            public class Result
            {
                public Place[] places { get; set; }
            }

            public class Place
            {
                public Attributes attributes { get; set; }
                public BoundingBox bounding_box { get; set; }
                public Place[] contained_within { get; set; }

                public string country { get; set; }
                public string country_code { get; set; }
                public string full_name { get; set; }
                public string id { get; set; }
                public string name { get; set; }
                public string place_type { get; set; }
                public string url { get; set; }
            }

            public class Attributes
            {
            }

            public class BoundingBox
            {
                public double[][][] coordinates { get; set; }
                public string type { get; set; }
            }
        }
        #endregion
        [TestMethod]
        public void twitter()
        {
            #region tw data
            string ss = @"{
  ""query"": {
    ""params"": {
      ""accuracy"": 0,
      ""autocomplete"": false,
      ""granularity"": ""neighborhood"",
      ""query"": ""Toronto"",
      ""trim_place"": false
    },
    ""type"": ""search"",
    ""url"": ""https://api.twitter.com/1.1/geo/search.json?accuracy=0&query=Toronto&granularity=neighborhood&autocomplete=false&trim_place=false""
  },
  ""result"": {
    ""places"": [
      {
        ""attributes"": {},
        ""bounding_box"": {
          ""coordinates"": [
            [
              [
                -96.647415,
                44.566715
              ],
              [
                -96.630435,
                44.566715
              ],
              [
                -96.630435,
                44.578118
              ],
              [
                -96.647415,
                44.578118
              ]
            ]
          ],
          ""type"": ""Polygon""
        },
        ""contained_within"": [
          {
            ""attributes"": {},
            ""bounding_box"": {
              ""coordinates"": [
                [
                  [
                    -104.057739,
                    42.479686
                  ],
                  [
                    -96.436472,
                    42.479686
                  ],
                  [
                    -96.436472,
                    45.945716
                  ],
                  [
                    -104.057739,
                    45.945716
                  ]
                ]
              ],
              ""type"": ""Polygon""
            },
            ""country"": ""United States"",
            ""country_code"": ""US"",
            ""full_name"": ""South Dakota, US"",
            ""id"": ""d06e595eb3733f42"",
            ""name"": ""South Dakota"",
            ""place_type"": ""admin"",
            ""url"": ""https://api.twitter.com/1.1/geo/id/d06e595eb3733f42.json""
          }
        ],
        ""country"": ""United States"",
        ""country_code"": ""US"",
        ""full_name"": ""Toronto, SD"",
        ""id"": ""3e8542a1e9f82870"",
        ""name"": ""Toronto"",
        ""place_type"": ""city"",
        ""url"": ""https://api.twitter.com/1.1/geo/id/3e8542a1e9f82870.json""
      },
      {
        ""attributes"": {},
        ""bounding_box"": {
          ""coordinates"": [
            [
              [
                -80.622815,
                40.436469
              ],
              [
                -80.596567,
                40.436469
              ],
              [
                -80.596567,
                40.482566
              ],
              [
                -80.622815,
                40.482566
              ]
            ]
          ],
          ""type"": ""Polygon""
        },
        ""contained_within"": [
          {
            ""attributes"": {},
            ""bounding_box"": {
              ""coordinates"": [
                [
                  [
                    -84.820305,
                    38.403423
                  ],
                  [
                    -80.518454,
                    38.403423
                  ],
                  [
                    -80.518454,
                    42.327132
                  ],
                  [
                    -84.820305,
                    42.327132
                  ]
                ]
              ],
              ""type"": ""Polygon""
            },
            ""country"": ""United States"",
            ""country_code"": ""US"",
            ""full_name"": ""Ohio, US"",
            ""id"": ""de599025180e2ee7"",
            ""name"": ""Ohio"",
            ""place_type"": ""admin"",
            ""url"": ""https://api.twitter.com/1.1/geo/id/de599025180e2ee7.json""
          }
        ],
        ""country"": ""United States"",
        ""country_code"": ""US"",
        ""full_name"": ""Toronto, OH"",
        ""id"": ""53d949149e8cd438"",
        ""name"": ""Toronto"",
        ""place_type"": ""city"",
        ""url"": ""https://api.twitter.com/1.1/geo/id/53d949149e8cd438.json""
      },
      {
        ""attributes"": {},
        ""bounding_box"": {
          ""coordinates"": [
            [
              [
                -79.639128,
                43.403221
              ],
              [
                -78.90582,
                43.403221
              ],
              [
                -78.90582,
                43.855466
              ],
              [
                -79.639128,
                43.855466
              ]
            ]
          ],
          ""type"": ""Polygon""
        },
        ""contained_within"": [
          {
            ""attributes"": {},
            ""bounding_box"": {
              ""coordinates"": [
                [
                  [
                    -95.155919,
                    41.676329
                  ],
                  [
                    -74.339383,
                    41.676329
                  ],
                  [
                    -74.339383,
                    56.852398
                  ],
                  [
                    -95.155919,
                    56.852398
                  ]
                ]
              ],
              ""type"": ""Polygon""
            },
            ""country"": ""Canada"",
            ""country_code"": ""CA"",
            ""full_name"": ""Ontario, Canada"",
            ""id"": ""89b2eb8b2b9847f7"",
            ""name"": ""Ontario"",
            ""place_type"": ""admin"",
            ""url"": ""https://api.twitter.com/1.1/geo/id/89b2eb8b2b9847f7.json""
          }
        ],
        ""country"": ""Canada"",
        ""country_code"": ""CA"",
        ""full_name"": ""Toronto, Ontario"",
        ""id"": ""8f9664a8ccd89e5c"",
        ""name"": ""Toronto"",
        ""place_type"": ""city"",
        ""url"": ""https://api.twitter.com/1.1/geo/id/8f9664a8ccd89e5c.json""
      },
      {
        ""attributes"": {},
        ""bounding_box"": {
          ""coordinates"": [
            [
              [
                -90.867234,
                41.898723
              ],
              [
                -90.859467,
                41.898723
              ],
              [
                -90.859467,
                41.906811
              ],
              [
                -90.867234,
                41.906811
              ]
            ]
          ],
          ""type"": ""Polygon""
        },
        ""contained_within"": [
          {
            ""attributes"": {},
            ""bounding_box"": {
              ""coordinates"": [
                [
                  [
                    -96.639485,
                    40.375437
                  ],
                  [
                    -90.140061,
                    40.375437
                  ],
                  [
                    -90.140061,
                    43.501196
                  ],
                  [
                    -96.639485,
                    43.501196
                  ]
                ]
              ],
              ""type"": ""Polygon""
            },
            ""country"": ""United States"",
            ""country_code"": ""US"",
            ""full_name"": ""Iowa, US"",
            ""id"": ""3cd4c18d3615bbc9"",
            ""name"": ""Iowa"",
            ""place_type"": ""admin"",
            ""url"": ""https://api.twitter.com/1.1/geo/id/3cd4c18d3615bbc9.json""
          }
        ],
        ""country"": ""United States"",
        ""country_code"": ""US"",
        ""full_name"": ""Toronto, IA"",
        ""id"": ""173d6f9c3249b4fd"",
        ""name"": ""Toronto"",
        ""place_type"": ""city"",
        ""url"": ""https://api.twitter.com/1.1/geo/id/173d6f9c3249b4fd.json""
      },
      {
        ""attributes"": {},
        ""bounding_box"": {
          ""coordinates"": [
            [
              [
                -95.956873,
                37.792724
              ],
              [
                -95.941288,
                37.792724
              ],
              [
                -95.941288,
                37.803752
              ],
              [
                -95.956873,
                37.803752
              ]
            ]
          ],
          ""type"": ""Polygon""
        },
        ""contained_within"": [
          {
            ""attributes"": {},
            ""bounding_box"": {
              ""coordinates"": [
                [
                  [
                    -102.051769,
                    36.993016
                  ],
                  [
                    -94.588387,
                    36.993016
                  ],
                  [
                    -94.588387,
                    40.003166
                  ],
                  [
                    -102.051769,
                    40.003166
                  ]
                ]
              ],
              ""type"": ""Polygon""
            },
            ""country"": ""United States"",
            ""country_code"": ""US"",
            ""full_name"": ""Kansas, US"",
            ""id"": ""27c45d804c777999"",
            ""name"": ""Kansas"",
            ""place_type"": ""admin"",
            ""url"": ""https://api.twitter.com/1.1/geo/id/27c45d804c777999.json""
          }
        ],
        ""country"": ""United States"",
        ""country_code"": ""US"",
        ""full_name"": ""Toronto, KS"",
        ""id"": ""b90e4628bff4ad82"",
        ""name"": ""Toronto"",
        ""place_type"": ""city"",
        ""url"": ""https://api.twitter.com/1.1/geo/id/b90e4628bff4ad82.json""
      }
    ]
  }
}";
            #endregion
            Twitter o = Utility.ToObject<Twitter>(ss);
        }

        [TestMethod]
        public void datetimeoff()
        {
            DateTimeOffset dt = new DateTimeOffset(DateTime.Now);
            // test with UTC format ('Z' in output rather than HH:MM timezone)
            Assert.AreEqual(dt.ToUniversalTime().ToString("O"), Utility.ToObject<DateTimeOffset>(Utility.ToJSON(dt, new Parameters { UseUTCDateTime = true })).ToUniversalTime().ToString("O"));
            
            // ticks will differ, so convert both to UTC and use ISO8601 roundtrip format to compare
            Assert.AreEqual(dt.ToUniversalTime().ToString("O"), Utility.ToObject<DateTimeOffset>(Utility.ToJSON(dt, new Parameters { UseUTCDateTime = false })).ToUniversalTime().ToString("O"));

            // test deserialize of output from DateTimeOffset.ToString()
            // DateTimeOffset roundtrip format, UTC 
            dt = new DateTimeOffset(DateTime.UtcNow);
            Assert.AreEqual(dt.ToUniversalTime().ToString("O"), Utility.ToObject<DateTimeOffset>($@"""{dt.ToString("O")}""").ToUniversalTime().ToString("O"));

            // DateTimeOffset roundtrip format, non-UTC
            dt = new DateTimeOffset(new DateTime(2017, 5, 22, 10, 06, 53, 123, DateTimeKind.Unspecified), TimeSpan.FromHours(11.5));
            Assert.AreEqual(dt.ToUniversalTime().ToString("O"), Utility.ToObject<DateTimeOffset>($@"""{dt.ToString("O")}""").ToUniversalTime().ToString("O"));

            // previous fastJSON serialization format for DateTimeOffset. Millisecond resolution only.
            Assert.AreEqual(dt.ToUniversalTime().ToString("O"), Utility.ToObject<DateTimeOffset>($@"""{dt.ToString("yyyy-MM-ddTHH:mm:ss.fff zzz")}""").ToUniversalTime().ToString("O"));
        }

        class X
        {
            public X(int i) => I = i;
            public int I { get; }
        }

        [TestMethod]
        public void ReadonlyProperty()
        {
            X x = new X(10);
            string s = Utility.ToJSON(x, new Parameters { ShowReadOnlyProperties = true });
            Assert.IsTrue(s.Contains("\"I\":"));
            X o = Utility.ToObject<X>(s, new Parameters { ParametricConstructorOverride = true });
            // no set available -> I = 0
            Assert.AreEqual(0, o.I);
        }


        public class il
        {
            public IList list;
            public string name;
        }

        [TestMethod]
        public void ilist()
        {
            il i = new il
            {
                list = new List<Base>()
            };
            i.list.Add(new ClassA("1", "1", Guid.NewGuid()));
            i.list.Add(new ClassB("4", "5", "hi"));
            i.name = "hi";

            string s = Utility.ToNiceJSON(i);//, new Parameters { UseExtensions = true });
            Console.WriteLine(s);

            il o = Utility.ToObject<il>(s);
        }


        public interface iintfc
        {
            string name { get; set; }
            int age { get; set; }
        }

        public class intfc : iintfc
        {
            public string address = "fadfsdf";
            private int _age;
            public int age
            {
                get
                {
                    return _age;
                }

                set
                {
                    _age = value;
                }
            }
            private string _name;
            public string name
            {
                get
                {
                    return _name;
                }

                set
                {
                    _name = value;
                }
            }
        }

        public class it
        {
            public iintfc i { get; set; }
            public string name = "bb";

        }

        [TestMethod]
        public void interface_test()
        {
            it ii = new it();

            intfc i = new intfc
            {
                age = 10,
                name = "aa"
            };

            ii.i = i;

            string s = Utility.ToJSON(ii);

            object o = Utility.ToObject(s);

        }

        [TestMethod]
        public void nested_dictionary()
        {
            Dictionary<int, Dictionary<string, double>> dic = new Dictionary<int, Dictionary<string, double>>
            {
                [0] = new Dictionary<string, double> { ["PX_LAST"] = 1.1, ["PX_LOW"] = 1.0 },
                [1] = new Dictionary<string, double> { ["PX_LAST"] = 2.1, ["PX_LOW"] = 2.0 }
            };

            string s = Utility.ToJSON(dic);
            Dictionary<int, Dictionary<string, double>> obj = Utility.ToObject<Dictionary<int, Dictionary<string, double>>>(s);
            Assert.AreEqual(2, obj[0].Count());
        }

        [TestMethod]
        public void DynamicEnumerate()
        {
            string j =
            @"[
   {
      ""Prop1"" : ""Info 1"",
      ""Prop2"" : ""More Info 1""
   },
   {
      ""Prop1"" : ""Info 2"",
      ""Prop2"" : ""More Info 2""
   }
]";

            dynamic testObject = Utility.ToDynamic(j);
            foreach (dynamic o in testObject)
            {
                Console.WriteLine(o.Prop1);
                Assert.IsTrue(o.Prop1 != "");
            }
        }

        public class AC { public AC() { } public decimal Lo { get; set; } public decimal Ratio { get; set; } }

        [TestMethod]
        public void DictListTest()
        {
            Dictionary<string, List<AC>> dictList = new Dictionary<string, List<AC>>
            {
                { "P", new List<AC>() }
            };
            dictList["P"].Add(new AC() { Lo = 1.5m, Ratio = 2.5m });
            dictList["P"].Add(new AC() { Lo = 2.5m, Ratio = 3.5m });
            string jsonstr = Utility.ToJSON(dictList, new Parameters { UseExtensions = false });

            Console.WriteLine();
            Console.WriteLine(jsonstr);

            Dictionary<string, List<AC>> dictList2 = Utility.ToObject<Dictionary<string, List<AC>>>(jsonstr);

            Assert.IsTrue(dictList2["P"].Count == 2);
            Assert.IsTrue(dictList2["P"][0].GetType() == typeof(AC));
            foreach (KeyValuePair<string, List<AC>> k in dictList2)
            {
                Console.Write(k.Key);
                foreach (AC v in k.Value)
                    Console.WriteLine(":\tLo:{0}\tRatio:{1}", v.Lo, v.Ratio);
            }
        }


        public class ac<T>
        {
            public T age;
        }
        [TestMethod]
        public void autoconvert()
        {
            long v = Int64.MaxValue;
            //v = 42;
            //byte v = 42;
            ac<long> o = Utility.ToObject<ac<long>>("{\"age\":\"" + v + "\"}");
            Assert.AreEqual(v, o.age);
        }

        [TestMethod]
        public void timespan()
        {
            TimeSpan t = new TimeSpan(2, 2, 2, 2);
            string s = Utility.ToJSON(t);
            TimeSpan o = Utility.ToObject<TimeSpan>(s);
            Assert.AreEqual(o.Days, t.Days);
        }

        public class dmember
        {
            [DataMember(Name = "prop")]
            public string MyProperty;
            [DataMember(Name = "id")]
            public int docid;
        }

        [TestMethod]
        public void DataMember()
        {
            string s = "{\"prop\":\"Date\",\"id\":42}";
            Console.WriteLine(s);
            dmember o = Utility.ToObject<dmember>(s);

            Assert.AreEqual(42, o.docid);
            Assert.AreEqual("Date", o.MyProperty);

            string ss = Utility.ToJSON(o, new Parameters { UseExtensions = false });
            Console.WriteLine(ss);
            Assert.AreEqual(s, ss);
        }

        [TestMethod]
        public void zerostring()
        {
            string t = "test\0test";
            Console.WriteLine(t);
            string s = Utility.ToJSON(t, new Parameters { UseEscapedUnicode = false });
            Assert.IsTrue(s.Contains("\\u0000"));
            Console.WriteLine(s);
            string o = Utility.ToObject<string>(s);
            Assert.IsTrue(o.Contains("\0"));
            Console.WriteLine("" + o);
        }

        [TestMethod]
        public void spacetest()
        {
            Column c = new Column();

            string s = Utility.ToNiceJSON(c);
            Console.WriteLine(s);
            s = Utility.Beautify(s, 2);
            Console.WriteLine(s);
            s = Utility.ToNiceJSON(c, new Parameters { FormatterIndentSpaces = 8 });
            Console.WriteLine(s);
        }

        public class DigitLimit
        {
            public float Fmin;
            public float Fmax;
            public decimal MminDec;
            public decimal MmaxDec;


            public decimal Mmin;
            public decimal Mmax;
            public double Dmin;
            public double Dmax;
            public double DminDec;
            public double DmaxDec;
            public double Dni;
            public double Dpi;
            public double Dnan;
            public float FminDec;
            public float FmaxDec;
            public float Fni;
            public float Fpi;
            public float Fnan;
            public long Lmin;
            public long Lmax;
            public ulong ULmax;
            public int Imin;
            public int Imax;
            public uint UImax;


            //public IntPtr Iptr1 = new IntPtr(0); //Serialized to a Dict, exception on deserialization
            //public IntPtr Iptr2 = new IntPtr(0x33445566); //Serialized to a Dict, exception on deserialization
            //public UIntPtr UIptr1 = new UIntPtr(0); //Serialized to a Dict, exception on deserialization
            //public UIntPtr UIptr2 = new UIntPtr(0x55667788); //Serialized to a Dict, exception on deserialization
        }

        [TestMethod]
        public void digitlimits()
        {
            DigitLimit d = new DigitLimit
            {
                Fmin = Single.MinValue,// serializer loss on tostring() 
                Fmax = Single.MaxValue,// serializer loss on tostring()
                MminDec = -7.9228162514264337593543950335m, //OK to be serialized but lost precision in deserialization
                MmaxDec = +7.9228162514264337593543950335m, //OK to be serialized but lost precision in deserialization

                Mmin = Decimal.MinValue,
                Mmax = Decimal.MaxValue,
                //d.Dmin = double.MinValue;
                //d.Dmax = double.MaxValue;
                DminDec = -Double.Epsilon,
                DmaxDec = Double.Epsilon,
                Dni = Double.NegativeInfinity,
                Dpi = Double.PositiveInfinity,
                Dnan = Double.NaN,
                FminDec = -Single.Epsilon,
                FmaxDec = Single.Epsilon,
                Fni = Single.NegativeInfinity,
                Fpi = Single.PositiveInfinity,
                Fnan = Single.NaN,
                Lmin = Int64.MinValue,
                Lmax = Int64.MaxValue,
                ULmax = UInt64.MaxValue,
                Imin = Int32.MinValue,
                Imax = Int32.MaxValue,
                UImax = UInt32.MaxValue
            };


            string s = Utility.ToNiceJSON(d);
            Console.WriteLine(s);
            DigitLimit o = Utility.ToObject<DigitLimit>(s);


            //ok
            Assert.AreEqual(d.Dmax, o.Dmax);
            Assert.AreEqual(d.DmaxDec, o.DmaxDec);
            Assert.AreEqual(d.Dmin, o.Dmin);
            Assert.AreEqual(d.DminDec, o.DminDec);
            Assert.AreEqual(d.Dnan, o.Dnan);
            Assert.AreEqual(d.Dni, o.Dni);
            Assert.AreEqual(d.Dpi, o.Dpi);
            Assert.AreEqual(d.FmaxDec, o.FmaxDec);
            Assert.AreEqual(d.FminDec, o.FminDec);
            Assert.AreEqual(d.Fnan, o.Fnan);
            Assert.AreEqual(d.Fni, o.Fni);
            Assert.AreEqual(d.Fpi, o.Fpi);
            Assert.AreEqual(d.Imax, o.Imax);
            Assert.AreEqual(d.Imin, o.Imin);
            Assert.AreEqual(d.Lmax, o.Lmax);
            Assert.AreEqual(d.Lmin, o.Lmin);
            Assert.AreEqual(d.Mmax, o.Mmax);
            Assert.AreEqual(d.Mmin, o.Mmin);
            Assert.AreEqual(d.UImax, o.UImax);
            Assert.AreEqual(d.ULmax, o.ULmax);

            // precision loss
            //Assert.AreEqual(d.Fmax, o.Fmax);
            //Assert.AreEqual(d.Fmin, o.Fmin);
            //Assert.AreEqual(d.MmaxDec, o.MmaxDec);
            //Assert.AreEqual(d.MminDec, o.MminDec);
        }


        public class TestData
        {
            [DataMember(Name = "foo")]
            public string Foo { get; set; }

            [DataMember(Name = "Bar")]
            public string Bar { get; set; }
        }
        [TestMethod]
        public void ConvertTest()
        {
            TestData data = new TestData
            {
                Foo = "foo_value",
                Bar = "bar_value"
            };
            string jsonData = Utility.ToJSON(data);

            TestData data2 = Utility.ToObject<TestData>(jsonData);

            // OK, since data member name is "foo" which is all in lower case
            Assert.AreEqual(data.Foo, data2.Foo);

            // Fails, since data member name is "Bar", but the library looks for "bar" when setting the value
            Assert.AreEqual(data.Bar, data2.Bar);
        }


        public class test { public string name = "me"; }
        [TestMethod]
        public void ArrayOfObjectExtOff()
        {
            string s = Utility.ToJSON(new test[] { new test(), new test() }, new Parameters { UseExtensions = false });
            test[] o = Utility.ToObject<test[]>(s);
            Console.WriteLine(o.GetType().ToString());
            Assert.AreEqual(typeof(test[]), o.GetType());
        }
        [TestMethod]
        public void ArrayOfObjectsWithoutTypeInfoToObjectTyped()
        {
            string s = Utility.ToJSON(new test[] { new test(), new test() });
            test[] o = Utility.ToObject<test[]>(s);
            Console.WriteLine(o.GetType().ToString());
            Assert.AreEqual(typeof(test[]), o.GetType());
        }
        [TestMethod]
        public void ArrayOfObjectsWithTypeInfoToObject()
        {
            string s = Utility.ToJSON(new test[] { new test(), new test() });
            object o = Utility.ToObject(s);
            Console.WriteLine(o.GetType().ToString());
            List<object> i = o as List<object>;
            Assert.AreEqual(typeof(test), i[0].GetType());
        }

        public class nskeys
        {
            public string name;
            public int age;
            public string address;
        }
        [TestMethod]
        public void NonStandardKey()
        {
            //var s = "{\"name\":\"m:e\", \"age\":42, \"address\":\"here\"}";
            //var o = Utility.ToObject<nskeys>(s);


            string s = "{name:\"m:e\", age:42, \"address\":\"here\"}";
            nskeys o = Utility.ToObject<nskeys>(s, new Parameters { AllowNonQuotedKeys = true });
            Assert.AreEqual("m:e", o.name);
            Assert.AreEqual("here", o.address);
            Assert.AreEqual(42, o.age);

            s = "{name  \t  :\"me\", age : 42, address  :\"here\"}";
            o = Utility.ToObject<nskeys>(s, new Parameters { AllowNonQuotedKeys = true });
            Assert.AreEqual("me", o.name);
            Assert.AreEqual("here", o.address);
            Assert.AreEqual(42, o.age);

            s = "{    name   :\"me\", age : 42, address :    \"here\"}";
            o = Utility.ToObject<nskeys>(s, new Parameters { AllowNonQuotedKeys = true });
            Assert.AreEqual("me", o.name);
            Assert.AreEqual("here", o.address);
            Assert.AreEqual(42, o.age);
        }

        public class cis
        {
            public string age;
        }

        [TestMethod]
        public void ConvertInt2String()
        {
            string s = "{\"age\":42}";
            cis o = Utility.ToObject<cis>(s);
        }

        [TestMethod]
        public void dicofdic()
        {
            string s = "{ 'Section1' : { 'Key1' : 'Value1', 'Key2' : 'Value2', 'Key3' : 'Value3', 'Key4' : 'Value4', 'Key5' : 'Value5' } }".Replace("\'", "\"");
            Dictionary<string, Dictionary<string, string>> o = Utility.ToObject<Dictionary<string, Dictionary<string, string>>>(s);
            Dictionary<string, string> v = o["Section1"];

            Assert.AreEqual(5, v.Count);
            Assert.AreEqual("Value2", v["Key2"]);
        }

        public class readonlyProps
        {
            public List<string> Collection { get; }

            public readonlyProps(List<string> collection) => Collection = collection;

            public readonlyProps()
            {
            }
        }

        [TestMethod]
        public void ReadOnlyProperty() // rbeurskens 
        {
            readonlyProps dto = new readonlyProps(new List<string> { "test", "test2" });

            Utility.Parameters.ShowReadOnlyProperties = true;
            string s = Utility.ToJSON(dto);
            readonlyProps o = Utility.ToObject<readonlyProps>(s);

            Assert.IsNotNull(o);
            CollectionAssert.AreEqual(dto.Collection, o.Collection);
        }

        public class nsb
        {
            public bool one = false; // number 1
            public bool two = false; // string 1
            public bool three = false; // string true
            public bool four = false; // string on
            public bool five = false; // string yes
        }
        [TestMethod]
        public void NonStrictBoolean()
        {
            string s = "{'one':1,'two':'1','three':'true','four':'on','five':'yes'}".Replace("\'", "\"");

            nsb o = Utility.ToObject<nsb>(s);
            Assert.AreEqual(true, o.one);
            Assert.AreEqual(true, o.two);
            Assert.AreEqual(true, o.three);
            Assert.AreEqual(true, o.four);
            Assert.AreEqual(true, o.five);
        }

        private class npc
        {
            public int a = 1;
            public int b = 2;
        }
        [TestMethod]
        public void NonPublicClass()
        {
            npc p = new npc
            {
                a = 10,
                b = 20
            };
            string s = Utility.ToJSON(p);
            npc o = (npc)Utility.ToObject(s);
            Assert.AreEqual(10, o.a);
            Assert.AreEqual(20, o.b);
        }

        public class Item
        {
            public int Id { get; set; }
            public string Data { get; set; }
        }

        public class TestObject
        {
            public int Id { get; set; }
            public string Stuff { get; set; }
            public virtual ObservableCollection<Item> Items { get; set; }
        }


        [TestMethod]
        public void noncapacitylist()
        {
            TestObject testObject = new TestObject
            {
                Id = 1,
                Stuff = "test",
                Items = new ObservableCollection<Item>()
            };

            testObject.Items.Add(new Item { Id = 1, Data = "Item 1" });
            testObject.Items.Add(new Item { Id = 2, Data = "Item 2" });

            string jsonData = Utility.ToNiceJSON(testObject);
            Console.WriteLine(jsonData);

            TestObject copyObject = new TestObject();
            Utility.FillObject(copyObject, jsonData);
        }
    }
}
