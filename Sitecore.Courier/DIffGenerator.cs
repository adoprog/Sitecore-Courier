using System.Diagnostics;
using System.Linq;
using Sitecore.Update.Commands;

namespace Sitecore.Courier
{
  using System.Collections.Generic;

  using Sitecore.Update;
  using Sitecore.Update.Configuration;
  using Sitecore.Update.Data;
  using Sitecore.Update.Interfaces;

  /// <summary>
  /// Defines the Diff generator class.
  /// </summary>
  public class DiffGenerator
  {
    public static List<ICommand> GetDiffCommands(string sourcePath, string targetPath, CollisionBehavior collisionBehavior = CollisionBehavior.Undefined)
    {
      var targetManager = Factory.Instance.GetSourceDataManager();
      var sourceManager = Factory.Instance.GetTargetDataManager();

      sourceManager.SerializationPath = sourcePath;
      targetManager.SerializationPath = targetPath;

      IDataIterator sourceDataIterator = sourceManager.ItemIterator;
      IDataIterator targetDataIterator = targetManager.ItemIterator;

      var engine = new DataEngine();

      var commands = new List<ICommand>();
      commands.AddRange(GenerateDiff(sourceDataIterator, targetDataIterator));
        //if an item is found to be deleted AND added, we can be sure it's a move
      var deleteCommands = commands.OfType<DeleteItemCommand>();
      var shouldBeUpdateCommands =
          commands.OfType<AddItemCommand>()
              .Select(a => new
              {
                  Added = a,
                  Deleted = deleteCommands.FirstOrDefault(d => d.ItemID == a.ItemID)
              }).Where(u => u.Deleted != null).ToList();
      foreach (var command in shouldBeUpdateCommands)
      {
          commands.AddRange(command.Deleted.GenerateUpdateCommand(command.Added));
          commands.Remove(command.Added);
          commands.Remove(command.Deleted);
          //now, this one is an assumption, but would go wrong without the assumption anyway: this assumption is in fact safer
          //if the itempath of a delete command starts with this delete command, it will be moved along to the new node, not deleted, just leave it alone
          commands.RemoveAll(c => c is DeleteItemCommand && ((DeleteItemCommand)c).ItemPath.StartsWith(command.Deleted.ItemPath));
      }

      commands.ForEach(_ => _.CollisionBehavior = collisionBehavior);

      engine.ProcessCommands(ref commands);
      return commands;
    }

    /// <summary>
    /// Generates the diff.
    /// </summary>
    /// <param name="sourceIterator">The source iterator.</param>
    /// <param name="targetIterator">The target iterator.</param>
    /// <returns>
    /// The diff.
    /// </returns>
    protected static IList<ICommand> GenerateDiff(IDataIterator sourceIterator, IDataIterator targetIterator)
    {
      List<ICommand> commands = new List<ICommand>();
      IDataItem sourceDataItem = sourceIterator.Next();
      IDataItem targetDataItem = targetIterator.Next();

      while (sourceDataItem != null || targetDataItem != null)
      {
        int compareResult = Compare(sourceDataItem, targetDataItem);
        commands.AddRange((sourceDataItem ?? targetDataItem).GenerateDiff(sourceDataItem, targetDataItem, compareResult));
        if (compareResult < 0)
        {
          sourceDataItem = sourceIterator.Next();
        }
        else
        {
          if (compareResult > 0)
          {
            targetDataItem = targetIterator.Next();
          }
          else
          {
            if (compareResult == 0)
            {
              targetDataItem = targetIterator.Next();
              sourceDataItem = sourceIterator.Next();
            }
          }
        }
      }
    
      return commands;
    }

    /// <summary>
    /// Compares the specified source item.
    /// </summary>
    /// <param name="sourceItem">The source item.</param>
    /// <param name="targetItem">The target item.</param>
    /// <returns>
    /// The int32.
    /// </returns>
    protected static int Compare(IDataItem sourceItem, IDataItem targetItem)
    {
        if (sourceItem == null && targetItem == null)
        {
            return 0;
        }

        if (sourceItem == null)
        {
            return 1;
        }

        if (targetItem == null)
        {
            return -1;
        }

        return sourceItem.CompareTo(targetItem);
    }
  }
}
