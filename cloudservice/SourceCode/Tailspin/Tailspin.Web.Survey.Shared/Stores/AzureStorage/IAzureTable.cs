namespace Tailspin.Web.Survey.Shared.Stores.AzureStorage
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.StorageClient;
    using Tailspin.Web.Survey.Shared.Stores.Azure;

    public interface IAzureTable<T> : IAzureObjectWithRetryPolicyFactory where T : TableServiceEntity
    {
        IQueryable<T> Query { get; }
        CloudStorageAccount Account { get; }

        void EnsureExist();
        void Add(T obj);
        void Add(IEnumerable<T> objs);
        void AddOrUpdate(T obj);
        void AddOrUpdate(IEnumerable<T> objs);
        void Delete(T obj);
        void Delete(IEnumerable<T> objs);
    }
}