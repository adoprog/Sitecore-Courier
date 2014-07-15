namespace Sitecore.Courier
{
  using Sitecore.Diagnostics;
  using Sitecore.Update.Data.Items;
  using System.IO;

  public class ItemUtils
  {
    public static FileSystemDataItem GetFileSystemDataItem(string root, FileInfo file)
    {
      Assert.IsNotNull((object)root, "root");
      Assert.IsNotNull((object)file, "file");

      return new FileSystemDataItem(root, root.TrimEnd('\\', '/').Length == file.Directory.FullName.TrimEnd('\\', '/').Length ? string.Empty : file.Directory.FullName.Substring(root.Length), file.Name);
    }
  }
}
