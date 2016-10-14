namespace Tailspin.Web.Survey.Shared.Stores.AzureStorage
{
    using System;
    using Microsoft.Practices.TransientFaultHandling;
    using Tailspin.Web.Survey.Shared.Stores.Azure;

    public abstract class AzureStorageWithRetryPolicy : AzureObjectWithRetryPolicyFactory
    {
        protected RetryPolicy StorageRetryPolicy
        {
            get
            {
                var retryPolicy = this.GetRetryPolicyFactoryInstance().GetDefaultAzureStorageRetryPolicy();                    
                retryPolicy.Retrying += new EventHandler<RetryingEventArgs>(RetryPolicyTrace);
                return retryPolicy;
            }
        }
    }
}
