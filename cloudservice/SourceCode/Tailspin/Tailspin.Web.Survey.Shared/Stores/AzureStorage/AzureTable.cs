namespace Tailspin.Web.Survey.Shared.Stores.AzureStorage
{
    using System.Collections.Generic;
    using System.Data.Services.Client;
    using System.Linq;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.StorageClient;
    using Tailspin.Web.Survey.Shared.Helpers;

    public class AzureTable<T> : AzureStorageWithRetryPolicy, IAzureTable<T> where T : TableServiceEntity
    {
        private readonly string tableName;
        private readonly CloudStorageAccount account;

        public AzureTable(CloudStorageAccount account)
            : this(account, typeof(T).Name)
        {
        }

        public AzureTable(CloudStorageAccount account, string tableName)
        {
            this.tableName = tableName;
            this.account = account;
        }

        public CloudStorageAccount Account
        {
            get
            {
                return this.account;
            }
        }

        public IQueryable<T> Query
        {
            get
            {
                TableServiceContext context = this.CreateContext();
                return context.CreateQuery<T>(this.tableName).AsTableServiceQuery();
            }
        }
                
        public void EnsureExist()
        {
            this.StorageRetryPolicy.ExecuteAction(() =>
            {
                var cloudTableClient = new CloudTableClient(this.account.TableEndpoint.ToString(), this.account.Credentials);
                cloudTableClient.CreateTableIfNotExist(this.tableName);
            });
        }

        public void Add(T obj)
        {
          this.Add(new[] { obj });
        }

        public void Add(IEnumerable<T> objs)
        {
            TableServiceContext context = this.CreateContext();

            foreach (var obj in objs)
            {
                context.AddObject(this.tableName, obj);
            }

            var saveChangesOptions = SaveChangesOptions.None;
            if (objs.Distinct(new PartitionKeyComparer()).Count() == 1)
            {
                saveChangesOptions = SaveChangesOptions.Batch;
            }

            this.StorageRetryPolicy.ExecuteAction(() => context.SaveChanges(saveChangesOptions));
        }

        public void AddOrUpdate(T obj)
        {
            this.AddOrUpdate(new[] { obj });
        }

        public void AddOrUpdate(IEnumerable<T> objs)
        {
            foreach (var obj in objs)
            {
                var existingObj = default(T);
                var added = this.StorageRetryPolicy.ExecuteAction<bool>(() =>
                    {
                        var result = false;

                        try
                        {
                            existingObj = (from o in this.Query
                                           where o.PartitionKey == obj.PartitionKey
                                           && o.RowKey == obj.RowKey
                                           select o).SingleOrDefault();
                        }
                        catch (DataServiceQueryException ex)
                        {
                            var dataServiceClientException = ex.InnerException as DataServiceClientException;
                            if (dataServiceClientException != null)
                            {
                                if (dataServiceClientException.StatusCode == 404)
                                {
                                    TraceHelper.TraceWarning(ex.TraceInformation());

                                    this.Add(obj);
                                    result = true;
                                }
                                else
                                {
                                    TraceHelper.TraceError(ex.TraceInformation());
                                    throw;
                                }
                            }
                            else
                            {
                                TraceHelper.TraceError(ex.TraceInformation());
                                throw;
                            }
                        }

                        return result;
                    });

                if (added)
                {
                    return;
                }

                if (existingObj == null)
                {
                    this.Add(obj);
                }
                else
                {
                    TableServiceContext context = this.CreateContext();
                    context.AttachTo(this.tableName, obj, "*");
                    context.UpdateObject(obj);
                    this.StorageRetryPolicy.ExecuteAction(() => context.SaveChanges(SaveChangesOptions.ReplaceOnUpdate));
                }
            }
        }

        public void Delete(T obj)
        {
            this.Delete(new[] { obj });
        }

        public void Delete(IEnumerable<T> objs)
        {
            TableServiceContext context = this.CreateContext();
            foreach (var obj in objs)
            {
                context.AttachTo(this.tableName, obj, "*");
                context.DeleteObject(obj);
            }

            this.StorageRetryPolicy.ExecuteAction(() =>
                {
                    try
                    {
                        context.SaveChanges();
                    }
                    catch (DataServiceRequestException ex)
                    {
                        var dataServiceClientException = ex.InnerException as DataServiceClientException;
                        if (dataServiceClientException != null)
                        {
                            if (dataServiceClientException.StatusCode == 404)
                            {
                                TraceHelper.TraceWarning(ex.TraceInformation());
                                return;
                            }
                        }

                        TraceHelper.TraceError(ex.TraceInformation());

                        throw;
                    }
                });
        }

        private TableServiceContext CreateContext()
        {
            return new TableServiceContext(this.account.TableEndpoint.ToString(), this.account.Credentials)
            {
                // retry policy is handled by TFHAB
                RetryPolicy = RetryPolicies.NoRetry()
            };
        }

        private class PartitionKeyComparer : IEqualityComparer<TableServiceEntity>
        {
            public bool Equals(TableServiceEntity x, TableServiceEntity y)
            {
                return string.Compare(x.PartitionKey, y.PartitionKey, true, System.Globalization.CultureInfo.InvariantCulture) == 0;
            }

            public int GetHashCode(TableServiceEntity obj)
            {
                return obj.PartitionKey.GetHashCode();
            }
        }
    }
}