using System.IO;
using Rainbow.Storage.Yaml;
using Sitecore.Update.Interfaces;

namespace Sitecore.Courier.Rainbow
{
    public class RainbowIterator : IDataIterator
    {
        private const string ItemPrefix = "ID:";
        private const string UserPrefix = "Username";
        private const string RolePrefix = "Role";
        private readonly string _rootPath;
        private readonly YamlSerializationFormatter itemFormatter;
        private string[] _allFiles;
        private int _currentPosition;

        public RainbowIterator(string rootPath, YamlSerializationFormatter itemFormatter)
        {
            _rootPath = Path.GetFullPath(rootPath);
            this.itemFormatter = itemFormatter;
            _currentPosition = 0;
            InitStack();
        }

        private void InitStack()
        {
            if (string.IsNullOrEmpty(_rootPath))
            {
                _allFiles = new string[0];
            }

            if (RainbowSerializationProvider.IncludeFiles)
            {
                _allFiles = Directory.GetFiles(_rootPath, "*", SearchOption.AllDirectories);
            }
            else
            {
                _allFiles = Directory.GetFiles(_rootPath, "*" + itemFormatter.FileExtension, SearchOption.AllDirectories);
            }
        }

        public IDataItem Next()
        {
            if (_allFiles == null || _allFiles.Length == 0 || _allFiles.Length == _currentPosition)
                return null;
            var file = _allFiles[_currentPosition];
            _currentPosition++;
            var dir = Path.GetDirectoryName(file);
            var name = Path.GetFileName(file);
            var relative = _rootPath.Length == dir.Length ? string.Empty : dir.Substring(_rootPath.Length);

            if (file.EndsWith(itemFormatter.FileExtension))
            {
                string itemType = RainbowItemExtensions.GetItemType(file);

                if (itemType.StartsWith(ItemPrefix))
                {
                    var item = new RainbowDataItem(
                        _rootPath,
                        relative,
                        name,
                        itemFormatter);

                    if (item.HasItem)
                    {
                        return item;
                    }
                }
                else if (itemType.StartsWith(UserPrefix) && DiffGenerator.IncludeSecurity)
                {
                    return ItemUtils.GetUserFileSystemDataItem(_rootPath, new FileInfo(file));
                }
                else if (itemType.StartsWith(RolePrefix) && DiffGenerator.IncludeSecurity)
                {
                    return ItemUtils.GetRoleFileSystemDataItem(_rootPath, new FileInfo(file));
                }

                return Next();
            }

            return ItemUtils.GetFileSystemDataItem(_rootPath, new FileInfo(file));
        }
    }
}