namespace Tailspin.Web.Survey.Shared.Stores.AzureStorage
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Web.Script.Serialization;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.StorageClient;
    using Microsoft.WindowsAzure.StorageClient.Protocol;
    using Tailspin.Web.Survey.Shared.Helpers;

    public class EntitiesBlobContainer<T> : AzureBlobContainer<T>
    {
        public EntitiesBlobContainer(CloudStorageAccount account)
            : base(account)
        {
        }

        public EntitiesBlobContainer(CloudStorageAccount account, string containerName)
            : base(account, containerName)
        {
        }

        protected override T DoGet(string objId, out OptimisticConcurrencyContext context)
        {
            CloudBlob blob = this.Container.GetBlobReference(objId);
            blob.FetchAttributes();
            context = new OptimisticConcurrencyContext(blob.Properties.ETag) { ObjectId = objId };
            return new JavaScriptSerializer().Deserialize<T>(blob.DownloadText());
        }

        protected override void DoSave(string objId, T obj)
        {
            OptimisticConcurrencyContext context = new OptimisticConcurrencyContext() { ObjectId = objId };
            this.DoSave(context, obj);
        }

        protected override void DoSave(IConcurrencyControlContext context, T obj)
        {
            if (string.IsNullOrWhiteSpace(context.ObjectId))
            {
                throw new ArgumentNullException("context.ObjectId", "ObjectId cannot be null or empty");
            }

            if (context is OptimisticConcurrencyContext)
            {
                CloudBlob blob = this.Container.GetBlobReference(context.ObjectId);
                blob.Properties.ContentType = "application/json";

                var blobRequestOptions = new BlobRequestOptions()
                {
                    AccessCondition = (context as OptimisticConcurrencyContext).AccessCondition
                };

                blob.UploadText(new JavaScriptSerializer().Serialize(obj), Encoding.Default, blobRequestOptions);
            }
            else if (context is PessimisticConcurrencyContext)
            {
                if (string.IsNullOrWhiteSpace((context as PessimisticConcurrencyContext).LockId))
                {
                    throw new ArgumentNullException("context.LockId", "LockId cannot be null or empty");
                }

                var blobProperties = new BlobProperties();
                blobProperties.ContentType = "application/json";

                var updateText = BlobRequest.Put(
                    this.GetUri(context.ObjectId),
                    BlobRequestTimeout,
                    blobProperties,
                    BlobType.BlockBlob,
                    (context as PessimisticConcurrencyContext).LockId,
                    0);
                using (var stream = new StreamWriter(updateText.GetRequestStream(), Encoding.Default))
                {
                    stream.Write(new JavaScriptSerializer().Serialize(obj));
                }
                this.Account.Credentials.SignRequest(updateText);

                using (var response = updateText.GetResponse())
                {
                    if (response is HttpWebResponse &&
                        !HttpStatusCode.Created.Equals((response as HttpWebResponse).StatusCode))
                    {
                        TraceHelper.TraceError("Error writing leased blob '{0}': {1}", context.ObjectId, (response as HttpWebResponse).StatusDescription);
                        throw new InvalidOperationException((response as HttpWebResponse).StatusDescription);
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("IConcurrencyControlContext implementation cannot be handled");
            }
        }
    }
}