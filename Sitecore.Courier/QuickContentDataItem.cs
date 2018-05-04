using System;
using System.Collections.Generic;
using System.IO;
using Sitecore.Data.Serialization.ObjectModel;
using Sitecore.Diagnostics;
using Sitecore.Update.Commands;
using Sitecore.Update.Data.Items;
using Sitecore.Update.Interfaces;

namespace Sitecore.Courier
{
    public class QuickContentDataItem : ContentDataItem
    {
        private SyncItem _item;
        private Guid _id;

        public QuickContentDataItem(string rootPath, string relatedPath, string name)
            : base(rootPath, relatedPath, name)
        {
        }

        public QuickContentDataItem(string rootPath, string relatedPath, string name, Stream stream)
            : base(rootPath, relatedPath, name, stream)
        {
        }

        public QuickContentDataItem(string rootPath, string relatedPath, string name, SyncItem syncItem)
            : base(rootPath, relatedPath, name, syncItem)
        {
        }

        public QuickContentDataItem(string rootPath, string relatedPath, string name, SyncItem syncItem, bool initializeSyncItem)
            : base(rootPath, relatedPath, name, syncItem)
        {
          if (initializeSyncItem)
          {
            this._item = syncItem;
          }
        }

        public virtual bool HasItem => (_item = GetItem()) != null;

        protected override SyncItem GetItem()
        {
            return _item = base.GetItem();
        }

        public override IList<ICommand> GenerateUpdateCommand(IDataItem dataItem)
        {
            var list = new List<ICommand>();
            Assert.IsTrue(dataItem is ContentDataItem, "Incorrect item type. Probably problem with engine");
            var target = dataItem as ContentDataItem;
            Assert.IsNotNull(target, "DataItem");
            if (CompareIds(target) != 0)
            {
                list.AddRange(this.GenerateDeleteCommand());
                list.AddRange(target.GenerateAddCommand());
                return list;
            }
            var item = new ChangeItemCommand(this, target);
            if (!item.IsEmpty)
            {
                list.Add(item);
            }
            return list;
        }

        public override int CompareTo(object obj)
        {
            var isEqual = CompareIds(obj);
            return isEqual == 0 ? DataItemCompareTo(obj) : isEqual;
        }

        public virtual int CompareIds(object obj)
        {
            if (obj is QuickContentDataItem)
            {
                return GetFastId().CompareTo(((QuickContentDataItem)obj).GetFastId());
            }
            else if (obj is ContentDataItem)
            {
                return this.ItemID.CompareTo((obj as ContentDataItem).ItemID);
            }
            return 0;
        }

        protected virtual Guid GetFastId()
        {
            if (_id != Guid.Empty)
                return _id;
            if (_item != null)
                return _id = Guid.Parse(_item.ID);
            using (var reader = File.OpenText(this.ItemPath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("id: "))
                    {
                        return _id = Guid.Parse(line.Substring("id: ".Length));
                    }
                }
            }
            return Guid.Empty;
        }

        //Copied because of skipping base to base.base
        private int DataItemCompareTo(object obj)
        {
            DataItem dataItem = obj as DataItem;
            Assert.IsNotNull((object)dataItem, "DataItem");
            if (this.CompareStrings(this.RelatedPath, dataItem.RelatedPath) == 0 &&
                this.CompareStrings(this.Name, dataItem.Name) == 0)
                return 0;
            string[] strArray1 = this.RelatedPath.ToLowerInvariant().Split(new char[1]
            {
                '\\'
            }, StringSplitOptions.RemoveEmptyEntries);
            string[] strArray2 = dataItem.RelatedPath.ToLowerInvariant().Split(new char[1]
            {
                '\\'
            }, StringSplitOptions.RemoveEmptyEntries);
            if (strArray1.Length > strArray2.Length)
                return -1;
            if (strArray1.Length < strArray2.Length)
                return 1;
            //bool isDirectory1 = this.IsDirectory;
            //bool isDirectory2 = dataItem.IsDirectory;
            //if (isDirectory1 == isDirectory2)
            {
                int index = 0;
                int num = strArray1.Length > strArray2.Length ? strArray2.Length : strArray1.Length;
                while (index < num && this.CompareStrings(strArray1[index], strArray2[index]) == 0)
                    ++index;
                if (index < num)
                    return strArray1[index].CompareTo(strArray2[index]);
            }
            //else if (isDirectory1)
            //    return 1;
            return -1;
        }

    }

}
