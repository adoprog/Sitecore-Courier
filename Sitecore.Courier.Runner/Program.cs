using Sitecore.Update;
using Sitecore.Update.Engine;
using System;

namespace Sitecore.Courier.Runner
{
    /// <summary>
    /// Defines the program class.
    /// </summary>
    internal class Program
    {
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
                Console.WriteLine("Configuration: {0}", options.Configuration);
                Console.WriteLine("Path to project file: {0}", options.ScProjFilePath);

                if (ExclusionHandler.HasValidExclusions(options.Configuration, options.ScProjFilePath))
                {
                    var exclusions = ExclusionHandler.GetExcludedItems(options.ScProjFilePath, options.Configuration);

                    ExclusionHandler.RemoveExcludedItems(options.Source, exclusions);
                    ExclusionHandler.RemoveExcludedItems(options.Target, exclusions);
                }

                var diff = new DiffInfo(
                    DiffGenerator.GetDiffCommands(options.Source, options.Target, options.CollisionBehavior),
                    "Sitecore Courier Package",
                    string.Empty,
                    string.Format("Diff between serialization folders '{0}' and '{1}'.", options.Source, options.Target));

                PackageGenerator.GeneratePackage(diff, string.Empty, options.Output);
            }
            else
            {
                Console.WriteLine(options.GetUsage());
            }
        }
    }
}
