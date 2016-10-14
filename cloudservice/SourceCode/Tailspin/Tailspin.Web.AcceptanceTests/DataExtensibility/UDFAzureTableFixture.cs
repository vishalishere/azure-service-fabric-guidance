namespace Tailspin.Web.AcceptanceTests.DataExtensibility
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.StorageClient;
    using Tailspin.Web.Survey.Extensibility;
    using Tailspin.Web.Survey.Shared.DataExtensibility;
    using Tailspin.Web.Survey.Shared.Helpers;
    
    [TestClass]
    public class UDFAzureTableFixture
    {
        private const string TableName = "tableForTest";

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            var account = CloudConfiguration.GetStorageAccount("DataConnectionString");
            new UDFAzureTable(account, TableName).EnsureExist();
        }

        [TestMethod]
        public void ShouldSaveAndRetrieveCustomEntity()
        {
            var account = CloudConfiguration.GetStorageAccount("DataConnectionString");

            var key = Guid.NewGuid().ToString();
            var customEntity = new CustomEntity()
            {
                PartitionKey = "CustomEntity",
                RowKey = key,
                Id = 5,
                Name = "five"
            };

            var udfAzureTable = new UDFAzureTable(account, TableName);
            udfAzureTable.Save(customEntity);

            var storedEntity = udfAzureTable.BuildQueryFor(customEntity.GetType())
                .Where(e => e.PartitionKey.Equals("CustomEntity") && e.RowKey.Equals(key))
                .FirstOrDefault();

            Assert.IsNotNull(storedEntity);
            Assert.AreEqual(customEntity.ToString(), storedEntity.ToString());
        }

        private class CustomEntity : TableServiceEntity, IModelExtension
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public override string ToString()
            {
                return string.Format("Id: {0} - Name: {1}", this.Id, this.Name);
            }

            public bool IsChildOf(object parent)
            {
                throw new NotImplementedException();
            }
        }
    }
}
