using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Sitecore.Courier.Runner.ExclusionsTests
{
    public class ConfigurationTests
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
            var actual = ExclusionHandler.HasValidExclusions(null, null);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void NullBuildConfigurationWithFileTest()
        {
            var expected = false;
            var actual = ExclusionHandler.HasValidExclusions(null, "myfile.txt");
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void BuildConfigurationWithNoFilePathTest()
        {
            Assert.Throws<NullReferenceException>(() => ExclusionHandler.HasValidExclusions("myconfig", null));            
        }

        [Test]
        public void BuildConfigurationWithInvalidFilePathTest()
        {
            Assert.Throws<FileNotFoundException>(() => ExclusionHandler.HasValidExclusions("myconfig", "invalidfile.xml"));
        }

        [Test]
        public void HasValidBuildConfigurationAndFileTest()
        {
            var expected = true;
            var path = Environment.CurrentDirectory;
            var xmlPath = string.Concat(path.Substring(0, path.IndexOf("bin")), "sample.xml");
            var actual = ExclusionHandler.HasValidExclusions("myconfig", xmlPath);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void HasCorrectNumberOfExcludedItemsTest()
        {
            var expected = 3;
            var path = Environment.CurrentDirectory;
            var xmlPath = string.Concat(path.Substring(0, path.IndexOf("bin")), "sample.xml");
            var exclusions = ExclusionHandler.GetExcludedItems(xmlPath, "Test");
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
            var exclusions = ExclusionHandler.GetExcludedItems(xmlPath, "Test");
            Assert.AreEqual(expected, exclusions);  
        }

        [Test]
        public void DoesNotReturnIncorrectExcludedItemsTest()
        {
            var path = Environment.CurrentDirectory;
            var xmlPath = string.Concat(path.Substring(0, path.IndexOf("bin")), "sample.xml");
            var exclusions = ExclusionHandler.GetExcludedItems(xmlPath, "Test");
            Assert.False(exclusions.Contains("sitecore\\content\\mysite\\donotexcludefrombuild1.item"));
        }


        [Test]
        public void DoesNotReturnForConfigurationWithoutExclusionsTest()
        {
            var path = Environment.CurrentDirectory;
            var xmlPath = string.Concat(path.Substring(0, path.IndexOf("bin")), "sample.xml");
            var configuration = "NO_RESULTS_CONFIGURATION";

            var actual = ExclusionHandler.HasValidExclusions(configuration, xmlPath);
            Assert.True(actual);
            var exclusions = ExclusionHandler.GetExcludedItems(xmlPath, configuration);
            Assert.False(exclusions.Any());
        }
    }
}
