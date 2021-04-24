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
        
        [Test]
        public void DiffGeneratorOperationsDisabledByOptions()
        {
            var options = new Runner.Options
            {
                DisableAddOperations = true,
                DisableDeleteOperations = true,
                DisableUpdateOperations = true
            };
            
            SetDiffGeneratorOptions(options);

            Assert.IsFalse(DiffGenerator.AllowedOperations.HasFlag(AllowedOperations.Create));
            Assert.IsFalse(DiffGenerator.AllowedOperations.HasFlag(AllowedOperations.Update));
            Assert.IsFalse(DiffGenerator.AllowedOperations.HasFlag(AllowedOperations.Delete));
        }

        [Test]
        public void DiffGeneratorOperationsEnableAddByOptions()
        {
            var options = new Runner.Options
            {
                DisableAddOperations = false,
                DisableDeleteOperations = true,
                DisableUpdateOperations = true
            };
            
            SetDiffGeneratorOptions(options);
            
            Assert.IsTrue(DiffGenerator.AllowedOperations.HasFlag(AllowedOperations.Create));
            Assert.IsFalse(DiffGenerator.AllowedOperations.HasFlag(AllowedOperations.Update));
            Assert.IsFalse(DiffGenerator.AllowedOperations.HasFlag(AllowedOperations.Delete));
        }
        
        [Test]
        public void DiffGeneratorOperationsEnableAddAndDeleteByOptions()
        {
            var options = new Runner.Options
            {
                DisableAddOperations = false,
                DisableDeleteOperations = false,
                DisableUpdateOperations = true
            };
            
            SetDiffGeneratorOptions(options);
            
            Assert.IsTrue(DiffGenerator.AllowedOperations.HasFlag(AllowedOperations.Create));
            Assert.IsTrue(DiffGenerator.AllowedOperations.HasFlag(AllowedOperations.Delete));
            Assert.IsFalse(DiffGenerator.AllowedOperations.HasFlag(AllowedOperations.Update));
        }
        
        [Test]
        public void DiffGeneratorOperationsEnableAllByOptions()
        {
            var options = new Runner.Options
            {
                DisableAddOperations = false,
                DisableDeleteOperations = false,
                DisableUpdateOperations = false
            };
            
            SetDiffGeneratorOptions(options);
            
            Assert.IsTrue(DiffGenerator.AllowedOperations.HasFlag(AllowedOperations.Create));
            Assert.IsTrue(DiffGenerator.AllowedOperations.HasFlag(AllowedOperations.Update));
            Assert.IsTrue(DiffGenerator.AllowedOperations.HasFlag(AllowedOperations.Delete));
        }
        
        private static void SetDiffGeneratorOptions(Runner.Options options)
        {
            DiffGenerator.AllowedOperations = AllowedOperations.Create | AllowedOperations.Delete | AllowedOperations.Update;
            DiffGenerator.AllowedOperations &= options.DisableAddOperations ? ~AllowedOperations.Create : DiffGenerator.AllowedOperations;
            DiffGenerator.AllowedOperations &= options.DisableDeleteOperations ? ~AllowedOperations.Delete : DiffGenerator.AllowedOperations;
            DiffGenerator.AllowedOperations &= options.DisableUpdateOperations ? ~AllowedOperations.Update : DiffGenerator.AllowedOperations;
        }
    }
}
