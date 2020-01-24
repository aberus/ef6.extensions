using NUnit.Framework;

namespace Aberus.Data.Entity.ModelConfiguration.Conventions
{
    [TestFixture]
    public sealed class SnakeCaseConverterTests
    {
        [Test]
        [TestCase("HpsmBizservis", ExpectedResult = "hpsm_bizservis")]
        [TestCase("HPSMBizservis", ExpectedResult = "hpsm_bizservis")]
        [TestCase("HPSM_Bizservis", ExpectedResult = "hpsm_bizservis")]
        public string ConvertToSnakeCaseTest(string input)
        {
            return SnakeCaseConverter.ConvertToSnakeCase(input);
        }
    }
}