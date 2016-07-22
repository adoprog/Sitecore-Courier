using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Sitecore.Data.Serialization.ObjectModel;
using Sitecore.Update;
using Sitecore.Update.Filters;
using Sitecore.Update.Interfaces;

namespace Sitecore.Courier
{
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
      return new FileSystemProvider(Name);
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
        Trace.TraceWarning("Can't resolve source path: " + ex);
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
    public IDataIterator GetIterator(string rootPath, IList<Filter> filters)
    {
      return new FileSystemDataIterator(rootPath, filters);
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
      private Stack<FileSystemDataIterator> stack;
      /// <summary>
      /// The data pair.
      /// 
      /// </summary>
      private DataPair[] dataPairs;
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
      public FileSystemDataIterator(string root, IList<Filter> filters)
        : this(root, root, filters)
      {
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="T:Sitecore.Courier.FileSystemProvider.FileSystemDataIterator"/> class.
      /// 
      /// </summary>
      /// <param name="root">The root path.</param><param name="relatedRoot">The related root path.</param><param name="filters">The filters.</param>
      private FileSystemDataIterator(string root, string relatedRoot, IList<Filter> filters)
        : base(filters)
      {
        this.root = root.Trim('\\');
        rootPath = relatedRoot.Trim('\\');
        stack = new Stack<FileSystemDataIterator>();
        position = 0;
        InitializeDataPairs(rootPath);
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
          while (stack.Count > 0)
          {
            FileSystemDataIterator systemDataIterator = stack.Pop();
            if (systemDataIterator != null)
            {
              IDataItem dataItem = systemDataIterator.Next();
              if (dataItem != null)
              {
                stack.Push(systemDataIterator);
                return dataItem;
              }
            }
            else
              break;
          }
          if (position >= dataPairs.Length)
            return null;
          DataPair dataPair = dataPairs[position];
          
          /* After we return our DataItem and it's actually read from the File System, its size might become pretty high. For instance if our *.item takes 200MB, DataItem object size might be over 400MB.
           * If DataItem is not used more (e.g. by AddCommand), GC is free to collect it. However our dataPairs collection root prevents object from being collected.
           * As result the Couirer tool memory usage might be pretty high.
           * Given that we don't get back to the this.dataPairs[position] struct more, we could safely clean it.
           * 
           * Local test demostrated that if DIFF package contains nothing (no commands are present), peak memory usage is reduced from 10GB to 2GB (on local test input).*/
          dataPairs[position] = new DataPair();
          position++;

          if (Directory.Exists(dataPair.Info.FullName))
          {
            stack.Push(new FileSystemDataIterator(root, dataPair.Info.FullName, Filters));
            return dataPair.DataItem;
          }
            if (File.Exists(dataPair.Info.FullName))
                return dataPair.DataItem;
            Trace.TraceError(string.Format("Can't find file system entity '{0}'", dataPair.Info.FullName));
        }
        catch (Exception ex)
        {
          Trace.TraceError(ex.ToString());
        }
        return null;
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
          dataPairs = new DataPair[0];
          Trace.TraceWarning("Directoty does not exist " + root);
        }
        else
        {
          List<DataPair> result = new List<DataPair>();
          try
          {
            var rootDirectory = new DirectoryInfo(root);
            var directories = new List<DirectoryInfo>(rootDirectory.GetDirectories());
            directories.Sort(new FileSystemInfoComparer<DirectoryInfo>());
            var files = new List<FileInfo>(rootDirectory.GetFiles());
            files.Sort(new FileSystemInfoComparer<FileInfo>());
            foreach (var file in files.Where(file => file != null))
            {
                if (SerializationUtils.IsItemSerialization(file.FullName))
                {
                    DirectoryInfo itemDirectory = null;
                    var dirName = file.FullName.Substring(0, file.FullName.Length - ".item".Length);
                    foreach (var dir in  directories.Where(d => string.Compare(d.FullName, dirName, true) == 0))
                    {
                        itemDirectory = dir;
                        directories.Remove(dir);
                        break;
                    }
                    if (itemDirectory != null)
                    {
                        var dataItem = new QuickContentDataItem(this.root, this.root.Length == file.Directory.FullName.Length ? string.Empty : file.Directory.FullName.Substring(this.root.Length), file.Name);
                        if (IsAllowed(dataItem))
                            result.Add(new DataPair(itemDirectory, dataItem));
                    }
                    else
                    {
                        var dataItem = new QuickContentDataItem(this.root, this.root.Length == file.Directory.FullName.Length ? string.Empty : file.Directory.FullName.Substring(this.root.Length), file.Name);
                        if (IsAllowed(dataItem))
                            result.Add(new DataPair(file, dataItem));
                    }
                }
                else
                {
                    var dataItem = ItemUtils.GetFileSystemDataItem(this.root, file);
                    if (IsAllowed(dataItem))
                        result.Add(new DataPair(file, dataItem));
                }
            }
            result.AddRange(
                from directory in directories
                let dataItem = Update.Utils.ItemUtils.GetSystemFolderDataItem(this.root, directory)
                where IsAllowed(dataItem)
                select new DataPair(directory, dataItem));
          }
          catch (Exception ex)
          {
            Trace.TraceError("Can't initialize provider. Exception: " + ex);
            throw ex;
          }
          dataPairs = result.ToArray();
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
            return info;
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
            return dataItem;
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
          dataItem = item;
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
          string strA = null;
          string strB = null;
          if (x != null)
            strA = x.FullName;
          if (y != null)
            strB = y.FullName;
          return string.Compare(strA, strB);
        }
      }
    }
  }
}
