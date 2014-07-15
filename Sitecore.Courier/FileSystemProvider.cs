namespace Sitecore.Courier
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.IO;

  using Sitecore.Data.Serialization.ObjectModel;
  using Sitecore.Update;
  using Sitecore.Update.Data.Items;
  using Sitecore.Update.Interfaces;
  using Sitecore.Update.Utils;

  /// <summary>
  /// The File system data provider.
  /// 
  /// </summary>
  public class FileSystemProvider : BaseDataProcessor, IDataProvider
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="T:Sitecore.Courier.FileSystemProvider"/> class.
    /// 
    /// </summary>
    /// <param name="name">The data name.</param>
    public FileSystemProvider(string name)
      : base(name)
    {
    }

    /// <summary>
    /// Clones this instance.
    /// 
    /// </summary>
    /// 
    /// <returns>
    /// The instance.
    /// </returns>
    public virtual object Clone()
    {
      return (object)new FileSystemProvider(this.Name);
    }

    /// <summary>
    /// Determines whether this provider can process the specified source path.
    /// 
    /// </summary>
    /// <param name="sourcePath">The source path.</param>
    /// <returns>
    /// <c>true</c> if this provider can process the specified source path; otherwise, <c>false</c>.
    /// 
    /// </returns>
    public bool CanProcess(string sourcePath)
    {
      try
      {
        if (!string.IsNullOrEmpty(sourcePath))
        {
          string path = sourcePath.Trim();
          if (path.EndsWith("/"))
            path = path.Substring(0, path.Length - 1);
          return Directory.Exists(path);
        }
      }
      catch (Exception ex)
      {
        Trace.TraceWarning("Can't resolve source path: " + (object)ex);
      }
      return false;
    }

    /// <summary>
    /// Gets the iterator.
    /// 
    /// </summary>
    /// <param name="rootPath">The root path.</param><param name="filters">The filters.</param>
    /// <returns>
    /// The Data iterator.
    /// </returns>
    public IDataIterator GetIterator(string rootPath, IList<Sitecore.Update.Filters.Filter> filters)
    {
      return (IDataIterator)new FileSystemProvider.FileSystemDataIterator(rootPath, filters);
    }

    /// <summary>
    /// The file syste data iterator.
    /// 
    /// </summary>
    public class FileSystemDataIterator : BaseDataIterator
    {
      /// <summary>
      /// The related root path.
      /// 
      /// </summary>
      private string rootPath;
      /// <summary>
      /// The root path.
      /// 
      /// </summary>
      private string root;
      /// <summary>
      /// The file stack.
      /// 
      /// </summary>
      private Stack<FileSystemProvider.FileSystemDataIterator> stack;
      /// <summary>
      /// The data pair.
      /// 
      /// </summary>
      private FileSystemProvider.FileSystemDataIterator.DataPair[] dataPairs;
      /// <summary>
      /// The pair position.
      /// 
      /// </summary>
      private int position;

      /// <summary>
      /// Initializes a new instance of the <see cref="T:Sitecore.Courier.FileSystemProvider.FileSystemDataIterator"/> class.
      /// 
      /// </summary>
      /// <param name="root">The root path.</param><param name="filters">The filters.</param>
      public FileSystemDataIterator(string root, IList<Sitecore.Update.Filters.Filter> filters)
        : this(root, root, filters)
      {
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="T:Sitecore.Courier.FileSystemProvider.FileSystemDataIterator"/> class.
      /// 
      /// </summary>
      /// <param name="root">The root path.</param><param name="relatedRoot">The related root path.</param><param name="filters">The filters.</param>
      private FileSystemDataIterator(string root, string relatedRoot, IList<Sitecore.Update.Filters.Filter> filters)
        : base(filters)
      {
        this.root = root.Trim(new char[1]
        {
          '\\'
        });
        this.rootPath = relatedRoot.Trim(new char[1]
        {
          '\\'
        });
        this.stack = new Stack<FileSystemProvider.FileSystemDataIterator>();
        this.position = 0;
        this.InitializeDataPairs(this.rootPath);
      }

      /// <summary>
      /// Return next instance in data collection.
      /// 
      /// </summary>
      /// 
      /// <returns>
      /// The Next data item in data collection.
      /// </returns>
      public override IDataItem Next()
      {
        try
        {
          while (this.stack.Count > 0)
          {
            FileSystemProvider.FileSystemDataIterator systemDataIterator = this.stack.Pop();
            if (systemDataIterator != null)
            {
              IDataItem dataItem = systemDataIterator.Next();
              if (dataItem != null)
              {
                this.stack.Push(systemDataIterator);
                return dataItem;
              }
            }
            else
              break;
          }
          if (this.position >= this.dataPairs.Length)
            return (IDataItem)null;
          FileSystemProvider.FileSystemDataIterator.DataPair dataPair = this.dataPairs[this.position++];
          if (Directory.Exists(dataPair.Info.FullName))
          {
            this.stack.Push(new FileSystemProvider.FileSystemDataIterator(this.root, dataPair.Info.FullName, this.Filters));
            return dataPair.DataItem;
          }
          else
          {
            if (File.Exists(dataPair.Info.FullName))
              return dataPair.DataItem;
            Trace.TraceError(string.Format("Can't find file system entity '{0}'", (object)dataPair.Info.FullName));
          }
        }
        catch (Exception ex)
        {
          Trace.TraceError(((object)ex).ToString());
        }
        return (IDataItem)null;
      }

      /// <summary>
      /// Initializes the data pairs.
      /// 
      /// </summary>
      /// <param name="root">The root path.</param><exception cref="T:System.Exception"><c>Exception</c>.</exception>
      protected virtual void InitializeDataPairs(string root)
      {
        if (!Directory.Exists(root))
        {
          this.dataPairs = new FileSystemProvider.FileSystemDataIterator.DataPair[0];
          Trace.TraceWarning("Directoty does not exist " + root);
        }
        else
        {
          List<FileSystemProvider.FileSystemDataIterator.DataPair> list1 = new List<FileSystemProvider.FileSystemDataIterator.DataPair>();
          try
          {
            DirectoryInfo directoryInfo1 = new DirectoryInfo(root);
            List<DirectoryInfo> list2 = new List<DirectoryInfo>((IEnumerable<DirectoryInfo>)directoryInfo1.GetDirectories());
            list2.Sort((IComparer<DirectoryInfo>)new FileSystemProvider.FileSystemDataIterator.FileSystemInfoComparer<DirectoryInfo>());
            List<FileInfo> list3 = new List<FileInfo>((IEnumerable<FileInfo>)directoryInfo1.GetFiles());
            list3.Sort((IComparer<FileInfo>)new FileSystemProvider.FileSystemDataIterator.FileSystemInfoComparer<FileInfo>());
            foreach (FileInfo file in list3)
            {
              if (file != null)
              {
                if (SerializationUtils.IsItemSerialization(file.FullName))
                {
                  DirectoryInfo directoryInfo2 = (DirectoryInfo)null;
                  string strB = file.FullName.Substring(0, file.FullName.Length - ".item".Length);
                  foreach (DirectoryInfo directoryInfo3 in list2)
                  {
                    if (string.Compare(directoryInfo3.FullName, strB, true) == 0)
                    {
                      directoryInfo2 = directoryInfo3;
                      list2.Remove(directoryInfo3);
                      break;
                    }
                  }
                  if (directoryInfo2 != null)
                  {
                    IDataItem dataItem = (IDataItem)new ContentDataItem(this.root, this.root.Length == file.Directory.FullName.Length ? string.Empty : file.Directory.FullName.Substring(this.root.Length), file.Name);
                    if (this.IsAllowed(dataItem))
                      list1.Add(new FileSystemProvider.FileSystemDataIterator.DataPair((FileSystemInfo)directoryInfo2, dataItem));
                  }
                  else
                  {
                    IDataItem dataItem = (IDataItem)new ContentDataItem(this.root, this.root.Length == file.Directory.FullName.Length ? string.Empty : file.Directory.FullName.Substring(this.root.Length), file.Name);
                    if (this.IsAllowed(dataItem))
                      list1.Add(new FileSystemProvider.FileSystemDataIterator.DataPair((FileSystemInfo)file, dataItem));
                  }
                }
                else
                {
                  IDataItem dataItem = (IDataItem)ItemUtils.GetFileSystemDataItem(this.root, file);
                  if (this.IsAllowed(dataItem))
                    list1.Add(new FileSystemProvider.FileSystemDataIterator.DataPair((FileSystemInfo)file, dataItem));
                }
              }
            }
            foreach (DirectoryInfo directory in list2)
            {
              IDataItem dataItem = (IDataItem)Update.Utils.ItemUtils.GetSystemFolderDataItem(this.root, directory);
              if (this.IsAllowed(dataItem))
                list1.Add(new FileSystemProvider.FileSystemDataIterator.DataPair((FileSystemInfo)directory, dataItem));
            }
          }
          catch (Exception ex)
          {
            Trace.TraceError("Can't initialize provider. Exception: " + (object)ex);
            throw ex;
          }
          this.dataPairs = list1.ToArray();
        }
      }

      /// <summary>
      /// The Data pair information.
      /// 
      /// </summary>
      protected struct DataPair
      {
        /// <summary>
        /// The file information.
        /// 
        /// </summary>
        private FileSystemInfo info;
        /// <summary>
        /// The data item.
        /// 
        /// </summary>
        private IDataItem dataItem;

        /// <summary>
        /// Gets the file information.
        /// 
        /// </summary>
        /// 
        /// <value>
        /// The file information.
        /// </value>
        public FileSystemInfo Info
        {
          get
          {
            return this.info;
          }
        }

        /// <summary>
        /// Gets the data item.
        /// 
        /// </summary>
        /// 
        /// <value>
        /// The data item.
        /// </value>
        public IDataItem DataItem
        {
          get
          {
            return this.dataItem;
          }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Sitecore.Courier.FileSystemProvider.FileSystemDataIterator.DataPair"/> struct.
        /// 
        /// </summary>
        /// <param name="info">The file information.</param><param name="item">The data item.</param>
        public DataPair(FileSystemInfo info, IDataItem item)
        {
          this.info = info;
          this.dataItem = item;
        }
      }

      /// <summary>
      /// The file camparer.
      /// 
      /// </summary>
      /// <typeparam name="T">The comparable type.</typeparam>
      public class FileSystemInfoComparer<T> : IComparer<T> where T : FileSystemInfo
      {
        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// 
        /// </summary>
        /// <param name="x">The first object to compare.</param><param name="y">The second object to compare.</param>
        /// <returns>
        /// Value
        ///             Condition
        ///             Less than zero
        ///             <paramref name="x"/> is less than <paramref name="y"/>.
        ///             Zero
        ///             <paramref name="x"/> equals <paramref name="y"/>.
        ///             Greater than zero
        ///             <paramref name="x"/> is greater than <paramref name="y"/>.
        /// 
        /// </returns>
        public int Compare(T x, T y)
        {
          string strA = (string)null;
          string strB = (string)null;
          if ((object)x != null)
            strA = x.FullName;
          if ((object)y != null)
            strB = y.FullName;
          return string.Compare(strA, strB);
        }
      }
    }
  }
}
