namespace Tailspin.Web.Survey.Shared.Stores.Azure
{
    using Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling;
    using Microsoft.Practices.TransientFaultHandling;

    public class ConfiguredRetryPolicyFactory : IRetryPolicyFactory
    {
        public RetryPolicy GetDefaultAzureCachingRetryPolicy()
        {
            return RetryPolicyFactory.GetDefaultAzureCachingRetryPolicy();
        }

        public RetryPolicy GetDefaultAzureServiceBusRetryPolicy()
        {
            return RetryPolicyFactory.GetDefaultAzureServiceBusRetryPolicy();
        }

        public RetryPolicy GetDefaultAzureStorageRetryPolicy()
        {
            return RetryPolicyFactory.GetDefaultAzureStorageRetryPolicy();
        }

        public RetryPolicy GetDefaultSqlCommandRetryPolicy()
        {
            return RetryPolicyFactory.GetDefaultSqlCommandRetryPolicy();
        }

        public RetryPolicy GetDefaultSqlConnectionRetryPolicy()
        {
            return RetryPolicyFactory.GetDefaultSqlConnectionRetryPolicy();
        }
    }
}
