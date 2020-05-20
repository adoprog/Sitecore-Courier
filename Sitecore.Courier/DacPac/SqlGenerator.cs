using Sitecore.Courier.Sql.Model;
using System.Linq;
using System.Text;
using Sitecore.Courier.DacPac;

namespace Sitecore.Courier.Sql
{
  public class SqlGenerator : SqlGeneratorBase
  {
    protected override void AddCreateTableStatements(StringBuilder result)
    {
      base.AddCreateTableStatements(result);
      result.AppendLine("CREATE TABLE #UnknownFields (ItemId uniqueidentifier NOT NULL, FieldId uniqueidentifier NOT NULL, Language nvarchar(50) COLLATE database_default NOT NULL, Version int NOT NULL, Value nvarchar(max) COLLATE database_default NOT NULL, Created datetime NOT NULL, Updated datetime NOT NULL, IsUnversioned bit NULL, PRIMARY KEY CLUSTERED ([ItemID], [FieldID], [Language], [Version]));");
    }

    protected override void AddDropTemporaryTablesStatements(StringBuilder result)
    {
      base.AddDropTemporaryTablesStatements(result);
      result.AppendLine("DROP TABLE #UnknownFields");
    }

    protected override void AddMergeStatements(StringBuilder result)
    {
      base.AddMergeStatements(result);
      result.AppendLine("\r\nMERGE[UnversionedFields] AS target\r\n    USING(SELECT DISTINCT ItemId, FieldId, Language, Value FROM #UnknownFields WHERE IsUnversioned = 1) AS source \r\n    ON target.ItemId = source.ItemId AND target.FieldId = source.FieldId AND target.Language = source.Language\r\n    WHEN MATCHED THEN\r\n        UPDATE SET\r\n            Value = source.Value,\r\n            Updated = GETUTCDATE()\r\n    WHEN NOT MATCHED THEN\r\n        INSERT(Id, ItemId, FieldId, Language, Value, Created, Updated)\r\n        VALUES(NEWID(), source.ItemId, source.FieldId, source.Language, source.Value, GETUTCDATE(), GETUTCDATE()); ");
      result.AppendLine("\r\nMERGE[VersionedFields] AS target\r\n    USING (SELECT * FROM #UnknownFields WHERE IsUnversioned IS NULL) AS source\r\n    ON target.ItemId = source.ItemId AND target.FieldId = source.FieldId AND target.Language = source.Language AND target.Version = source.Version\r\n    WHEN MATCHED AND source.IsUnversioned is null THEN\r\n        UPDATE SET\r\n            Value = source.Value,\r\n            Updated = source.Updated\r\n    WHEN NOT MATCHED THEN\r\n        INSERT(Id, ItemId, FieldId, Language, Version, Value, Created, Updated)\r\n        VALUES(NEWID(), source.ItemId, source.FieldId, source.Language, source.Version, source.Value, source.Created, source.Updated);");
    }

