using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace FastJSON.Test.Resources
{
    public static class Miscellaneous
    {
        public static int OneThousand { get; } = 1000;

        public static int Five { get; } = 5;

        public static DataSet Data { get; } = new DataSet { };
    }

    public static class Utilities
    {
        public static void LogDifference(this DateTime startTime) => Logger.LogMessage($"Test completed in {DateTime.Now.Subtract(startTime).TotalMilliseconds}ms.");

        public static TTarget Cast<TOriginal, TTarget>(this TOriginal data) where TTarget : struct => Swallow(() => (data as TTarget?).Value);

        /// <summary>
        /// A method that executes the given <paramref name="operation"/> and catches any exceptions that occur, returning the default value of <typeparamref name="T"/> if necessary.
        /// </summary>
        /// <typeparam name="T">The type that the swallowed function returns.</typeparam>
        /// <param name="operation">The function that should be swallowed.</param>
        /// <returns>The return value of the successfully-executed <paramref name="operation"/> <see cref="Func{T}"/>, or <see cref="default(T)"/> if any exceptions are thrown during execution.</returns>
        public static T Swallow<T>(this Func<T> operation)
        {
            try { return operation.Invoke(); }
            catch { return default(T); }
        }

        public static Column CreateObject(bool exotic = false, bool dataset = false)
        {
            Column column = new Column
            {
                BooleanValue = true,
                OrdinaryDecimal = 3
            };

            if (exotic)
            {
                column.NullableGUID = Guid.NewGuid();
                column.Hash = new Hashtable
                {
                    [new ClassA("0", "hello", Guid.NewGuid())] = new ClassB("1", "code", "desc"),
                    [new ClassB("0", "hello", "pppp")] = new ClassA("1", "code", Guid.NewGuid())
                };
                if (dataset)
                    column.Data = CreateDataset();
                column.Bytes = new byte[1024];
                column.StringDictionary = new Dictionary<string, Base>();
                column.ObjectDictionary = new Dictionary<Base, Base>();
                column.IntDictionary = new Dictionary<int, Base>();
                column.NullableDouble = 100.003;

                column.NullableDecimal = 3.14M;

                column.StringDictionary.Add("name1", new ClassB("1", "code", "desc"));
                column.StringDictionary.Add("name2", new ClassA("1", "code", Guid.NewGuid()));

                column.IntDictionary.Add(1, new ClassB("1", "code", "desc"));
                column.IntDictionary.Add(2, new ClassA("1", "code", Guid.NewGuid()));

                column.ObjectDictionary.Add(new ClassA("0", "hello", Guid.NewGuid()), new ClassB("1", "code", "desc"));
                column.ObjectDictionary.Add(new ClassB("0", "hello", "pppp"), new ClassA("1", "code", Guid.NewGuid()));

                column.ArrayType = new Base[2];
                column.ArrayType[0] = new ClassA();
                column.ArrayType[1] = new ClassB();
            }

            column.Items.Add(new ClassA("1", "1", Guid.NewGuid()));
            column.Items.Add(new ClassB("2", "2", "desc1"));
            column.Items.Add(new ClassA("3", "3", Guid.NewGuid()));
            column.Items.Add(new ClassB("4", "4", "desc2"));

            column.LastString = $"{DateTime.Now}";

            return column;
        }

        public static long CreateLong(string s)
        {
            long num = 0;
            bool neg = false;
            foreach (char cc in s)
            {
                if (cc == '-')
                    neg = true;
                else if (cc == '+')
                    neg = false;
                else
                {
                    num *= 10;
                    num += (int)(cc - '0');
                }
            }

            return neg ? -num : num;
        }

        public static DataSet CreateDataset()
        {
            DataSet dataset = new DataSet();
            for (int j = 1; j < 3; j++)
            {
                DataTable data = new DataTable { TableName = "Table" + j };
                data.Columns.Add("col1", typeof(int));
                data.Columns.Add("col2", typeof(string));
                data.Columns.Add("col3", typeof(Guid));
                data.Columns.Add("col4", typeof(string));
                data.Columns.Add("col5", typeof(bool));
                data.Columns.Add("col6", typeof(string));
                data.Columns.Add("col7", typeof(string));
                dataset.Tables.Add(data);
                Random generator = new Random { };
                for (int i = 0; i < 100; i++)
                {
                    DataRow row = data.NewRow();
                    row[0] = generator.Next(Int32.MaxValue);
                    row[1] = $"{generator.Next(Int32.MaxValue)}";
                    row[2] = Guid.NewGuid();
                    row[3] = $"{generator.Next(Int32.MaxValue)}";
                    row[4] = true;
                    row[5] = $"{generator.Next(Int32.MaxValue)}";
                    row[6] = $"{generator.Next(Int32.MaxValue)}";

                    data.Rows.Add(row);
                }
            }
            return dataset;
        }
    }

    public enum Gender
    {
        Male,
        Female
    }

    [Serializable]
    public class Column
    {
        public Column()
        {
            Items = new List<Base> { };
            Date = DateTime.Now;
            MultilineString = @"
                AJKLjaskljLA
           ahjksjkAHJKS سلام فارسی
           AJKHSKJhaksjhAHSJKa
           AJKSHajkhsjkHKSJKash
           ASJKhasjkKASJKahsjk
                ";
            IsNew = true;
            BooleanValue = true;
            OrdinaryDouble = 0.001;
            Gender = Gender.Female;
            Intarray = new int[5] { 1, 2, 3, 4, 5 };
        }

        public bool BooleanValue { get; set; }

        public DateTime Date { get; set; }

        public string MultilineString { get; set; }

        public List<Base> Items { get; set; }

        public decimal OrdinaryDecimal { get; set; }

        public double OrdinaryDouble { get; set; }

        public bool IsNew { get; set; }

        public string LastString { get; set; }

        public Gender Gender { get; set; }

        public DataSet Data { get; set; }

        public Hashtable Hash { get; set; }

        public Dictionary<string, Base> StringDictionary { get; set; }

        public Dictionary<Base, Base> ObjectDictionary { get; set; }

        public Dictionary<int, Base> IntDictionary { get; set; }

        public Guid? NullableGUID { get; set; }

        public decimal? NullableDecimal { get; set; }

        public double? NullableDouble { get; set; }

        public Base[] ArrayType { get; set; }

        public byte[] Bytes { get; set; }

        public int[] Intarray { get; set; }
    }

    [Serializable]
    public class Base
    {
        public string Name { get; set; }

        public string Code { get; set; }

        public Base() { }

        public Base(string name, string code) => (Name, Code) = (name, code);
    }

    [Serializable]
    public class ClassA : Base
    {
        public Guid GUID { get; set; }

        public ClassA() { }

        public ClassA(string name, string code, Guid guid) : base(name, code) => GUID = guid;
    }

    [Serializable]
    public class ClassB : Base
    {
        public string Description { get; set; }

        public ClassB() { }

        public ClassB(string name, string code, string description) : base(name, code) => Description = description;
    }

    [Serializable]
    public class NoExtension
    {
        [System.Xml.Serialization.XmlIgnore()]
        public string Name { get; set; }
        public string Address { get; set; }
        public int Age { get; set; }
        public Base[] Objects { get; set; }
        public Dictionary<string, ClassA> Dictionary { get; set; }
        public NoExtension InternalData { get; set; }
    }

    [Serializable]
    public class ReturnClass
    {
        public string Field1;

        public int Field2;

        public object obj;

        public object ReturnEntity { get; set; }

        public string Name { get; set; }

        public DateTime Date { get; set; }

        public DataTable Data { get; set; }

        public string TestString => "sdfas df ";
    }

    [Serializable]
    public struct ReturnStruct
    {
        public string Field1;

        public int Field2;

        public object ReturnEntity { get; set; }

        public string Name { get; set; }

        public string ppp => "sdfas df ";

        public DateTime Date { get; set; }

        public DataTable Data { get; set; }
    }

    [Serializable]
    public class RetNestedclass
    {
        public ReturnClass Nested { get; set; }
    }
}
