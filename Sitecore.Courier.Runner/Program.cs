namespace Sitecore.Courier.Runner
{
  using Sitecore.Update;
  using Sitecore.Update.Engine;
  using System;

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
       string.Format("Diff between serialization folders '{0}' and '{1}'.", sourcePath, targetPath));

      PackageGenerator.GeneratePackage(diff, string.Empty, outputPath);
    }

    /// <summary>
    /// Gets the argument.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="args">The arguments.</param>
    /// <returns></returns>
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

      Console.WriteLine("Enter the path to " + name + " folder/file: ");
      return Console.ReadLine();
    }

    /// <summary>
    /// Gets the name of the formatted argument.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns></returns>
    public static string GetFormattedArgumentName(string name)
    {
      return string.Format("/{0}:", name.ToLowerInvariant());
    }
  }
}
