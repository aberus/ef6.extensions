using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aberus.Data.Entity.Common
{
    internal class Check
    {
        public static T NotNull<T>(T obj, string parameterName)
            where T : class
        {
            return obj ?? throw new ArgumentNullException(parameterName);
        }

        public static string ThrowIfWhitespace(string value, string parameterName)
        {
            return value.Trim().Length == 0 ?
                throw new ArgumentException("Argument cannot be an empty string or whitespace", parameterName)
                : value;
        }
    }
}
