namespace Tailspin.Web.Survey.Shared.DataExtensibility
{
    using System;
    using System.Linq;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.StorageClient;

    public interface IUDFAzureTable
    {
        IQueryable<TableServiceEntity> BuildQueryFor(Type entityType);
        void Delete(TableServiceEntity entity);
        void EnsureExist();
        void Save(TableServiceEntity entity);
    }
}
