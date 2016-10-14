namespace Tailspin.Web.Survey.Shared.DataExtensibility
{
    using System;
    using System.Linq;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.StorageClient;
    using Tailspin.Web.Survey.Shared.Stores.AzureStorage;

    public class UDFAzureTable : IUDFAzureTable
    {
        private readonly CloudStorageAccount account;
        private readonly string tableName;

        public UDFAzureTable(CloudStorageAccount account, string tableName)
        {
            this.account = account;
            this.tableName = tableName;
        }

        public IQueryable<TableServiceEntity> BuildQueryFor(Type entityType)
        {
            var azureTableType = typeof(AzureTable<>).MakeGenericType(new Type[] { entityType });
            var azureTableInstance = Activator.CreateInstance(azureTableType, this.account, this.tableName);
            return azureTableType
                .GetProperty("Query")
                .GetValue(azureTableInstance, null) as IQueryable<TableServiceEntity>;
        }

        public void EnsureExist()
        {
            new AzureTable<TableServiceEntity>(this.account, this.tableName).EnsureExist();
        }

        public void Delete(TableServiceEntity entity)
        {
            new AzureTable<TableServiceEntity>(this.account, this.tableName).Delete(entity);
        }
        
        public void Save(TableServiceEntity entity)
        {
            new AzureTable<TableServiceEntity>(this.account, this.tableName).Add(entity);
        }
    }
}
