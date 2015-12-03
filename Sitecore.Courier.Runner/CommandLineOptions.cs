using CommandLine;
using CommandLine.Text;

namespace Sitecore.Courier.Runner
{
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

        [Option('r', "rainbow", Required = false,
          HelpText = "Use Rainbow serializer")]
        public bool UseRainbow { get; set; }

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
