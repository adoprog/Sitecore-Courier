using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sitecore.Update.Commands;
using Sitecore.Update.Data.Items;
using Sitecore.Update.Interfaces;

namespace Sitecore.Courier
{
    public class RoleFileSystemDataItem : FileSystemDataItem
    {
        public RoleFileSystemDataItem(string root, string s, string fileName) : base(root, s, fileName)
        {
        }

        public override IList<ICommand> GenerateAddCommand()
        {
            var commands = base.GenerateAddCommand();
            var field = typeof(BaseFileCommand).GetField("relatedFilePath", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(commands.FirstOrDefault(), $"temp\\{DiffGenerator.Version}\\roles\\{this.Name}");
            return commands;
        }
    }
}