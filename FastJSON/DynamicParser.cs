using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace FastJSON
{
    internal class DynamicParser : DynamicObject, IEnumerable
    {
        IDictionary<string, object> ResultDictionary { get; set; }

        List<object> ResultList { get; set; }

        public DynamicParser(string json)
        {
            object parse = Utility.Parse(json);

            if (parse is IDictionary<string, object>)
                ResultDictionary = (IDictionary<string, object>)parse;
            else
                ResultList = (List<object>)parse;
        }

        DynamicParser(object dictionary) => ResultDictionary = dictionary as Dictionary<string, object> ?? ResultDictionary;

        public override IEnumerable<string> GetDynamicMemberNames() => ResultDictionary.Keys.ToList();

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            object index = indexes[0];
            result = index is int ? ResultList[(int) index] : ResultDictionary[(string) index];
            if (result is IDictionary<string, object>)
                result = new DynamicParser(result as IDictionary<string, object>);
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (ResultDictionary.TryGetValue(binder.Name, out result) == false)
                if (ResultDictionary.TryGetValue(binder.Name.ToLowerInvariant(), out result) == false)
                    return false;// throw new Exception("property not found " + binder.Name);

            if (result is IDictionary<string, object>)
            {
                result = new DynamicParser(result as IDictionary<string, object>);
            }
            else if (result is List<object>)
            {
                List<object> list = new List<object> { };
                foreach (object item in (List<object>)result)
                {
                    if (item is IDictionary<string, object>)
                        list.Add(new DynamicParser(item as IDictionary<string, object>));
                    else
                        list.Add(item);
                }
                result = list;
            }

            return ResultDictionary.ContainsKey(binder.Name);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach(object o in ResultList)
            {
                yield return new DynamicParser(o as IDictionary<string, object>);
            }
        }
    }
}