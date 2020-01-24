using System.Text.RegularExpressions;

namespace Aberus.Data.Entity.ModelConfiguration.Conventions
{
    public static class SnakeCaseConverter
    {
        internal static string ConvertToSnakeCase(string input)
        {
            var result = Regex.Replace(input, "(.)([A-Z][a-z]+)", m => m.Groups[1].Value + "_" + m.Groups[2].Value);
            result = Regex.Replace(result, "(.)([0-9]+)", m => m.Groups[1].Value + "_" + m.Groups[2].Value);
            result = Regex.Replace(result, "([a-z0-9])([A-Z])", m => m.Groups[1].Value + "_" + m.Groups[2].Value);
            result = Regex.Replace(result, "_+", "_");

            return result.ToLowerInvariant();
        }
    }
}
