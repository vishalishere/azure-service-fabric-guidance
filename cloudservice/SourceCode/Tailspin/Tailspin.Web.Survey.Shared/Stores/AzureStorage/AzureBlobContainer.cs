namespace Tailspin.Web.Survey.Shared.Stores.AzureStorage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.StorageClient;
    using Microsoft.WindowsAzure.StorageClient.Protocol;
    using Tailspin.Web.Survey.Shared.Helpers;

    public abstract class AzureBlobContainer<T> : AzureStorageWithRetryPolicy, IAzureBlobContainer<T>
    {
        protected const int BlobRequestTimeout = 120;

        protected readonly CloudBlobContainer Container;
        protected readonly CloudStorageAccount Account;
        
        public AzureBlobContainer(CloudStorageAccount account)
            : this(account, typeof(T).Name.ToLowerInvariant())
        {
        }

        public AzureBlobContainer(CloudStorageAccount account, string containerName)
        {
            this.Account = account;

            var client = account.CreateCloudBlobClient();

            // retry policy is handled by TFHAB
            client.RetryPolicy = RetryPolicies.NoRetry();

            this.Container = client.GetContainerReference(containerName);
        }

        public bool AcquireLock(PessimisticConcurrencyContext lockContext)
        {
            var request = BlobRequest.Lease(this.GetUri(lockContext.ObjectId), BlobRequestTimeout, LeaseAction.Acquire, null);
            this.Account.Credentials.SignRequest(request);

            // add extra headers not supported by SDK - not supported by emulator yet (SDK 1.7)
            ////request.Headers["x-ms-version"] = "2012-02-12";
            ////request.Headers.Add("x-ms-lease-duration", lockContext.Duration.TotalSeconds.ToString());

            try
            {
                using (var response = request.GetResponse())
                {
                    if (response is HttpWebResponse &&
                       HttpStatusCode.Created.Equals((response as HttpWebResponse).StatusCode))
                    {
                        lockContext.LockId = response.Headers["x-ms-lease-id"];
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (WebException e)
            {
                TraceHelper.TraceWarning("Warning acquiring blob '{0}' lease: {1}", lockContext.ObjectId, e.Message);
                if (WebExceptionStatus.ProtocolError.Equals(e.Status))
                {
                    if (e.Response is HttpWebResponse)
                    {
                        if (HttpStatusCode.NotFound.Equals((e.Response as HttpWebResponse).StatusCode))
                        {
                            lockContext.LockId = null;
                            return true;
                        }
                        else if (HttpStatusCode.Conflict.Equals((e.Response as HttpWebResponse).StatusCode))
                        {
                            lockContext.LockId = null;
                            return false;
                        }
                    }
                    throw;
                }
                return false;
            }
            catch (Exception e)
            {
                TraceHelper.TraceError("Error acquiring blob '{0}' lease: {1}", lockContext.ObjectId, e.Message);
                throw;
            }
        }

        public void ReleaseLock(PessimisticConcurrencyContext lockContext)
        {
            if (string.IsNullOrWhiteSpace(lockContext.LockId))
            {
                throw new ArgumentNullException("lockContext.LockId", "LockId cannot be null or empty");
            }

            var request = BlobRequest.Lease(this.GetUri(lockContext.ObjectId), BlobRequestTimeout, LeaseAction.Release, lockContext.LockId);
            this.Account.Credentials.SignRequest(request);

            using (var response = request.GetResponse())
            {
                if (response is HttpWebResponse &&
                    !HttpStatusCode.OK.Equals((response as HttpWebResponse).StatusCode))
                {
                    TraceHelper.TraceError("Error releasing blob '{0}' lease: {1}", lockContext.ObjectId, (response as HttpWebResponse).StatusDescription);
                    throw new InvalidOperationException((response as HttpWebResponse).StatusDescription);
                }
            }
        }

        public virtual void Delete(string objId)
        {
            this.StorageRetryPolicy.ExecuteAction(() =>
            {
                CloudBlob blob = this.Container.GetBlobReference(objId);
                blob.DeleteIfExists();
            });
        }

        public virtual void DeleteContainer()
        {
            this.StorageRetryPolicy.ExecuteAction(() =>
            {
                try
                {
                    this.Container.Delete();
                }
                catch (StorageClientException ex)
                {                    
                    if (ex.StatusCode == HttpStatusCode.NotFound)
                    {
                        TraceHelper.TraceWarning(ex.TraceInformation());
                        return;
                    }

                    TraceHelper.TraceError(ex.TraceInformation());

                    throw;
                }
            });
        }

        public virtual void EnsureExist()
        {
            this.StorageRetryPolicy.ExecuteAction(() => this.Container.CreateIfNotExist());
        }

        public virtual T Get(string objId)
        {
            OptimisticConcurrencyContext optimisticContext;
            return this.Get(objId, out optimisticContext);
        }

        public virtual T Get(string objId, out OptimisticConcurrencyContext context)
        {
            OptimisticConcurrencyContext optimisticContext = null;
            var result = this.StorageRetryPolicy.ExecuteAction<T>(() =>
                {
                    try
                    {
                        return this.DoGet(objId, out optimisticContext);
                    }
                    catch (StorageClientException ex)
                    {
                        TraceHelper.TraceWarning(ex.TraceInformation());
                        if (HttpStatusCode.NotFound.Equals(ex.StatusCode) &&
                            (StorageErrorCode.BlobNotFound.Equals(ex.ErrorCode) ||
                            StorageErrorCode.ResourceNotFound.Equals(ex.ErrorCode)))
                        {
                            optimisticContext = this.GetContextForUnexistentBlob(objId);
                            return default(T);
                        }
                        throw;
                    }
                });
            context = optimisticContext;
            return result;
        }

        public virtual IEnumerable<IListBlobItemWithName> GetBlobList()
        {
            return this.StorageRetryPolicy.ExecuteAction<IEnumerable<IListBlobItemWithName>>(() => this.Container.ListBlobs().Select(b => new AzureBlob(b as CloudBlob)));
        }

        public virtual Uri GetUri(string objId)
        {
            CloudBlob blob = this.Container.GetBlobReference(objId);
            return blob.Uri;
        }

        public virtual void Save(string objId, T obj)
        {
            this.StorageRetryPolicy.ExecuteAction(() => this.DoSave(objId, obj));
        }

        public virtual void Save(IConcurrencyControlContext context, T obj)
        {
            this.StorageRetryPolicy.ExecuteAction(() => this.DoSave(context, obj));
        }

        protected OptimisticConcurrencyContext GetContextForUnexistentBlob(string objId)
        {
            return new OptimisticConcurrencyContext()
            {
                ObjectId = objId,
                AccessCondition = AccessCondition.IfNotModifiedSince(DateTime.MinValue)
            };
        }
        
        protected abstract T DoGet(string objId, out OptimisticConcurrencyContext context);

        protected abstract void DoSave(string objId, T obj);

        protected abstract void DoSave(IConcurrencyControlContext context, T obj);
    }
}
