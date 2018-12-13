using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace FastJSON.ConsoleTest
{
    class DataObjects
    {
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
                Items = new List<Base>();
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

            public string Laststring { get; set; }

            public Gender Gender { get; set; }

            public DataSet Dataset { get; set; }

            public Dictionary<string, Base> StringDictionary { get; set; }

            public Dictionary<Base, Base> ObjectDictionary { get; set; }

            public Dictionary<int, Base> IntDictionary { get; set; }

            public Guid? NullableGuid { get; set; }

            public decimal? NullableDecimal { get; set; }

            public double? NullableDouble { get; set; }

            public Hashtable Hash { get; set; }

            public Base[] ArrayType { get; set; }

            public byte[] Bytes { get; set; }

            public int[] Intarray { get; set; }
        }
    }
}