    protected override void AddPreprocessStatements(StringBuilder result)
    {
      base.AddPreprocessStatements(result);
      result.AppendLine("\r\nUPDATE #UnknownFields SET IsUnversioned = 1\r\n    FROM #UnknownFields ItemFields INNER JOIN [SharedFields] FieldProps ON ItemFields.FieldId = FieldProps.ItemId\r\n    WHERE FieldProps.FieldId = '39847666-389D-409B-95BD-F2016F11EED5' And FieldProps.Value = '1'");
      result.AppendLine("\r\nDECLARE @itemId uniqueidentifier,\r\n    @fieldId uniqueidentifier,\r\n    @language nvarchar(50),\r\n    @version int,\r\n    @blobValue nvarchar(MAX),\r\n    @blobId uniqueidentifier,\r\n    @fieldType varchar(50)\r\n\r\nDECLARE blobValuesCursor CURSOR\r\n  FOR SELECT FieldValues.ItemId, FieldValues.FieldId, FieldValues.Language, FieldValues.Version, FieldValues.Value, FieldValues.FieldType\r\n      FROM(\r\n        SELECT ItemId, FieldId, Language, Version, Value, CASE WHEN IsUnversioned IS NULL THEN 'versioned' ELSE 'unversioned' END AS FieldType\r\n        FROM #UnknownFields  UNION\r\n        SELECT ItemId, FieldId, NULL, NULL, Value, 'shared' AS FieldType\r\n        FROM #SharedFields) FieldValues INNER JOIN \r\n            SharedFields FieldProps ON FieldValues.FieldId = FieldProps.ItemId\r\n      WHERE FieldProps.FieldId = 'FF8A2D01-8A77-4F1B-A966-65806993CD31' And FieldProps.Value = '1'\r\nOPEN blobValuesCursor\r\nFETCH NEXT FROM blobValuesCursor\r\nINTO @itemId, @fieldId, @language, @version, @blobValue, @fieldType\r\n\r\nWHILE @@FETCH_STATUS = 0\r\nBEGIN\r\n    IF @fieldType = 'shared'\r\n    BEGIN\r\n        SELECT @blobId = Value\r\n        FROM SharedFields\r\n        WHERE ItemId = @itemId AND FieldId = @fieldId\r\n    END\r\n    IF @fieldType = 'unversioned'\r\n    BEGIN\r\n        SELECT @blobId = Value\r\n        FROM UnversionedFields\r\n        WHERE ItemId = @itemId AND FieldId = @fieldId AND Language = @language\r\n    END\r\n    IF @fieldType = 'versioned'\r\n    BEGIN\r\n        SELECT @blobId = Value\r\n        FROM VersionedFields\r\n        WHERE ItemId = @itemId AND FieldId = @fieldId AND Language = @language AND Version = @version\r\n    END\r\n    IF @blobId IS NULL\r\n    BEGIN\r\n        SET @blobId = NEWID()\r\n    END\r\n\r\n    IF NOT EXISTS(SELECT * FROM #Blobs WHERE BlobId = @blobId)\r\n    BEGIN\r\n        INSERT INTO #Blobs(BlobId, [Index], Data, Created)\r\n        VALUES(@blobId, 0, (CAST(CAST(N'' as xml).value('xs:base64Binary(sql:variable(\"@blobValue\"))', 'varbinary(max)')  AS IMAGE)), GETDATE())\r\n    END\r\n\r\n    IF(@fieldType = 'shared')\r\n    BEGIN\r\n        UPDATE #SharedFields\r\n        SET Value = @blobId\r\n        WHERE ItemId = @itemId AND FieldId = @fieldId\r\n    END\r\n\r\n    IF(@fieldType = 'unversioned')\r\n    BEGIN\r\n        UPDATE #UnknownFields\r\n        SET Value = @blobId\r\n        WHERE ItemId = @itemId AND FieldId = @fieldId AND Language = @language AND Version IS NULL\r\n    END\r\n\r\n    IF(@fieldType = 'versioned')\r\n    BEGIN\r\n        UPDATE #UnknownFields\r\n        SET Value = @blobId\r\n        WHERE ItemId = @itemId AND FieldId = @fieldId AND Language = @language AND Version = @version\r\n    END\r\n\r\n    FETCH NEXT FROM blobValuesCursor\r\n    INTO @itemId, @fieldId, @language, @version, @blobValue, @fieldType\r\n    SET @blobId = null\r\nEND\r\n\r\nCLOSE blobValuesCursor\r\nDEALLOCATE blobValuesCursor");
    }

    public override string GenerateAddItemStatements(Item item)
    {
      StringBuilder stringBuilder = new StringBuilder(base.GenerateAddItemStatements(item));
      string str = this.ScriptUnknownFieldsTableOperations(item);
      if (!string.IsNullOrEmpty(str))
      {
        stringBuilder.AppendLine(str);
      }
      stringBuilder.AppendLine("GO");
      return stringBuilder.ToString();
    }

    private string ScriptUnknownFieldsTableOperations(Item item)
    {
      StringBuilder stringBuilder = new StringBuilder();
      foreach (Field field in
          from i in item.Fields
          where i.FieldType == FieldType.Unknown
          select i)
      {
        string str = string.Concat(new string[] { "u", item.Database, item.Id, field.Id, item.Language, item.Version });
        if (this.processedItems.ContainsKey(str))
        {
          continue;
        }
        this.processedItems[str] = null;
        stringBuilder.AppendLine("INSERT INTO #UnknownFields (ItemId, FieldId, Language, Version, Value, Created, Updated)");
        stringBuilder.AppendLine(string.Concat(new string[] { "   VALUES ('", item.Id, "', '", field.Id, "', '", item.Language, "', '", item.Version, "', ", FormatValue(field.Value), ", GETUTCDATE(), GETUTCDATE());" }));
      }
      return stringBuilder.ToString();
    }
  }
}
