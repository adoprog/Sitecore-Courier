using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Sitecore.Data.Serialization.ObjectModel;
using Sitecore.Update;
using Sitecore.Update.Commands;
using Sitecore.Update.Data;
using Sitecore.Update.Interfaces;

namespace Sitecore.Courier.Runner.ExclusionsTests
{
  public class DiffGeneratorTests
  {
    [Test]
    public void DeleteItemsTest()
    {
      // Arrange
      var sourceItems = new List<IDataItem>();
      var sourceItem = new QuickContentDataItem(string.Empty, string.Empty, string.Empty, new SyncItem());
      sourceItems.Add(sourceItem);

      var targetItems = new List<IDataItem>();
      var sourceDataIterator = new TestDataIterator(sourceItems);
      var targetDataIterator = new TestDataIterator(targetItems);

      var engineMock = new Mock<DataEngine>(null, null, new List<ICommandFilter>());

      // Act
      var commands = DiffGenerator.GetDiffCommands(string.Empty, CollisionBehavior.Force, sourceDataIterator, targetDataIterator, engineMock.Object);

      //Assert
      Assert.AreEqual(1, commands.Count);
      Assert.AreEqual(1, commands.Count(x => x is DeleteItemCommand));
    }

    [Test]
    public void AddItemsTest()
    {
      // Arrange
      var sourceItems = new List<IDataItem>();

      var targetItems = new List<IDataItem>();
      var targetItem = new QuickContentDataItem(string.Empty, string.Empty, string.Empty, new SyncItem());
      targetItems.Add(targetItem);

      var sourceDataIterator = new TestDataIterator(sourceItems);
      var targetDataIterator = new TestDataIterator(targetItems);

      var engineMock = new Mock<DataEngine>(null, null, new List<ICommandFilter>());

      // Act
      var commands = DiffGenerator.GetDiffCommands(string.Empty, CollisionBehavior.Force, sourceDataIterator, targetDataIterator, engineMock.Object);

      //Assert
      Assert.AreEqual(1, commands.Count);
      Assert.AreEqual(1, commands.Count(x => x is AddItemCommand));
    }

    [Test]
    public void MoveItemsTest()
    {
      // Arrange
      var id = Guid.NewGuid().ToString();
      var sourceItems = new List<IDataItem>();
      var sourceItem = new QuickContentDataItem(string.Empty, string.Empty, string.Empty, new SyncItem() { ID = id, TemplateID = id, ItemPath = "/sitecore/content/Home/Test1" }, true);
      sourceItems.Add(sourceItem);

      var targetItems = new List<IDataItem>();
      var targetItem = new QuickContentDataItem(string.Empty, string.Empty, string.Empty, new SyncItem() { ID = id, TemplateID = id, ItemPath = "/sitecore/content/Home/Test2" }, true);
      targetItems.Add(targetItem);

      var sourceDataIterator = new TestDataIterator(sourceItems);
      var targetDataIterator = new TestDataIterator(targetItems);

      var engineMock = new Mock<DataEngine>(null, null, new List<ICommandFilter>());

      // Act
      var commands = DiffGenerator.GetDiffCommands(string.Empty, CollisionBehavior.Force, sourceDataIterator, targetDataIterator, engineMock.Object);

      //Assert
      Assert.AreEqual(1, commands.Count);
      Assert.AreEqual(1, commands.Count(x => x is ChangeItemCommand));
    }
  }

  public class TestDataIterator : IDataIterator
  {
    readonly List<IDataItem> items;

    public TestDataIterator(List<IDataItem> items)
    {
      this.items = items;
    }

    public IDataItem Next()
    {
      if (items.Any())
      {
        var item = items.First();
        items.Remove(item);
        return item;
      }

      return null;
    }
  }
}