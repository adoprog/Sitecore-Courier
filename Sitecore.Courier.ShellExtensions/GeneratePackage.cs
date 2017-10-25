namespace Sitecore.Courier.ShellExtensions
{
  using System.IO;
  using SharpShell.Attributes;
  using SharpShell.SharpContextMenu;
  using System;
  using System.Diagnostics;
  using System.Runtime.InteropServices;
  using System.Windows.Forms;

  [ComVisible(true)]
  [COMServerAssociation(AssociationType.Directory)]
  public class GeneratePackage : SharpContextMenu
  {
    protected override bool CanShowMenu()
    {
      //  We always show the menu.
      return true;
    }

    /// <summary>
    /// Creates the context menu. This can be a single menu item or a tree of them.
    /// </summary>
    /// <returns>
    /// The context menu for the shell context menu.
    /// </returns>
    protected override ContextMenuStrip CreateMenu()
    {
      //  Create the menu strip.
      var menu = new ContextMenuStrip();

      //  Create a 'count lines' item.
      var itemCountLines = new ToolStripMenuItem
      {
        Text = "Package with Sitecore Courier", Image = Resources.Icon
      };

      //  When we click, we'll count the lines.
      itemCountLines.Click += (sender, args) => this.RunCourier();

      //  Add the item to the context menu.
      menu.Items.Add(itemCountLines);

      //  Return the menu.
      return menu;
    }

    /// <summary>
    /// Counts the lines in the selected files.
    /// </summary>
    private void RunCourier()
    {
      foreach (var filePath in SelectedItemPaths)
      {
        try
        {
          var targetFolder = new DirectoryInfo(filePath);
          var process = new Process();

          process.StartInfo.UseShellExecute = false;
          process.StartInfo.FileName = @"Sitecore.Courier.Runner.exe";
          process.StartInfo.Arguments = string.Format(@"-t ""{0}"" -o ""{1}\Sitecore.Courier_{2}.update""", filePath, targetFolder.Parent.FullName, targetFolder.Name);
          process.Start();
        }
        catch (Exception e)
        {
          MessageBox.Show(e.Message);
        }
      }
    }
  }
}