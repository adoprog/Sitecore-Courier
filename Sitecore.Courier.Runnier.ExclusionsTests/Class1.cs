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

        [Test]
        public void HasValidBuildConfigurationAndFileTest()
        {
            var expected = true;
            var path = Environment.CurrentDirectory;
            var xmlPath = string.Concat(path.Substring(0, path.IndexOf("bin")), "sample.xml");
            var actual = ExclusionValidator.HasValidExclusions("myconfig", xmlPath);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void HasCorrectNumberOfExcludedItemsTest()
        {
            var expected = 3;
            var path = Environment.CurrentDirectory;
            var xmlPath = string.Concat(path.Substring(0, path.IndexOf("bin")), "sample.xml");
            var exclusions = ExclusionReader.GetExcludedItems(xmlPath, "Test");
            Assert.AreEqual(expected, exclusions.Count);            
        }

        [Test]
        public void ReturnsCorrectExcludedItemsTest()
        {
            var expected = new string[]
            {
                "sitecore\\content\\mysite\\excludefrombuild1.item", 
                "sitecore\\content\\mysite\\excludefrombuild2.item",
                "sitecore\\content\\mysite\\excludefrombuild3.item"
            };

            var path = Environment.CurrentDirectory;
            var xmlPath = string.Concat(path.Substring(0, path.IndexOf("bin")), "sample.xml");
            var exclusions = ExclusionReader.GetExcludedItems(xmlPath, "Test");
            Assert.AreEqual(expected, exclusions);  
        }

        [Test]
        public void DoesNotReturnIncorrectExcludedItemsTest()
        {
            var path = Environment.CurrentDirectory;
            var xmlPath = string.Concat(path.Substring(0, path.IndexOf("bin")), "sample.xml");
            var exclusions = ExclusionReader.GetExcludedItems(xmlPath, "Test");
            Assert.False(exclusions.Contains("sitecore\\content\\mysite\\donotexcludefrombuild1.item"));
        }


        [Test]
        public void DoesNotReturnForConfigurationWithoutExclusionsTest()
        {
            var path = Environment.CurrentDirectory;
            var xmlPath = string.Concat(path.Substring(0, path.IndexOf("bin")), "sample.xml");
            var configuration = "NO_RESULTS_CONFIGURATION";

            var actual = ExclusionValidator.HasValidExclusions(configuration, xmlPath);
            Assert.True(actual);
            var exclusions = ExclusionReader.GetExcludedItems(xmlPath, configuration);
            Assert.False(exclusions.Any());
        }
    }
}
