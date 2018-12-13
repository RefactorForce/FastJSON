using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FastJSON.ConsoleTest
{
    public class TestClass
    {
        public string Channel { get; set; }
        public string Start { get; set; }
        public string Stop { get; set; }
        public string Eventid { get; set; }
        public string Charset { get; set; }

        public List<string> Titles { get; set; } = new List<string>();
        public List<string> Events { get; set; } = new List<string>();
        public List<string> Descriptions { get; set; } = new List<string>();

        public static List<TestClass> CreateList(int count)
        {
            List<TestClass> lst = new List<TestClass>();
            foreach (int i in Enumerable.Range(1, count))
            {
                TestClass t = new TestClass
                {
                    Channel = $"Channel-{i % 10}",
                    Start = $"{i * 1000}",
                    Stop = $"{i * 1000 + 10}",
                    Charset = "255"
                };
                t.Descriptions.Add($"Description Description Description Description Description Description Description {i} ");
                t.Events.Add($"Event {i} ");
                t.Titles.Add($"The Title {i} ");
                lst.Add(t);
            }
            return lst;
        }
    }
}
