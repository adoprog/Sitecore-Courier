using Sitecore.Update;

namespace Sitecore.Courier
{
  using System.Collections.Generic;
  using System.Linq;

  using Sitecore.Update.Commands;
  using Sitecore.Update.Interfaces;

  /// <summary>
  /// The language filter.
  /// </summary>
  public class FileCommandsFilter : ICommandFilter
  {
    private List<string> excludedFolders = new List<string>();
    private bool forceoverwrites = false;

    public FileCommandsFilter()
    {
      excludedFolders.Add("serialization");
      excludedFolders.Add("data");
    }

    public List<string> ExcludedFolders { get { return excludedFolders; } set { excludedFolders = value; } }

    /// <summary>
    /// Filters the command. 
    /// </summary>
    /// <param name="command">The command.</param>
    /// <returns>The filtered command.</returns>
    public ICommand FilterCommand(ICommand command)
    {
      if (command == null)
        return null;
        
      if (!(command is AddFolderCommand) && !(command is AddFileCommand))
      {
        return command;
      }

      if (this.excludedFolders.Any(folder => command.EntityPath.Replace("\\", string.Empty).StartsWith(folder)))
      {
        return null;
      }

      return command;
    }

    /// <summary>
    /// Clones this instance.
    /// </summary>
    /// <returns>
    /// The I command filter.
    /// </returns>
    public ICommandFilter Clone()
    {
        return new FileCommandsFilter()
        {
            ExcludedFolders = excludedFolders
        };
    }

  }
}
