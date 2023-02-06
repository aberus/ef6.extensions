using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Aberus.Data.Entity.ModelConfiguration.Conventions
{
    public static class CamelCaseConverter
    {
        internal static string Convert(string input)
        {
            bool nextIsUpper = false;
            bool firstPart = true;
            StringBuilder result = new StringBuilder(input.Length);

            bool isUpper;
            bool wasUpper = true;
            for (int i = 0; i < input.Length; i++, wasUpper = isUpper)
            {
                isUpper = char.IsUpper(input[i]);
                if (input[i] == '_')
                {
                    nextIsUpper = true;
                    if (result.Length != 0)
                    {
                        firstPart = false;
                    }
                    continue;
                }
                else if (nextIsUpper)
                {
                    nextIsUpper = false;
                    if (char.IsLower(input[i]))
                    {
                        result.Append(char.ToUpperInvariant(input[i]));
                        continue;
                    }
                }
                else if (firstPart)
                {
                    if (result.Length != 0 && isUpper && (!wasUpper || (i + 1 < input.Length && char.IsLower(input[i + 1]))))
                    {
                        firstPart = false;
                    }
                    else
                    {
                        result.Append(char.ToLowerInvariant(input[i]));
                        continue;
                    }
                }

                result.Append(input[i]);
            }
            return result.ToString();
        }
    }
}
