namespace Tailspin.Web.Survey.Shared.Tests.Stores
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.StorageClient;
    using Moq;
    using Shared.Models;
    using Shared.Stores;
    using Shared.Stores.AzureStorage;

    [TestClass]
    public class TenantStoreFixture
    {
        [TestMethod]
        public void GetTenantCallsBlobStorageToRetrieveTenant()
        {
            var mockTenantBlobContainer = new Mock<IAzureBlobContainer<Tenant>>();
            var store = new TenantStore(mockTenantBlobContainer.Object, null);

            store.GetTenant("tenant");

            mockTenantBlobContainer.Verify(c => c.Get("tenant"), Times.Once());
        }

        [TestMethod]
        public void GetTenantReturnsTenantFromBlobStorage()
        {
            var mockTenantBlobContainer = new Mock<IAzureBlobContainer<Tenant>>();
            var store = new TenantStore(mockTenantBlobContainer.Object, null);
            var tenant = new Tenant();
            mockTenantBlobContainer.Setup(c => c.Get("tenant")).Returns(tenant);

            var actualTenant = store.GetTenant("tenant");

            Assert.AreSame(tenant, actualTenant);
        }

        [TestMethod]
        public void GetTenantNamesReturnsBlobNamesFromContainer()
        {
            var mockTenantBlobContainer = new Mock<IAzureBlobContainer<Tenant>>();
            var store = new TenantStore(mockTenantBlobContainer.Object, null);
            var blobs = new List<IListBlobItemWithName>() { new MockListBlobItem("b1"), new MockListBlobItem("b2") };
            mockTenantBlobContainer.Setup(c => c.GetBlobList()).Returns(blobs);

            var tenantNames = store.GetTenantNames().ToList();

            Assert.AreEqual(2, tenantNames.Count());
            CollectionAssert.Contains(tenantNames, "b1");
            CollectionAssert.Contains(tenantNames, "b2");
        }

        [TestMethod]
        public void InitializeEnsuresContainerExists()
        {
            var mockTenantBlobContainer = new Mock<IAzureBlobContainer<Tenant>>();
            var store = new TenantStore(mockTenantBlobContainer.Object, new Mock<IAzureBlobContainer<byte[]>>().Object);

            store.Initialize();

            mockTenantBlobContainer.Verify(c => c.EnsureExist(), Times.Once());
        }

        [TestMethod]
        public void UploadLogoSavesLogoToContainer()
        {
            var mockLogosBlobContainer = new Mock<IAzureBlobContainer<byte[]>>();
            var mockTenantContainer = new Mock<IAzureBlobContainer<Tenant>>();
            var store = new TenantStore(mockTenantContainer.Object, mockLogosBlobContainer.Object);
            mockTenantContainer.Setup(c => c.Get("tenant")).Returns(new Tenant() { Name = "tenant" });
            mockLogosBlobContainer.Setup(c => c.GetUri(It.IsAny<string>())).Returns(new Uri("http://bloburi"));
            var logo = new byte[1];

            store.UploadLogo("tenant", logo);

            mockLogosBlobContainer.Verify(c => c.Save("tenant", logo), Times.Once());
        }

        [TestMethod]
        public void UploadLogoGetsTenatToUpdateFromContainer()
        {
            var mockLogosBlobContainer = new Mock<IAzureBlobContainer<byte[]>>();
            var mockTenantContainer = new Mock<IAzureBlobContainer<Tenant>>();
            var store = new TenantStore(mockTenantContainer.Object, mockLogosBlobContainer.Object);
            mockTenantContainer.Setup(c => c.Get("tenant")).Returns(new Tenant() { Name = "tenant" }).Verifiable();
            mockLogosBlobContainer.Setup(c => c.GetUri(It.IsAny<string>())).Returns(new Uri("http://bloburi"));

            store.UploadLogo("tenant", new byte[1]);

            mockTenantContainer.Verify();
        }

        [TestMethod]
        public void UploadLogoSaveTenatWithLogoUrl()
        {
            var mockLogosBlobContainer = new Mock<IAzureBlobContainer<byte[]>>();
            var mockTenantContainer = new Mock<IAzureBlobContainer<Tenant>>();
            var store = new TenantStore(mockTenantContainer.Object, mockLogosBlobContainer.Object);
            mockTenantContainer.Setup(c => c.Get("tenant")).Returns(new Tenant() { Name = "tenant" });
            mockLogosBlobContainer.Setup(c => c.GetUri(It.IsAny<string>())).Returns(new Uri("http://bloburi/"));

            store.UploadLogo("tenant", new byte[1]);

            mockTenantContainer.Verify(c => c.Save("tenant", It.Is<Tenant>(t => t.Logo == "http://bloburi/")));
        }

        private class MockListBlobItem : IListBlobItemWithName
        {
            public MockListBlobItem(string name)
            {
                this.Name = name;
            }

            public string Name { get; set; }

            public CloudBlobContainer Container
            {
                get { throw new NotImplementedException(); }
            }

            public CloudBlobDirectory Parent
            {
                get { throw new NotImplementedException(); }
            }

            public Uri Uri
            {
                get { throw new NotImplementedException(); }
            }
        }
    }
}