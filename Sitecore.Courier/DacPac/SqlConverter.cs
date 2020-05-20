using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Courier.Sql;
using Sitecore.Courier.Sql.Model;
using Sitecore.Update;
using Sitecore.Update.Commands;
using Sitecore.Update.Items;

namespace Sitecore.Courier.DacPac
{
  public class SqlConverter
  {
    private void ConvertItems(Converter converter)
    {
      foreach (var command in converter.Diff.Commands)
      {
        var addItemCommand = command as AddItemCommand;
        if (addItemCommand == null)
        {
          return;
        }

        var items = ParseAddItemCommand(addItemCommand);
        foreach (var item in items)
        {
          converter.Writer.WriteItem(item);
        }
      }
    }

    public void ConvertPackage(DiffInfo diff, string outputPath)
    {
      using (Converter converter = new Converter(diff,
        new SqlWriter(new FileSystemProvider(), new SqlGenerator(), outputPath)))
      {
        this.ConvertItems(converter);
      }
    }

    private IEnumerable<Item> ParseAddItemCommand(AddItemCommand commandXml)
    {
      List<Item> objList = new List<Item>();
      string str1 = commandXml.AddedItemData.DatabaseName;
      string str2 = commandXml.AddedItemData.Name;
      string str3 = commandXml.AddedItemData.ID;
      string str4 = commandXml.AddedItemData.BranchId;
      string str5 = commandXml.AddedItemData.ParentId;
      string str6 = commandXml.AddedItemData.TemplateId;
      IEnumerable<Field> first = this.ParseFields(commandXml.AddedItemData.SharedFields)
        .Select<KeyValuePair<string, string>, Field>((Func<KeyValuePair<string, string>, Field>) (f => new Field()
        {
          FieldType = FieldType.Shared,
          Id = f.Key,
          Value = f.Value
        }));
      foreach (var xpathSelectElement in commandXml.AddedItemData.Versions)
      {
        string str7 = xpathSelectElement.Version;
        string str8 = xpathSelectElement.Language;
        IEnumerable<Field> second = this.ParseFields(xpathSelectElement.Fields)
          .Select<KeyValuePair<string, string>, Field>((Func<KeyValuePair<string, string>, Field>) (f => new Field()
          {
            FieldType = FieldType.Unknown,
            Id = f.Key,
            Value = f.Value
          }));
        objList.Add(new Item()
        {
          Database = str1,
          Name = str2,
          Id = str3,
          MasterId = str4,
          ParentId = str5,
          TemplateId = str6,
          Language = str8,
          Version = str7,
          Fields = first.Union<Field>(second)
        });
      }

      return (IEnumerable<Item>) objList;
    }

    private IEnumerable<KeyValuePair<string, string>> ParseFields(
      IEnumerable<UpdateSyncField> fieldElements)
    {
      return fieldElements.Select(e => new KeyValuePair<string, string>(e.FieldId, e.FieldValue));
    }
  }
}