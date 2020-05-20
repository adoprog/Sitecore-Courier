namespace Sitecore.Courier.DacPac
{
  public interface IFileSystemProvider
  {
    void AppendFile(string fileName, string content);

    void DeleteFile(string fileName);

    bool FileExists(string fileName);

    void WriteFile(string fileName, byte[] content);

    void WriteFile(string fileName, string content);
  }
}
