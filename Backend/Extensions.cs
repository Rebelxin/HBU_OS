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
    }
}
