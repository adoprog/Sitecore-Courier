using System.Collections.Generic;
using System.IO;
using Sitecore.Courier.Sql;
using Sitecore.Courier.Sql.Model;

namespace Sitecore.Courier.DacPac
{
   public class SqlWriter : ISqlWriter
  {
    private readonly List<string> databaseFiles = new List<string>();

    private readonly IFileSystemProvider fileSystemProvider;

    private readonly string outputPath;

    private readonly ISqlGenerator sqlGenerator;

    public SqlWriter(IFileSystemProvider fileSystemProvider, ISqlGenerator sqlGenerator, string outputPath)
    {
      this.fileSystemProvider = fileSystemProvider;
      this.sqlGenerator = sqlGenerator;
      this.outputPath = outputPath;
    }

    public void Dispose()
    {
      string str = this.sqlGenerator.GenerateAppendStatements();
      foreach (string databaseFile in this.databaseFiles)
      {
        this.fileSystemProvider.AppendFile(databaseFile, str);
      }
    }

    public void WriteBlob(Blob blob)
    {
      string str = this.sqlGenerator.GenerateAddBlobStatements(blob);
      this.fileSystemProvider.AppendFile(Path.Combine(this.outputPath, string.Concat(blob.Database, ".sql")), str);
    }

    public void WriteFile(PackageFile file)
    {
      this.fileSystemProvider.WriteFile(Path.Combine(this.outputPath, "website", file.FileName), file.Content);
    }

    public void WriteItem(Item item)
    {
      string str = Path.Combine(this.outputPath, string.Concat(item.Database, ".sql"));
      if (!this.databaseFiles.Contains(str))
      {
        this.databaseFiles.Add(str);
        if (this.fileSystemProvider.FileExists(str))
        {
          this.fileSystemProvider.DeleteFile(str);
        }
        string str1 = this.sqlGenerator.GeneratePrependStatements();
        this.fileSystemProvider.AppendFile(str, str1);
      }
      string str2 = this.sqlGenerator.GenerateAddItemStatements(item);
      this.fileSystemProvider.AppendFile(str, str2);
    }

    public void WritePostStep(string packageName, string postStep)
    {
      this.fileSystemProvider.WriteFile(Path.Combine(this.outputPath, string.Concat("website\\App_Data\\poststeps\\", (packageName ?? "poststep").Replace(' ', '\u005F'), ".poststep")), postStep);
    }

    public void WriteRole(Role role)
    {
      string str = this.sqlGenerator.GenerateAddRoleStatements(role);
      this.fileSystemProvider.AppendFile(Path.Combine(this.outputPath, "core.sql"), str);
    }
  }
}
