namespace Tailspin.Web.Survey.Shared.Stores.Azure
{
    using Microsoft.Practices.TransientFaultHandling;

    public interface IRetryPolicyFactory
    {
        RetryPolicy GetDefaultAzureCachingRetryPolicy();

        RetryPolicy GetDefaultAzureServiceBusRetryPolicy();

        RetryPolicy GetDefaultAzureStorageRetryPolicy();

        RetryPolicy GetDefaultSqlCommandRetryPolicy();

        RetryPolicy GetDefaultSqlConnectionRetryPolicy();
    }
}
