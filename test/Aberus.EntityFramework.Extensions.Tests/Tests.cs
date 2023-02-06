using NUnit.Framework;

namespace Aberus.Data.Entity.ModelConfiguration.Conventions
{
    public sealed class CamelCaseConverterTests
    {
        [Test]
        [TestCase("TestService", ExpectedResult = "testService")]
        [TestCase("TESTService", ExpectedResult = "testService")]
        [TestCase("TEST_Service", ExpectedResult = "testService")]
        public string ConvertToSnakeCaseTest(string input)
        {
            return CamelCaseConverter.Convert(input);
        }
    }

    [TestFixture]
    public sealed class SnakeCaseConverterTests
    {
        [Test]
        [TestCase("TestService", ExpectedResult = "test_service")]
        [TestCase("TESTService", ExpectedResult = "test_service")]
        [TestCase("TEST_Service", ExpectedResult = "test_service")]
        public string ConvertToSnakeCaseTest(string input)
        {
            return SnakeCaseConverter.Convert(input);
        }
    }

    public sealed class UpperSnakeCaseConverterTests
    {
        [Test]
        [TestCase("TestService", ExpectedResult = "TEST_SERVICE")]
        [TestCase("TESTService", ExpectedResult = "TEST_SERVICE")]
        [TestCase("TEST_Service", ExpectedResult = "TEST_SERVICE")]
        public string ConvertToSnakeCaseTest(string input)
        {
            return UpperSnakeCaseConverter.Convert(input);
        }
    }




}