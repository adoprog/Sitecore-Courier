using Sitecore.Courier.Sql.Model;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Sitecore.Courier.DacPac
{
  public abstract class SqlGeneratorBase : ISqlGenerator
  {
    protected Dictionary<string, object> processedItems = new Dictionary<string, object>();

    protected virtual void AddCreateTableStatements(StringBuilder result)
    {
      result.AppendLine("CREATE TABLE #Blobs (BlobId uniqueidentifier NOT NULL, [Index] int NOT NULL, Data image NOT NULL, Created datetime NOT NULL);");
      result.AppendLine("CREATE TABLE #Items (ID uniqueidentifier NOT NULL PRIMARY KEY, Name nvarchar(256) COLLATE database_default NOT NULL, TemplateID uniqueidentifier NOT NULL, MasterID uniqueidentifier NOT NULL, ParentID uniqueidentifier NOT NULL, Created datetime NOT NULL, Updated datetime NOT NULL);");
      result.AppendLine("CREATE TABLE #SharedFields (ItemId uniqueidentifier NOT NULL, FieldId uniqueidentifier NOT NULL, Value nvarchar(max) COLLATE database_default NOT NULL, Created datetime NOT NULL, Updated datetime NOT NULL, PRIMARY KEY CLUSTERED ([ItemID], [FieldID]));");
    }

    protected virtual void AddDropTemporaryTablesStatements(StringBuilder result)
    {
      result.AppendLine("DROP TABLE #Blobs");
      result.AppendLine("DROP TABLE #Items");
      result.AppendLine("DROP TABLE #SharedFields");
    }

    protected virtual void AddMergeStatements(StringBuilder result)
    {
      result.AppendLine("\r\nMERGE [Blobs] AS target\r\nUSING #Blobs AS source\r\nON target.BlobId = source.BlobId AND target.[Index] = source.[Index]\r\nWHEN MATCHED THEN\r\n    UPDATE SET\r\n        Data = source.Data\r\nWHEN NOT MATCHED THEN\r\n    INSERT (Id, BlobId, [Index], Data, Created)\r\n    VALUES (NEWID(), source.BlobId, source.[Index], source.Data, source.Created);\r\n");
      result.AppendLine("\r\nMERGE [Items] AS target\r\nUSING #Items AS source\r\nON target.ID = source.ID\r\nWHEN MATCHED THEN\r\n    UPDATE SET\r\n        Name = source.Name,\r\n        TemplateID = source.TemplateID,\r\n        MasterID = source.MasterID,\r\n        ParentID = source.ParentID,\r\n        Updated = source.Updated\r\nWHEN NOT MATCHED THEN\r\n    INSERT (ID, Name, TemplateID, MasterID, ParentID, Created, Updated)\r\n    VALUES (source.ID, source.Name, source.TemplateID, source.MasterID, source.ParentID, source.Created, source.Updated);\r\n");
      result.AppendLine("\r\nMERGE [SharedFields] AS target\r\nUSING #SharedFields AS source\r\nON target.ItemId = source.ItemId AND target.FieldId = source.FieldId\r\nWHEN MATCHED THEN\r\n    UPDATE SET\r\n        Value = source.Value,\r\n        Updated = source.Updated\r\nWHEN NOT MATCHED THEN\r\n    INSERT (Id, ItemId, FieldId, Value, Created, Updated)\r\n    VALUES (NEWID(), source.ItemId, source.FieldId, source.Value, source.Created, source.Updated);\r\n");
    }

    protected virtual void AddPreprocessStatements(StringBuilder result)
    {
    }

    public static string FormatBlobValue(byte[] value)
    {
      StringBuilder stringBuilder = new StringBuilder((int)value.Length * 2);
      byte[] numArray = value;
      for (int i = 0; i < (int)numArray.Length; i++)
      {
        byte num = numArray[i];
        stringBuilder.Append(num.ToString("X2", CultureInfo.InvariantCulture));
      }
      return stringBuilder.ToString();
    }

    protected static string FormatValue(string value)
    {
      string str = value.Replace("'", "''").Replace("$(", "$' as nvarchar(max)),cast(N'(");
      if (str.Contains("$' as nvarchar(max)),cast(N'("))
      {
        return string.Concat("concat(cast(N'", str, "' as nvarchar(max)))");
      }
      return string.Concat("N'", str, "'");
    }

    public string GenerateAddBlobStatements(Blob blob)
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendLine(string.Format("\r\nINSERT INTO #Blobs (BlobId, [Index], Data, Created)\r\nVALUES ('{0}', '{1}', 0x{2}, GETUTCDATE())", blob.Id, 0, FormatBlobValue(blob.Data)));
      stringBuilder.AppendLine("GO");
      return stringBuilder.ToString();
    }

    public virtual string GenerateAddItemStatements(Item item)
    {
      StringBuilder stringBuilder = new StringBuilder();
      string str = this.ScriptItemTableOperations(item);
      if (!string.IsNullOrEmpty(str))
      {
        stringBuilder.AppendLine(str);
      }
      string str1 = this.ScriptSharedFieldsTableOperations(item);
      if (!string.IsNullOrEmpty(str1))
      {
        stringBuilder.AppendLine(str1);
      }
      return stringBuilder.ToString();
    }

    public string GenerateAddRoleStatements(Role role)
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendLine("DECLARE @applicationId nvarchar(256)");
      stringBuilder.AppendLine("SELECT TOP 1 @applicationId = [ApplicationId] FROM [aspnet_Applications] WHERE [ApplicationName] = 'sitecore'");
      stringBuilder.AppendLine(string.Concat("IF NOT EXISTS (SELECT TOP 1 [RoleId] FROM [aspnet_Roles] WHERE [ApplicationId] = @applicationId AND [RoleName] = '", role.Name, "')"));
      stringBuilder.AppendLine("BEGIN");
      stringBuilder.AppendLine("    INSERT INTO [aspnet_Roles] (ApplicationId, RoleId, RoleName, LoweredRoleName, Description)");
      stringBuilder.AppendLine(string.Concat(new string[] { "    VALUES (@applicationId, NEWID(), '", role.Name, "', LOWER('", role.Name, "'), NULL)" }));
      stringBuilder.AppendLine("END");
      foreach (string membership in role.Membership)
      {
        stringBuilder.AppendLine(string.Concat(new string[] { "IF NOT EXISTS (SELECT TOP 1 * FROM [RolesInRoles] WHERE [MemberRoleName] = '", role.Name, "' AND [TargetRoleName] = '", membership, "')" }));
        stringBuilder.AppendLine("BEGIN");
        stringBuilder.AppendLine("    INSERT INTO [RolesInRoles] (Id, MemberRoleName, TargetRoleName, ApplicationName, Created)");
        stringBuilder.AppendLine(string.Concat(new string[] { "    VALUES (NEWID(), '", role.Name, "', '", membership, "', '', SYSUTCDATETIME())" }));
        stringBuilder.AppendLine("END");
      }
      stringBuilder.AppendLine("GO");
      return stringBuilder.ToString();
    }

    public virtual string GenerateAppendStatements()
    {
      StringBuilder stringBuilder = new StringBuilder();
      this.AddPreprocessStatements(stringBuilder);
      this.AddMergeStatements(stringBuilder);
      this.AddDropTemporaryTablesStatements(stringBuilder);
      stringBuilder.AppendLine("COMMIT TRANSACTION;");
      return stringBuilder.ToString();
    }

    public virtual string GeneratePrependStatements()
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendLine("BEGIN TRANSACTION;");
      this.AddCreateTableStatements(stringBuilder);
      return stringBuilder.ToString();
    }

    protected string ScriptItemTableOperations(Item item)
    {
      string str = string.Concat("i", item.Database, item.Id);
      if (this.processedItems.ContainsKey(str))
      {
        return string.Empty;
      }
      this.processedItems[str] = null;
      return string.Concat(new string[] { "\r\nINSERT INTO #Items (ID, Name, TemplateID, MasterID, ParentID, Created, Updated)\r\n    VALUES ('", item.Id, "', ", FormatValue(item.Name), ", '", item.TemplateId, "', '", item.MasterId, "', '", item.ParentId, "', GETUTCDATE(), GETUTCDATE());" });
    }

    protected string ScriptSharedFieldsTableOperations(Item item)
    {
      StringBuilder stringBuilder = new StringBuilder();
      foreach (Field field in
          from f in item.Fields
          where f.FieldType == FieldType.Shared
          select f)
      {
        string str = string.Concat("s", item.Database, item.Id, field.Id);
        if (this.processedItems.ContainsKey(str))
        {
          continue;
        }
        this.processedItems[str] = null;
        stringBuilder.AppendLine("INSERT INTO #SharedFields (ItemId, FieldId, Value, Created, Updated)");
        stringBuilder.AppendLine(string.Concat(new string[] { "    VALUES ('", item.Id, "', '", field.Id, "', ", FormatValue(field.Value), ", GETUTCDATE(), GETUTCDATE());" }));
      }
      return stringBuilder.ToString();
    }
  }
}
