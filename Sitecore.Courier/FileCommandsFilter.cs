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
    private readonly List<string> excludedFolders = new List<string>();

    public FileCommandsFilter()
    {
      excludedFolders.Add("serialization");
      excludedFolders.Add("data");
    }

    /// <summary>
    /// Filters the command. 
    /// </summary>
    /// <param name="command">The command.</param>
    /// <returns>The filtered command.</returns>
    public ICommand FilterCommand(ICommand command)
    {
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
      return new FileCommandsFilter();
    }
  }
}
