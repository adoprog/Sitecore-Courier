using Sitecore.Courier.Iterators;
using Sitecore.Update;
using Sitecore.Update.Configuration;
using Sitecore.Update.Data;
using Sitecore.Update.Data.Items;
using Sitecore.Update.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sitecore.Courier
{
    /// <summary>
    /// Defines the Diff generator class.
    /// </summary>
    public class DiffGenerator
    {
        public static bool IncludeSecurity { get; set; }
        public static string Version { get; set; }
        public static AllowedOperations AllowedOperations { get; set; } = AllowedOperations.Create | AllowedOperations.Delete | AllowedOperations.Update;

        public static List<ICommand> GetDiffCommands(string sourcePath, string targetPath, bool includeSecurity, string version, CollisionBehavior collisionBehavior = CollisionBehavior.Undefined)
        {
            IncludeSecurity = includeSecurity;
            Version = version;

            var sourceManager = Factory.Instance.GetSourceDataManager();
            var targetManager = Factory.Instance.GetTargetDataManager();

            sourceManager.SerializationPath = sourcePath;
            targetManager.SerializationPath = targetPath;

            IDataIterator sourceDataIterator = sourceManager.ItemIterator ?? new EmptyIterator();
            IDataIterator targetDataIterator = targetManager.ItemIterator;
            
            var commands = GetCommands(sourceDataIterator, targetDataIterator);
            var engine = new DataEngine();
            engine.ProcessCommands(ref commands);
            return commands;
        }

        public static List<ICommand> GetCommands(IDataIterator sourceDataIterator, IDataIterator targetDataIterator)
        {
            var source = Map(sourceDataIterator);
            var target = Map(targetDataIterator);

            var commands = GetCommands(source, target);
            return commands;
        }

        /// <summary>
        /// Goes through source and target and creates add/update/delete commands as needed
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        protected static List<ICommand> GetCommands(Dictionary<string, IDataItem> source, Dictionary<string, IDataItem> target)
        {
            var commands = new List<ICommand>();
            //Iterate through all source items and match them in target. If there's a match, update it and remove from target. Else delete
            foreach (var old in source)
            {
                if (AllowedOperations.HasFlag(AllowedOperations.Update) &&  target.ContainsKey(old.Key))
                {
                    commands.AddRange(old.Value.GenerateUpdateCommand(target[old.Key]));
                    target.Remove(old.Key);
                }
                else if(AllowedOperations.HasFlag(AllowedOperations.Delete))
                {
                    commands.AddRange(old.Value.GenerateDeleteCommand());
                }
            }

            //The ones still left in target will be the ones that did not compare to any item in source, create add commands
            if (AllowedOperations.HasFlag(AllowedOperations.Create))
            {
                commands.AddRange(target.Values.SelectMany(t => t.GenerateAddCommand()));
            }
            
            return commands;
        }

        /// <summary>
        /// Maps an iterator into a Dictionary with unique keys for each item.
        /// </summary>
        /// <param name="iterator"></param>
        /// <returns></returns>
        protected static Dictionary<string, IDataItem> Map(IDataIterator iterator)
        {
            var dict = new Dictionary<string, IDataItem>();
            while (true)
            {
                var item = iterator.Next();
                if (item != null)
                {
                    var key = CreateUniqueKey(item);
                    try
                    {
                        dict.Add(key, item);
                    }
                    catch (ArgumentException)
                    {
                        //This should not happen unless the dataset is corrupted, which would mean it would not give a proper result
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Two items got the same unique key. This suggests invalid data. Check {GetItemPath(item)} vs {GetItemPath(dict[key])}");
                        Console.ResetColor();
                    }
                }
                else
                {
                    return dict;
                }
            }
        }

        /// <summary>
        /// Creates a unique key for an item (a file is the path, an item is the database and ID)
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected static string CreateUniqueKey(IDataItem item)
        {
            if (item is ContentDataItem)
            {
                //Unique key is database and item id
                var content = item as ContentDataItem;
                return $"content::{content.DatabaseName}::{content.ItemID}";
            }
            else if (item is FileSystemDataItem)
            {
                //The path should always be unique for files on windows
                var file = item as FileSystemDataItem;
                return $"file::{file.RelatedPath}";
            }
            //This should not happen, if it does the throw helps us spot it!
            throw new Exception($"Unhandled DataItem type {GetItemPath(item)}");
        }

        /// <summary>
        /// Only used for improved exception messages (even though it should give the same value as file.HashCode above)
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected static string GetItemPath(IDataItem item)
        {
            return string.Join("\\", new[] { item.RootPath, item.RelatedPath }.Where(s => !string.IsNullOrWhiteSpace(s)));
        }
    }
}
