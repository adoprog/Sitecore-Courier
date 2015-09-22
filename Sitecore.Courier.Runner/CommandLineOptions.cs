using CommandLine;
using CommandLine.Text;

namespace Sitecore.Courier.Runner
{
    using Sitecore.Update;

    // Define a class to receive parsed values
    class Options
    {
        [Option('s', "source", Required = true,
          HelpText = "Source files")]
        public string Source { get; set; }

        [Option('t', "target", Required = true,
          HelpText = "Target files")]
        public string Target { get; set; }

        [Option('o', "output", Required = true,
          HelpText = "Location of update package")]
        public string Output { get; set; }
        
        [Option('c', "collisionBehavior", Required = false, DefaultValue = CollisionBehavior.Undefined,
          HelpText = "The collision behavior (default, force or skip) for the update package.")]
        public CollisionBehavior CollisionBehavior { get; set; }

        [Option('c', "configuration", Required = false,
            HelpText = "Build Configuration to diff")]
        public string Configuration { get; set; }

        [Option('p', "exclusionsfilepath", Required = false,
          HelpText = "Path to exclusions file if excluding for build configuration")]
        public string ScProjFilePath { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
              (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
