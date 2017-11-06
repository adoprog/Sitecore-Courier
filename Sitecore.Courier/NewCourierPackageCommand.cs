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
  [OutputType(new Type[] { typeof(string) })]
  [Alias("New-SitecorePackage")]
  public class NewCourierPackageCommand : Cmdlet
  {
    public static class ParameterSets
    {
      public const string DEFAULT = "default";
    }

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

    protected override void BeginProcessing()
    {
      AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Sitecore.Courier.dll.config"));
      ResetConfigMechanism();

      Console.WriteLine("Source: {0}", Source);
      Console.WriteLine("Target: {0}", Target);
      Console.WriteLine("Output: {0}", Output);
      Console.WriteLine("SerializationProvider: {0}", SerializationProvider);
      Console.WriteLine("CollisionBehavior: {0}", CollisionBehavior);

      RainbowSerializationProvider.Enabled = SerializationProvider == SerializationProvider.Rainbow;

      var diff = new DiffInfo(
        DiffGenerator.GetDiffCommands(Source, Target, CollisionBehavior),
        "Sitecore Courier Package",
        string.Empty,
        string.Format("Diff between serialization folders '{0}' and '{1}'.", Source, Target));

      PackageGenerator.GeneratePackage(diff, string.Empty, Output);
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
