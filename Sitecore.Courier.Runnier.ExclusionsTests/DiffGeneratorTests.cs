using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Sitecore.Data.Serialization.ObjectModel;
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
            var commands = DiffGenerator.GetCommands(sourceDataIterator, targetDataIterator);

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
            var commands = DiffGenerator.GetCommands(sourceDataIterator, targetDataIterator);

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
            var commands = DiffGenerator.GetCommands(sourceDataIterator, targetDataIterator);

            //Assert
            Assert.AreEqual(1, commands.Count);
            Assert.AreEqual(1, commands.Count(x => x is ChangeItemCommand));
        }

        [Test]
        public void ChangedUnicornSerializationNameAndDeletionTest()
        {
            //Source dataset, a template with 2 fields. Serialized to a unicorn setting named Unicorn1
            var sourceItems = new List<IDataItem>();
            sourceItems.Add(new QuickContentDataItem("C:\\Source", "Unicorn1", "Template 1.yml", 
                new SyncItem() {
                    ID = "2a54b494-68fe-44f6-85d9-88a3b57cf689",
                    TemplateID = "ab86861a-6030-46c5-b394-e8f99e8b87db",
                    ParentID = "3C1715FE-6A13-4FCF-845F-DE308BA9741D",
                    ItemPath = "/sitecore/Templates/Template 1" }, true));
            sourceItems.Add(new QuickContentDataItem("C:\\Source", "Unicorn1\\Template 1", "Section 1.yml",
                new SyncItem()
                {
                    ID = "79a314b1-82ad-4733-bb82-92cc62306c31",
                    TemplateID = "e269fbb5-3750-427a-9149-7aa950b49301",
                    ParentID = "2a54b494-68fe-44f6-85d9-88a3b57cf689",
                    ItemPath = "/sitecore/Templates/Template 1/Section 1"
                }, true));
            sourceItems.Add(new QuickContentDataItem("C:\\Source", "Unicorn1\\Template 1\\Section 1", "Field1.yml",
                new SyncItem()
                {
                    ID = "dcfcf4d9-6fed-4f16-a8a3-acf22188dd74",
                    TemplateID = "455a3e98-a627-4b40-8035-e683a0331ac7",
                    ParentID = "79a314b1-82ad-4733-bb82-92cc62306c31",
                    ItemPath = "/sitecore/Templates/Template 1/Section 1/Field1"
                }, true));
            sourceItems.Add(new QuickContentDataItem("C:\\Source", "Unicorn1\\Template 1\\Section 1", "Field2.yml",
                new SyncItem()
                {
                    ID = "dcfcf4d9-6fed-4f16-a8a3-acf22188dd73",
                    TemplateID = "455a3e98-a627-4b40-8035-e683a0331ac7",
                    ParentID = "79a314b1-82ad-4733-bb82-92cc62306c31",
                    ItemPath = "/sitecore/Templates/Template 1/Section 1/Field2"
                }, true));

            //Target dataset, same template, 1 field removed. Serialized to a unicorn setting named Unicorn2
            var targetItems = new List<IDataItem>();
            targetItems.Add(new QuickContentDataItem("C:\\Target", "Unicorn2", "Template 1.yml",
                new SyncItem()
                {
                    ID = "2a54b494-68fe-44f6-85d9-88a3b57cf689",
                    TemplateID = "ab86861a-6030-46c5-b394-e8f99e8b87db",
                    ParentID = "3C1715FE-6A13-4FCF-845F-DE308BA9741D",
                    ItemPath = "/sitecore/Templates/Template 1"
                }, true));
            targetItems.Add(new QuickContentDataItem("C:\\Target", "Unicorn2\\Template 1", "Section 1.yml",
                new SyncItem()
                {
                    ID = "79a314b1-82ad-4733-bb82-92cc62306c31",
                    TemplateID = "e269fbb5-3750-427a-9149-7aa950b49301",
                    ParentID = "2a54b494-68fe-44f6-85d9-88a3b57cf689",
                    ItemPath = "/sitecore/Templates/Template 1/Section 1"
                }, true));
            targetItems.Add(new QuickContentDataItem("C:\\Target", "Unicorn2\\Template 1\\Section 1", "Field1.yml",
                new SyncItem()
                {
                    ID = "dcfcf4d9-6fed-4f16-a8a3-acf22188dd74",
                    TemplateID = "455a3e98-a627-4b40-8035-e683a0331ac7",
                    ParentID = "79a314b1-82ad-4733-bb82-92cc62306c31",
                    ItemPath = "/sitecore/Templates/Template 1/Section 1/Field1"
                }, true));

            var sourceDataIterator = new TestDataIterator(sourceItems);
            var targetDataIterator = new TestDataIterator(targetItems);

            var engineMock = new Mock<DataEngine>(null, null, new List<ICommandFilter>());

            // Act
            var commands = DiffGenerator.GetCommands(sourceDataIterator, targetDataIterator);

            //Assert
            Assert.AreEqual(1, commands.Count);
            Assert.AreEqual(1, commands.Count(x => x is DeleteItemCommand));
        }
    
        [Test]
        public void ChangedUnicornSerializationNameAndDeletionTestNew()
        {
            //Source dataset, a template with 2 fields. Serialized to a unicorn setting named Unicorn1
            var sourceItems = new List<IDataItem>();
            sourceItems.Add(new QuickContentDataItem("C:\\Source", "Unicorn1", "Template 1.yml", 
                new SyncItem() {
                    ID = "2a54b494-68fe-44f6-85d9-88a3b57cf689",
                    TemplateID = "ab86861a-6030-46c5-b394-e8f99e8b87db",
                    ParentID = "3C1715FE-6A13-4FCF-845F-DE308BA9741D",
                    ItemPath = "/sitecore/Templates/Template 1" }, true));
            sourceItems.Add(new QuickContentDataItem("C:\\Source", "Unicorn1\\Template 1", "Section 1.yml",
                new SyncItem()
                {
                    ID = "79a314b1-82ad-4733-bb82-92cc62306c31",
                    TemplateID = "e269fbb5-3750-427a-9149-7aa950b49301",
                    ParentID = "2a54b494-68fe-44f6-85d9-88a3b57cf689",
                    ItemPath = "/sitecore/Templates/Template 1/Section 1"
                }, true));
            sourceItems.Add(new QuickContentDataItem("C:\\Source", "Unicorn1\\Template 1\\Section 1", "Field1.yml",
                new SyncItem()
                {
                    ID = "dcfcf4d9-6fed-4f16-a8a3-acf22188dd74",
                    TemplateID = "455a3e98-a627-4b40-8035-e683a0331ac7",
                    ParentID = "79a314b1-82ad-4733-bb82-92cc62306c31",
                    ItemPath = "/sitecore/Templates/Template 1/Section 1/Field1"
                }, true));
            sourceItems.Add(new QuickContentDataItem("C:\\Source", "Unicorn1\\Template 1\\Section 1", "Field2.yml",
                new SyncItem()
                {
                    ID = "dcfcf4d9-6fed-4f16-a8a3-acf22188dd73",
                    TemplateID = "455a3e98-a627-4b40-8035-e683a0331ac7",
                    ParentID = "79a314b1-82ad-4733-bb82-92cc62306c31",
                    ItemPath = "/sitecore/Templates/Template 1/Section 1/Field2"
                }, true));

            //Target dataset, same template, 1 field removed. Serialized to a unicorn setting named Unicorn2
            var targetItems = new List<IDataItem>();
            targetItems.Add(new QuickContentDataItem("C:\\Target", "Unicorn2", "Template 1.yml",
                new SyncItem()
                {
                    ID = "2a54b494-68fe-44f6-85d9-88a3b57cf689",
                    TemplateID = "ab86861a-6030-46c5-b394-e8f99e8b87db",
                    ParentID = "3C1715FE-6A13-4FCF-845F-DE308BA9741D",
                    ItemPath = "/sitecore/Templates/Template 1"
                }, true));
            targetItems.Add(new QuickContentDataItem("C:\\Target", "Unicorn2\\Template 1", "Section 1.yml",
                new SyncItem()
                {
                    ID = "79a314b1-82ad-4733-bb82-92cc62306c31",
                    TemplateID = "e269fbb5-3750-427a-9149-7aa950b49301",
                    ParentID = "2a54b494-68fe-44f6-85d9-88a3b57cf689",
                    ItemPath = "/sitecore/Templates/Template 1/Section 1"
                }, true));
            targetItems.Add(new QuickContentDataItem("C:\\Target", "Unicorn2\\Template 1\\Section 1", "Field1.yml",
                new SyncItem()
                {
                    ID = "dcfcf4d9-6fed-4f16-a8a3-acf22188dd74",
                    TemplateID = "455a3e98-a627-4b40-8035-e683a0331ac7",
                    ParentID = "79a314b1-82ad-4733-bb82-92cc62306c31",
                    ItemPath = "/sitecore/Templates/Template 1/Section 1/Field1"
                }, true));

            var sourceDataIterator = new TestDataIterator(sourceItems);
            var targetDataIterator = new TestDataIterator(targetItems);

            // Act
            var commands = DiffGenerator.GetCommands(sourceDataIterator, targetDataIterator);

            //Assert
            Assert.AreEqual(1, commands.Count);
            Assert.AreEqual(1, commands.Count(x => x is DeleteItemCommand));
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