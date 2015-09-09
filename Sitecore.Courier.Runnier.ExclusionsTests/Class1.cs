using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Sitecore.Courier.Runner;

namespace Sitecore.Courier.Runner.ExclusionsTests
{
    public class Class1
    {
        [Test]
        public void ArgumentParserTest()
        {
            var options = new Options();
            var args = new string[]
            {
                "-s",
                "sourcepath",
                "-t",
                "targetpath",
                "-o",
                "outputfile",
                "-c",
                "configuration",
                "-p",
                "tdsfilepath"
            };
            
            Assert.DoesNotThrow(() => CommandLine.Parser.Default.ParseArguments(args, options));
        }

        [Test]
        public void NullBuildConfigurationTest()
        {
            var expected = false;
            var actual = ExclusionValidator.HasValidExclusions(null, null);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void NullBuildConfigurationWithFileTest()
        {
            var expected = false;
            var actual = ExclusionValidator.HasValidExclusions(null, "myfile.txt");
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void BuildConfigurationWithNoFilePathTest()
        {
            Assert.Throws<NullReferenceException>(() => ExclusionValidator.HasValidExclusions("myconfig", null));            
        }

        [Test]
        public void BuildConfigurationWithInvalidFilePathTest()
        {
            Assert.Throws<FileNotFoundException>(() => ExclusionValidator.HasValidExclusions("myconfig", "invalidfile.xml"));
        }
    }
}
