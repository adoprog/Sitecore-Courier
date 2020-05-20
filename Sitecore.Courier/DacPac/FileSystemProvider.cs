using System.IO;

namespace Sitecore.Courier.DacPac
{
  public class FileSystemProvider : IFileSystemProvider
  {
    public void AppendFile(string fileName, string content)
    {
      this.EnsureFolderExists(Path.GetDirectoryName(fileName));
      File.AppendAllText(fileName, content);
    }

    public void DeleteFile(string fileName)
    {
      File.Delete(fileName);
    }

    public void EnsureFolderExists(string path)
    {
      if (!Directory.Exists(path))
      {
        Directory.CreateDirectory(path);
      }
    }

    public bool FileExists(string fileName)
    {
      return File.Exists(fileName);
    }

    public void WriteFile(string fileName, byte[] content)
    {
      this.WriteFile(fileName, new MemoryStream(content));
    }

    public void WriteFile(string fileName, string content)
    {
      MemoryStream memoryStream = new MemoryStream();
      StreamWriter streamWriter = new StreamWriter(memoryStream);
      streamWriter.WriteAsync(content).GetAwaiter().GetResult();
      streamWriter.FlushAsync().GetAwaiter().GetResult();
      memoryStream.Seek((long)0, SeekOrigin.Begin);
      this.WriteFile(fileName, memoryStream);
    }

    private void WriteFile(string fileName, Stream content)
    {
      this.EnsureFolderExists(Path.GetDirectoryName(fileName));
      if (content.Length != 0 && !fileName.EndsWith("\\") && !fileName.EndsWith("/"))
      {
        using (FileStream fileStream = File.Create(fileName))
        {
          content.CopyToAsync(fileStream).GetAwaiter().GetResult();
        }
      }
      content.Dispose();
    }
  }
}