using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using Sitecore.Courier.Rainbow;
using Sitecore.Update;
using Sitecore.Update.Engine;

namespace Sitecore.Courier
{
    [Cmdlet(VerbsCommon.New, "CourierPackage", DefaultParameterSetName = "default", SupportsShouldProcess = true)]
    [OutputType(new Type[] {typeof(string)})]
    [Alias("New-SitecorePackage")]
    public class NewCourierPackageCommand : Cmdlet
    {
        public static class ParameterSets
        {
            public const string DEFAULT = "default";
        }

        private const string PostDeployDll = "Sitecore.Courier.PackageInstallPostProcessor.dll";

        private const string PostStep =
                "Sitecore.Courier.PackageInstallPostProcessor.DoPostDeployActions, Sitecore.Courier.PackageInstallPostProcessor"
            ;

        [Parameter(Mandatory = false, Position = 0, ParameterSetName = ParameterSets.DEFAULT)]
        public string Source { get; set; }

        [Parameter(Mandatory = true, Position = 1, ParameterSetName = ParameterSets.DEFAULT)]
        public string Target { get; set; }

        [Parameter(Mandatory = true, Position = 2, ParameterSetName = ParameterSets.DEFAULT)]
        public string Output { get; set; }

        [Parameter(Mandatory = true, Position = 3, ParameterSetName = ParameterSets.DEFAULT)]
        public SerializationProvider SerializationProvider { get; set; }

        [Parameter(Mandatory = false, Position = 4, ParameterSetName = ParameterSets.DEFAULT)]
        public CollisionBehavior CollisionBehavior { get; set; }

        [Parameter(Mandatory = false, Position = 5, ParameterSetName = ParameterSets.DEFAULT)]
        public bool IncludeFiles { get; set; }

        [Parameter(Mandatory = false, Position = 6, ParameterSetName = ParameterSets.DEFAULT)]
        public bool IncludeSecurity { get; set; }

        protected override void BeginProcessing()
        {
            try
            {
                var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE",
                    Path.Combine(currentDirectory, "Sitecore.Courier.dll.config"));
                ResetConfigMechanism();
                string version = Guid.NewGuid().ToString();

                Console.WriteLine("Source: {0}", Source);
                Console.WriteLine("Target: {0}", Target);
                Console.WriteLine("Output: {0}", Output);
                Console.WriteLine("SerializationProvider: {0}", SerializationProvider);
                Console.WriteLine("CollisionBehavior: {0}", CollisionBehavior);
                Console.WriteLine("IncludeFiles: {0}", IncludeFiles);

                RainbowSerializationProvider.Enabled = SerializationProvider == SerializationProvider.Rainbow;
                RainbowSerializationProvider.IncludeFiles = IncludeFiles;

                var diff = new DiffInfo(
                    DiffGenerator.GetDiffCommands(Source, Target, IncludeSecurity, version, CollisionBehavior),
                    "Sitecore Courier Package",
                    string.Empty,
                    string.Format("Diff between serialization folders '{0}' and '{1}'.", Source, Target));

                if (IncludeSecurity)
                {
                    diff.Commands.Add(new PostStepFileSystemDataItem(currentDirectory, string.Empty, PostDeployDll)
                        .GenerateAddCommand().FirstOrDefault());
                    diff.PostStep = PostStep;
                    diff.Version = version;
                }

                PackageGenerator.GeneratePackage(diff, string.Empty, Output);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw ex;
            }
        }

        private static void ResetConfigMechanism()
        {
            typeof(ConfigurationManager)
                .GetField("s_initState", BindingFlags.NonPublic |
                                         BindingFlags.Static)
                .SetValue(null, 0);

            typeof(ConfigurationManager)
                .GetField("s_configSystem", BindingFlags.NonPublic |
                                            BindingFlags.Static)
                .SetValue(null, null);

            typeof(ConfigurationManager)
                .Assembly.GetTypes()
                .Where(x => x.FullName ==
                            "System.Configuration.ClientConfigPaths")
                .First()
                .GetField("s_current", BindingFlags.NonPublic |
                                       BindingFlags.Static)
                .SetValue(null, null);
        }
    }
}