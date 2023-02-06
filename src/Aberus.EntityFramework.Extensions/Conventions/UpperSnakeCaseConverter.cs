namespace Aberus.Data.Entity.ModelConfiguration.Conventions
{
    public static class UpperSnakeCaseConverter
    {
        internal static string Convert(string input)
        {
            return SnakeCaseConverter.Convert(input).ToUpperInvariant();
        }
    }
}
