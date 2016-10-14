namespace Tailspin.Web.Survey.Shared.Stores.AzureStorage
{
    using System;
    using Microsoft.WindowsAzure.StorageClient;

    public abstract class AzureQueueMessage
    {
        [NonSerialized]
        private IUpdateableAzureQueue updateableQueueReference;

        [NonSerialized]
        private CloudQueueMessage messageReference;

        public CloudQueueMessage GetMessageReference()
        {
            return this.messageReference;
        }
        
        public IUpdateableAzureQueue GetUpdateableQueueReference()
        {
            return this.updateableQueueReference;
        }

        public void SetMessageReference(CloudQueueMessage reference)
        {
            this.messageReference = reference;
        }
        
        public void SetUpdateableQueueReference(IUpdateableAzureQueue reference)
        {
            this.updateableQueueReference = reference;
        }

        public void DeleteQueueMessage()
        {
            if (this.updateableQueueReference == null)
            {
                throw new InvalidOperationException("GetUpdateableQueueReference() cannot return null");
            }

            this.updateableQueueReference.DeleteMessage(this);
        }

        public void UpdateQueueMessage()
        {
            if (this.updateableQueueReference == null)
            {
                throw new InvalidOperationException("GetUpdateableQueueReference() cannot return null");
            }

            this.updateableQueueReference.UpdateMessage(this);
        }
    }
}