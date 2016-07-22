using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Rainbow.Storage;
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

    public class RainbowIterator : IDataIterator
    {
        private readonly string _rootPath;
        private readonly YamlSerializationFormatter _formatter;
        private FileData[] _allFiles;
        private int _currentPosition;

        public RainbowIterator(string rootPath, YamlSerializationFormatter formatter)
        {
            _rootPath = rootPath;
            _formatter = formatter;
            _currentPosition = 0;
            InitStack();
        }

        private void InitStack()
        {
            _allFiles = FastDirectoryEnumerator.GetFiles(_rootPath, "*" + _formatter.FileExtension, SearchOption.AllDirectories);
        }

        public IDataItem Next()
        {
            if (_allFiles == null || _allFiles.Length == 0 || _allFiles.Length == _currentPosition)
                return null;
            var file = _allFiles[_currentPosition];
            _currentPosition++;
            var dir = Path.GetDirectoryName(file.Path);
            var relative = _rootPath.Length == dir.Length ? string.Empty : dir.Substring(_rootPath.Length);
            return new RainbowDataItem(
                _rootPath,
                relative,
                file.Name,
                _formatter);
        }
    }
}
