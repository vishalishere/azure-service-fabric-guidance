namespace Tailspin.Web.Survey.Shared.Stores.AzureStorage
{
    using System;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.StorageClient;

    public class FilesBlobContainer : AzureBlobContainer<byte[]>
    {
        private readonly string contentType;

        public FilesBlobContainer(CloudStorageAccount account, string containerName, string contentType)
            : base(account, containerName)
        {
            this.contentType = contentType;
        }

        public override void EnsureExist()
        {
            this.StorageRetryPolicy.ExecuteAction(() =>
            {
                this.Container.CreateIfNotExist();
                this.Container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
            });
        }

        protected override byte[] DoGet(string objId, out OptimisticConcurrencyContext context)
        {
            CloudBlob blob = this.Container.GetBlobReference(objId);
            blob.FetchAttributes();
            context = new OptimisticConcurrencyContext(blob.Properties.ETag) { ObjectId = objId };
            return blob.DownloadByteArray();
        }

        protected override void DoSave(string objId, byte[] obj)
        {
            CloudBlob blob = this.Container.GetBlobReference(objId);
            blob.Properties.ContentType = this.contentType;
            blob.UploadByteArray(obj);
        }

        protected override void DoSave(IConcurrencyControlContext context, byte[] obj)
        {
            throw new NotImplementedException();
        }
    }
}