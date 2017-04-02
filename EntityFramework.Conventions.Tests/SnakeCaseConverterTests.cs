using EnitityFramework.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace EnitityFramework.Conventions.Tests
{
    [TestFixture()]
    public class SnakeCaseConverterTests
    {
        [Test()]
        [TestCase("HpsmBizservis", ExpectedResult = "hpsm_bizservis")]
        [TestCase("HPSMBizservis", ExpectedResult = "hpsm_bizservis")]
        [TestCase("HPSM_Bizservis", ExpectedResult = "hpsm_bizservis")]
        public string ConvertToSnakeCaseTest(string input)
        {
            return SnakeCaseConverter.ConvertToSnakeCase(input);
        }
    }
}