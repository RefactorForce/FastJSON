using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace FastJSON
{
    class Helper
    {
        public static bool IsNullable(Type t) => t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>));

        public static Type UnderlyingTypeOf(Type t) => Reflection.Instance.GetGenericArguments(t)[0];

        public static DateTimeOffset CreateDateTimeOffset(int year, int month, int day, int hour, int min, int sec, int milli, int extraTicks, TimeSpan offset)
        {
            DateTimeOffset dt = new DateTimeOffset(year, month, day, hour, min, sec, milli, offset);

            if (extraTicks > 0)
                dt += TimeSpan.FromTicks(extraTicks);

            return dt;
        }

        public static bool BoolConv(object v)
        {
            bool oset = false;
            switch (v)
            {
                case bool b:
                    oset = b;
                    break;
                case long l:
                    oset = l > 0;
                    break;
                case string s when s.ToLowerInvariant() is string sL && (sL == "1" || sL == "true" || sL == "yes" || sL == "on"):
                    oset = true;
                    break;
            }

            return oset;
        }

        public static long AutoConv(object value)
        {
            switch (value)
            {
                case string s:
                    return CreateLong(s, 0, s.Length);
                case long l:
                    return l;
                default:
                    return Convert.ToInt64(value);
            }
        }

        public static long CreateLong(string s, int index, int count)
        {
            long num = 0;
            bool neg = false;

            for (int x = 0; x < count; x++, index++)
            {
                char cc = s[index];

                switch (cc)
                {
                    case '-':
                        neg = true;
                        break;
                    case '+':
                        neg = false;
                        break;
                    default:
                        num *= 10;
                        num += cc - '0';
                        break;
                }
            }

            if (neg)
                num = -num;

            return num;
        }

        public static long CreateLong(char[] s, int index, int count)
        {
            long num = 0;
            bool neg = false;

            for (int x = 0; x < count; x++, index++)
            {
                char cc = s[index];

                switch (cc)
                {
                    case '-':
                        neg = true;
                        break;
                    case '+':
                        neg = false;
                        break;
                    default:
                        num *= 10;
                        num += cc - '0';
                        break;
                }
            }

            if (neg)
                num = -num;

            return num;
        }

        public static int CreateInteger(string s, int index, int count)
        {
            int num = 0;
            bool neg = false;

            for (int x = 0; x < count; x++, index++)
            {
                char cc = s[index];

                switch (cc)
                {
                    case '-':
                        neg = true;
                        break;
                    case '+':
                        neg = false;
                        break;
                    default:
                        num *= 10;
                        num += cc - '0';
                        break;
                }
            }

            if (neg) num = -num;

            return num;
        }

        public static object CreateEnum(Type pt, object v) => Enum.Parse(pt, v.ToString(), true);

        public static Guid CreateGuid(string s) => s.Length > 30 ? new Guid(s) : new Guid(Convert.FromBase64String(s));

        public static StringDictionary CreateSD(Dictionary<string, object> d)
        {
            StringDictionary nv = new StringDictionary { };

            foreach (KeyValuePair<string, object> o in d)
                nv.Add(o.Key, (string)o.Value);

            return nv;
        }

        public static NameValueCollection CreateNV(Dictionary<string, object> d)
        {
            NameValueCollection nv = new NameValueCollection();

            foreach (KeyValuePair<string, object> o in d)
                nv.Add(o.Key, (string)o.Value);

            return nv;
        }

        public static object CreateDateTimeOffset(string value)
        {
            //                   0123456789012345678 9012 9/3 0/4  1/5
            // datetime format = yyyy-MM-ddTHH:mm:ss .nnn  _   +   00:00

            // ISO8601 roundtrip formats have 7 digits for ticks, and no space before the '+'
            // datetime format = yyyy-MM-ddTHH:mm:ss .nnnnnnn  +   00:00  
            // datetime format = yyyy-MM-ddTHH:mm:ss .nnnnnnn  Z  

            bool neg = false;

            int year;
            int month;
            int day;
            int hour;
            int min;
            int sec;
            int ms = 0;
            int usTicks = 0; // ticks for xxx.x microseconds
            int th = 0;
            int tm = 0;

            year = CreateInteger(value, 0, 4);
            month = CreateInteger(value, 5, 2);
            day = CreateInteger(value, 8, 2);
            hour = CreateInteger(value, 11, 2);
            min = CreateInteger(value, 14, 2);
            sec = CreateInteger(value, 17, 2);

            int p = 20;

            if (value.Length > 21 && value[19] == '.')
            {
                ms = CreateInteger(value, p, 3);
                p = 23;

                // handle 7 digit case
                if (value.Length > 25 && Char.IsDigit(value[p]))
                {
                    usTicks = CreateInteger(value, p, 4);
                    p = 27;
                }
            }

            if (value[p] == 'Z')
                // UTC
                return CreateDateTimeOffset(year, month, day, hour, min, sec, ms, usTicks, TimeSpan.Zero);

            if (value[p] == ' ' || (neg = value[p + 1] == '-'))
                ++p;

            // +00:00
            th = CreateInteger(value, p + 1, 2);
            tm = CreateInteger(value, p + 1 + 2 + 1, 2);

            if (neg || value[p] == '-')
                th = -th;

            return CreateDateTimeOffset(year, month, day, hour, min, sec, ms, usTicks, new TimeSpan(th, tm, 0));
        }

        public static DateTime CreateDateTime(string value, bool UseUTCDateTime)
        {
            if (value.Length < 19)
                return DateTime.MinValue;

            bool utc = false;
            //                   0123456789012345678 9012 9/3
            // datetime format = yyyy-MM-ddTHH:mm:ss .nnn  Z
            int year;
            int month;
            int day;
            int hour;
            int min;
            int sec;
            int ms = 0;

            year = CreateInteger(value, 0, 4);
            month = CreateInteger(value, 5, 2);
            day = CreateInteger(value, 8, 2);
            hour = CreateInteger(value, 11, 2);
            min = CreateInteger(value, 14, 2);
            sec = CreateInteger(value, 17, 2);
            if (value.Length > 21 && value[19] == '.')
                ms = CreateInteger(value, 20, 3);

            if (value[value.Length - 1] == 'Z')
                utc = true;

            return UseUTCDateTime == false && utc == false
                ? new DateTime(year, month, day, hour, min, sec, ms)
                : new DateTime(year, month, day, hour, min, sec, ms, DateTimeKind.Utc).ToLocalTime();
        }
    }
}
