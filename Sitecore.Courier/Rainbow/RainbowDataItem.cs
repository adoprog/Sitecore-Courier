// Decompiled with JetBrains decompiler
// Type: Sitecore.Update.Data.Items.ContentDataItem
// Assembly: Sitecore.Update, Version=6.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1C87198F-53E7-4FA3-AF4F-BFE9D0713881
// Assembly location: C:\github\Sitecore-Courier\Sitecore.Courier.Runner\bin\Debug\Sitecore.Update.dll

using System;
using System.IO;
using System.Linq;
using Rainbow.Model;
using Rainbow.Storage.Yaml;
using Sitecore.Data;
using Sitecore.Data.Serialization.ObjectModel;

namespace Sitecore.Courier.Rainbow
{
    /// <summary>
    /// 
    /// </summary>
    public class RainbowDataItem : QuickContentDataItem
    {
        private const string RevisionFieldId = "{8cdc337e-a112-42fb-bbb4-4143751e123f}";

        /// <summary>
        /// The _formatter
        /// </summary>
        private readonly YamlSerializationFormatter _formatter;
        /// <summary>
        /// The item data
        /// </summary>
        private IItemData _itemData;
        /// <summary>
        /// The item
        /// </summary>
        private SyncItem _item;

        private IItemMetadata _itemMetaData;

        /// <summary>
        /// Initializes a new instance of the <see cref="RainbowDataItem" /> class.
        /// </summary>
        /// <param name="rootPath">The root path.</param>
        /// <param name="relatedPath">The related path.</param>
        /// <param name="name">The name.</param>
        /// <param name="formatter">The formatter.</param>
        public RainbowDataItem(string rootPath, string relatedPath, string name, YamlSerializationFormatter formatter)
          : base(rootPath, relatedPath, name)
        {
            _formatter = formatter;
        }


        /// <summary>
        /// Gets the fast identifier.
        /// </summary>
        /// <returns></returns>
        protected override Guid GetFastId()
        {
            return GetItemMetaData().Id;
        }

        /// <summary>
        /// Gets the item data.
        /// </summary>
        /// <returns></returns>
        protected virtual IItemData GetItemData()
        {
            if (_itemData != null)
            {
                return _itemData;
            }

            using (var stream = File.OpenRead(ItemPath))
            {
                try
                {
                    _itemData = _formatter.ReadSerializedItem(stream, ItemPath);
                }
                catch (Exception)
                {
                    Console.WriteLine("Unable to deserialize item: " + ItemPath);
                    return null;
                }
            }

            _itemMetaData = _itemData;
            return _itemData;
        }

        private IItemMetadata GetItemMetaData()
        {
            if (_itemMetaData != null)
                return _itemMetaData;
            using (var stream = File.OpenRead(ItemPath))
            {
                return _itemMetaData = _formatter.ReadSerializedItemMetadata(stream, ItemPath);
            }
        }

        /// <summary>
        /// Gets the syncitem.
        /// </summary>
        /// <returns></returns>
        protected override SyncItem GetItem()
        {
            if (this._item != null)
                return this._item;

            var itemData = GetItemData();
            if (itemData == null)
                return null;
            return _item = BuildSyncItem(itemData);
        }

        public SyncItem BuildSyncItem(IItemData item)
        {
            var syncItem = new SyncItem
            {
                ID = item.Id.ToString("B"),
                DatabaseName = item.DatabaseName,
                ParentID = item.ParentId.ToString("B"),
                Name = item.Name,
                BranchId = item.BranchId.ToString("B"),
                TemplateID = item.TemplateId.ToString("B"),
                ItemPath = item.Path
            };
            //syncItem.TemplateName = item.TemplateName;
            foreach (var field in item.SharedFields) //TODO: ItemSynchronization.BuildSyncItem sorts the fields and versions first, should we ?
            {
                syncItem.AddSharedField(field.FieldId.ToString("B"), null/*name*/, null/*key?*/, field.Value, true);
            }
            foreach (var version in item.Versions)
            {
                var syncVersion = syncItem.AddVersion(version.Language.ToString(), version.VersionNumber.ToString(), version.VersionNumber.ToString() /*revisionid needed?*/);
                if (syncVersion != null)
                {
                    foreach (var field in version.Fields)
                    {
                        syncVersion.AddField(field.FieldId.ToString("B"), null/*name*/,null /*key?*/, field.Value, true);
                    }
                    foreach (var field in item.UnversionedFields.Where(x => x.Language.ToString() == version.Language.ToString()).SelectMany(x => x.Fields))
                    {
                        syncVersion.AddField(field.FieldId.ToString("B"), null/*name*/, null/*key?*/, field.Value, true);
                    }
                }

                if (RainbowSerializationProvider.EnsureRevision && !syncVersion.Fields.Any(x => x.FieldID == RevisionFieldId))
                {
                    syncVersion.AddField(RevisionFieldId, null/*name*/, null /*key?*/, Guid.NewGuid().ToString().ToLowerInvariant(), true);
                }
            }

            return syncItem;
        }
    }
}
