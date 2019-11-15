using System.Collections.Generic;
using System.Configuration;
using Rainbow.Storage.Yaml;
using Sitecore.Courier.Rainbow.Configuration;
using Sitecore.Update;
using Sitecore.Update.Filters;
using Sitecore.Update.Interfaces;

namespace Sitecore.Courier.Rainbow
{
    /// <summary>
    /// The File system data provider.
    /// 
    /// </summary>
    public class RainbowSerializationProvider : BaseDataProcessor, IDataProvider
    {
        public static bool Enabled = false;
        public static bool IncludeFiles = false;
        private readonly string _name;
        private readonly YamlSerializationFormatter _formatter;

        public RainbowSerializationProvider(string name) : base(name)
        {
            _name = name;

            var config = (RainbowConfigSection)ConfigurationManager.GetSection("rainbow");
            var rainbowConfigFactory = new RainbowConfigFactory(config);
            _formatter = rainbowConfigFactory.CreateFormatter() as YamlSerializationFormatter;
        }

        public object Clone()
        {
            return new RainbowSerializationProvider(_name);
        }

        public bool CanProcess(string sourcePath)
        {
            return Enabled;
        }

        public IDataIterator GetIterator(string rootPath, IList<Filter> filters)
        {
            return new RainbowIterator(rootPath, _formatter);
        }
    }
}
