using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend
{
    internal static class StringExtensions
    {
        public static string Repeat(this string str, int count)
        {
            return string.Concat(Enumerable.Repeat(str, count));
        }

        public static string CheckFileName(this string str)
        {
            return str.CheckName(3);
        }

        private static string CheckName(this string str, int limit)
        {
            if (str.Length < limit)
            {
                return str + "\0".Repeat(limit - str.Length);
            }
            if (str.Length > limit)
            {
                return str.Substring(0, limit);
            }
            return str;
        }

        public static string CheckExtendedName(this string str)
        {
            return str.CheckName(2);
        }
    }
}
