namespace Sitecore.Courier
{
  using Sitecore.Update.Commands;
  using Sitecore.Update.Interfaces;

  /// <summary>
  /// The language filter.
  /// </summary>
  public class FileCommandsFilter : ICommandFilter
  {
    /// <summary>
    /// Filters the command. 
    /// </summary>
    /// <param name="command">The command.</param>
    /// <returns>The filtered command.</returns>
    public ICommand FilterCommand(ICommand command)
    {
      if ((command is ChangeFileCommand) || (command is AddFileCommand) || (command is AddFolderCommand))
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
