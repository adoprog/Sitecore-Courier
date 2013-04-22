namespace Sitecore.Courier.Runner
{
  using System;
  using System.Collections.Generic;

  using Sitecore.Update;
  using Sitecore.Update.Engine;

  /// <summary>
  /// Defines the program class.
  /// </summary>
  internal class Program
  {
    /// <summary>
    /// Mains the specified args.
    /// </summary>
    /// <param name="args">The arguments.</param>
    private static void Main(string[] args)
    {
      for (int i = 0; i < args.Length; i++)
      {
        string s = args[i];
        Console.WriteLine("\t" + s);
      }

      string sourcePath = GetArgument("source", args);
      string targetPath = GetArgument("target", args);
      string outputPath = GetArgument("output", args);
      
      var diff = new DiffInfo(
        DiffGenerator.GetDiffCommands(sourcePath, targetPath),
        "Sitecore Courier Package",
        string.Empty,
        string.Format("Diff between folders '{0}' and '{1}'", sourcePath, targetPath));
      PackageGenerator.GeneratePackage(diff, string.Empty, outputPath);
    }

    public static string GetArgument(string name, string[] args)
    {
      string argument = GetFormattedArgumentName(name);
      for (int i = 0; i < args.Length; i++)
      {
        string param = args[i];
        if (param.Trim().ToLowerInvariant().StartsWith(argument))
        {
          return param.Substring(argument.Length).Trim();
        }
      }

      Console.WriteLine("Enter the path to " + name + " folder: ");
      return Console.ReadLine();
    }

    public static string GetFormattedArgumentName(string name)
    {
      return string.Format("/{0}:", name.ToLowerInvariant());
    }
  }
}
