using CommandLine;
using CommandLine.Text;
using Sitecore.Update;

namespace Sitecore.Courier.Runner
{
    // Define a class to receive parsed values
    public class Options
    {
        [Option('s', "source", Required = false,
          HelpText = "Source files")]
        public string Source { get; set; }

        [Option('t', "target", Required = true,
          HelpText = "Target files")]
        public string Target { get; set; }

        [Option('o', "output", Required = true,
            HelpText = "Location of update package")]
        public string Output { get; set; }

        [Option('b', "behavior", Required = false, DefaultValue = CollisionBehavior.Undefined,
          HelpText = "The collision behavior (default, force or skip) for the update package.")]
        public CollisionBehavior CollisionBehavior { get; set; }

        [Option('c', "configuration", Required = false,
            HelpText = "Build Configuration to diff")]
        public string Configuration { get; set; }

        [Option('p', "exclusionsfilepath", Required = false,
          HelpText = "Path to exclusions file if excluding for build configuration")]
        public string ScProjFilePath { get; set; }

        [Option('r', "rainbow", Required = false,
          HelpText = "Use Rainbow serializer")]
        public bool UseRainbow { get; set; }

        [Option('d', "dacpac", Required = false,
          HelpText = "DacPac Output")]
        public bool DacPac { get; set; }

        [Option('e', "ensurerevision", Required = false,
            HelpText = "Ensure revision")]
        public bool EnsureRevision { get; set; }

        [Option('f', "files", Required = false,
          HelpText = "Include files when using Rainbow serializer")]
        public bool IncludeFiles { get; set; }

        [Option('i', "includesecurity", Required = false,
            HelpText = "Include security - roles and users. Only supported when using Rainbow serializer")]
        public bool IncludeSecurity { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        
        [Option('a', "addoperations", Required = false,
            HelpText = "Disable add operations")]
        public bool DisableAddOperations { get; set; }
        
        [Option('d', "deleteoperations", Required = false,
            HelpText = "Disable delete operations")]
        public bool DisableDeleteOperations { get; set; }
        
        [Option('u', "updateoperations", Required = false,
            HelpText = "Disable update operations")]
        public bool DisableUpdateOperations { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
              (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
