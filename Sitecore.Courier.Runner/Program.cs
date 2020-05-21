using Sitecore.Update;
using Sitecore.Update.Engine;
using System;
using Sitecore.Courier.Rainbow;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sitecore.Courier.DacPac;
using Sitecore.Courier.Sql;
using Sitecore.Update.Interfaces;

namespace Sitecore.Courier.Runner
{
    /// <summary>
    /// Defines the program class.
    /// </summary>
    internal class Program
    {
        private const string PostDeployDll = "Sitecore.Courier.PackageInstallPostProcessor.dll";
        private const string PostStep = "Sitecore.Courier.PackageInstallPostProcessor.DoPostDeployActions, Sitecore.Courier.PackageInstallPostProcessor";

        /// <summary>
        /// Mains the specified args.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                Console.WriteLine("Source: {0}", options.Source);
                Console.WriteLine("Target: {0}", options.Target);
                Console.WriteLine("Output: {0}", options.Output);
                Console.WriteLine("Collision behavior: {0}", options.CollisionBehavior);
                Console.WriteLine("Use Rainbow: {0}", options.UseRainbow);
                Console.WriteLine("Include Security: {0}", options.IncludeSecurity);
                Console.WriteLine("Include Files: {0}", options.IncludeFiles);
                Console.WriteLine("Configuration: {0}", options.Configuration);
                Console.WriteLine("Ensure Revision: {0}", options.EnsureRevision);
                Console.WriteLine("Path to project file: {0}", options.ScProjFilePath);
                Console.WriteLine("DacPac Output: {0}", options.DacPac);

                string version = Guid.NewGuid().ToString();
                SanitizeOptions(options);

                if (ExclusionHandler.HasValidExclusions(options.Configuration, options.ScProjFilePath))
                {
                    var exclusions = ExclusionHandler.GetExcludedItems(options.ScProjFilePath, options.Configuration);

                    ExclusionHandler.RemoveExcludedItems(options.Source, exclusions);
                    ExclusionHandler.RemoveExcludedItems(options.Target, exclusions);
                }

                RainbowSerializationProvider.Enabled = options.UseRainbow;
                RainbowSerializationProvider.IncludeFiles = options.IncludeFiles;
                RainbowSerializationProvider.EnsureRevision = options.EnsureRevision;

                var commands = DiffGenerator.GetDiffCommands(options.Source, options.Target, options.IncludeSecurity, version, options.CollisionBehavior);

                var diff = new DiffInfo(
                    commands,
                    "Sitecore Courier Package",
                    string.Empty,
                    string.Format("Diff between serialization folders '{0}' and '{1}'.", options.Source, options.Target));

                if (options.IncludeSecurity)
                {
                    var currentDirectory = Directory.GetCurrentDirectory();
                    commands.Add(new PostStepFileSystemDataItem(currentDirectory, string.Empty, PostDeployDll)
                        .GenerateAddCommand().FirstOrDefault());
                    diff.PostStep = PostStep;
                    diff.Version = version;
                }

                if (options.DacPac)
                {
                  SqlConverter c = new SqlConverter();
                  c.ConvertPackage(diff, options.Output);

                  var builder = new DacPacBuilder();
                  DirectoryInfo d = new DirectoryInfo(options.Output);
                  foreach (var file in d.GetFiles("*.sql"))
                  {
                    builder.ConvertToDacPac(file.FullName, Path.Combine(file.DirectoryName, $"{Path.GetFileNameWithoutExtension(file.Name)}.dacpac"));
                  }
                }
                else
                {
                  PackageGenerator.GeneratePackage(diff, string.Empty, options.Output);
                }
            }
            else
            {
                Console.WriteLine(options.GetUsage());
            }
        }

        private static void SanitizeOptions(Options options)
        {
          if (options.Source != null)
          {
            options.Source = options.Source.Replace("'", string.Empty);
          }

          if (options.Target != null)
          {
            options.Target = options.Target.Replace("'", string.Empty);
          }

          if (options.Output != null)
          {
            options.Output = options.Output.Replace("'", string.Empty);
          }
        }
    }
}
