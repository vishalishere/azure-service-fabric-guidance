namespace Tailspin.Web.AcceptanceTests.Stores.AzureStorage
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.StorageClient;
    using Tailspin.Web.Survey.Shared.Helpers;
    using Tailspin.Web.Survey.Shared.Stores.AzureStorage;

    [TestClass]
    public class AzureBlobContainerFixture
    {
        private const string AzureBlobTestContainer = "azureblobcontainerfortest";

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            var azureBlobContainer = new TestAzureBlobContainer(
                CloudConfiguration.GetStorageAccount("DataConnectionString"),
                AzureBlobTestContainer);
            azureBlobContainer.EnsureExist();
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            var azureBlobContainer = new TestAzureBlobContainer(
                 CloudConfiguration.GetStorageAccount("DataConnectionString"),
                 AzureBlobTestContainer);
            azureBlobContainer.DeleteContainer();
        }

        [TestMethod]
        public void DeleteShouldRemoveTheBlob()
        {
            var objId = Guid.NewGuid().ToString();

            var azureBlobContainer = new TestAzureBlobContainer(
                CloudConfiguration.GetStorageAccount("DataConnectionString"),
                AzureBlobTestContainer);
            azureBlobContainer.Save(objId, "testText");

            Assert.IsNotNull(azureBlobContainer.Get(objId));

            azureBlobContainer.Delete(objId);

            Assert.IsNull(azureBlobContainer.Get(objId));
        }

        [TestMethod]
        public void GetShouldRetrieveTheBlob()
        {
            var objId = Guid.NewGuid().ToString();

            var azureBlobContainer = new TestAzureBlobContainer(
                CloudConfiguration.GetStorageAccount("DataConnectionString"),
                AzureBlobTestContainer);
            azureBlobContainer.Save(objId, "testText");

            Assert.IsNotNull(azureBlobContainer.Get(objId));
        }

        [TestMethod]
        public void SaveShouldStoreTheBlob()
        {
            var objId = Guid.NewGuid().ToString();

            var azureBlobContainer = new TestAzureBlobContainer(
                CloudConfiguration.GetStorageAccount("DataConnectionString"),
                AzureBlobTestContainer);
            azureBlobContainer.Save(objId, "testText");

            Assert.IsNotNull(azureBlobContainer.Get(objId));
        }

        [TestMethod]
        public void GetBlobListReturnsAllBlobsInContainer()
        {
            var objId1 = Guid.NewGuid().ToString();
            var objId2 = Guid.NewGuid().ToString();

            var azureBlobContainer = new TestAzureBlobContainer(
                CloudConfiguration.GetStorageAccount("DataConnectionString"),
                AzureBlobTestContainer);
            azureBlobContainer.Save(objId1, "testText");
            azureBlobContainer.Save(objId2, "testText");

            var blobList = azureBlobContainer.GetBlobList().Select(b => b.Name).ToList();

            CollectionAssert.Contains(blobList, objId1);
            CollectionAssert.Contains(blobList, objId2);
        }

        [TestMethod]
        public void GetUriReturnsContainerUrl()
        {
            var objId = Guid.NewGuid().ToString();

            var azureBlobContainer = new TestAzureBlobContainer(
                CloudConfiguration.GetStorageAccount("DataConnectionString"),
                AzureBlobTestContainer);
            Assert.AreEqual(
                string.Format("http://127.0.0.1:10000/devstoreaccount1/{0}/{1}", AzureBlobTestContainer, objId),
                azureBlobContainer.GetUri(objId).ToString());
        }

        [TestMethod]
        public void LockFreeResourceReturnsTrueWithLockId()
        {
            var azureBlobContainer = new TestAzureBlobContainer(
                CloudConfiguration.GetStorageAccount("DataConnectionString"),
                AzureBlobTestContainer);

            var objId = Guid.NewGuid().ToString();
            azureBlobContainer.Save(objId, "testText");

            var lockContext = new PessimisticConcurrencyContext()
            {
                ObjectId = objId
            };

            Assert.IsTrue(azureBlobContainer.AcquireLock(lockContext));
            Assert.IsNotNull(lockContext.LockId);
        }

        [TestMethod]
        public void LockBusyResourceReturnsFalseWithNoLockId()
        {
            var azureBlobContainer = new TestAzureBlobContainer(
                CloudConfiguration.GetStorageAccount("DataConnectionString"),
                AzureBlobTestContainer);

            var objId = Guid.NewGuid().ToString();
            azureBlobContainer.Save(objId, "testText");

            var lockContext = new PessimisticConcurrencyContext()
            {
                ObjectId = objId
            };

            Assert.IsTrue(azureBlobContainer.AcquireLock(lockContext));
            Assert.IsNotNull(lockContext.LockId);

            Assert.IsFalse(azureBlobContainer.AcquireLock(lockContext));
            Assert.IsNull(lockContext.LockId);
        }

        [TestMethod]
        public void LockUnexistentResourceReturnsTrueWithNoLockId()
        {
            var azureBlobContainer = new TestAzureBlobContainer(
                CloudConfiguration.GetStorageAccount("DataConnectionString"),
                AzureBlobTestContainer);

            var lockContext = new PessimisticConcurrencyContext()
            {
                ObjectId = Guid.NewGuid().ToString()
            };

            Assert.IsTrue(azureBlobContainer.AcquireLock(lockContext));
            Assert.IsNull(lockContext.LockId);
        }

        [TestMethod]
        public void ReleaseResourceAllowsGettingNewLock()
        {
            var azureBlobContainer = new TestAzureBlobContainer(
                CloudConfiguration.GetStorageAccount("DataConnectionString"),
                AzureBlobTestContainer);

            var objId = Guid.NewGuid().ToString();
            azureBlobContainer.Save(objId, "testText");

            var lockContext = new PessimisticConcurrencyContext()
            {
                ObjectId = objId
            };

            Assert.IsTrue(azureBlobContainer.AcquireLock(lockContext));
            Assert.IsNotNull(lockContext.LockId);
            var firstLockId = lockContext.LockId;

            azureBlobContainer.ReleaseLock(lockContext);

            Assert.IsTrue(azureBlobContainer.AcquireLock(lockContext));
            Assert.IsNotNull(lockContext.LockId);
            Assert.AreNotEqual(firstLockId, lockContext.LockId);
        }

        [TestMethod]
        public void OptimisticCreateNewBlob()
        {
            var azureBlobContainer = new TestAzureBlobContainer(
                CloudConfiguration.GetStorageAccount("DataConnectionString"),
                AzureBlobTestContainer);

            var objId = Guid.NewGuid().ToString();

            OptimisticConcurrencyContext context;
            var text = azureBlobContainer.Get(objId, out context);

            Assert.IsNull(text);
            Assert.AreEqual(context.ObjectId, objId);

            azureBlobContainer.Save(objId, "testText");

            text = azureBlobContainer.Get(objId, out context);

            Assert.IsNotNull(text);
            Assert.AreEqual(context.ObjectId, objId);
        }

        private class TestAzureBlobContainer : AzureBlobContainer<string>
        {
            public TestAzureBlobContainer(CloudStorageAccount account, string containerName) : base(account, containerName) { }

            protected override string DoGet(string objId, out OptimisticConcurrencyContext context)
            {
                CloudBlob blob = this.Container.GetBlobReference(objId);
                blob.FetchAttributes();
                context = new OptimisticConcurrencyContext()
                {
                    ObjectId = objId,
                    AccessCondition = AccessCondition.IfMatch(blob.Properties.ETag)
                };
                return blob.DownloadText();
            }

            protected override void DoSave(string objId, string obj)
            {
                CloudBlob blob = this.Container.GetBlobReference(objId);
                blob.UploadText(obj);
            }

            protected override void DoSave(IConcurrencyControlContext context, string obj)
            {
                throw new NotImplementedException();
            }
        }
    }
}