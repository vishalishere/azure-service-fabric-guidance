namespace Tailspin.Web.Survey.Shared.Tests.Stores.Azure
{
    using Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.AzureStorage;
    using Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.Cache;
    using Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.ServiceBus;
    using Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.SqlAzure;
    using Microsoft.Practices.TransientFaultHandling;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Tailspin.Web.Survey.Shared.Stores.Azure;
    
    [TestClass]
    public class ConfiguredRetryPolicyFactoryFixture
    {
        [TestMethod]
        public void GetDefaultCacheRetryPolicyReturnsConfigured()
        {
            var policy = new ConfiguredRetryPolicyFactory().GetDefaultAzureCachingRetryPolicy();
            Assert.IsInstanceOfType(policy.ErrorDetectionStrategy, typeof(CacheTransientErrorDetectionStrategy));
            Assert.IsInstanceOfType(policy.RetryStrategy, typeof(FixedInterval));
        }

        [TestMethod]
        public void GetDefaultServiceBusRetryPolicyReturnsConfigured()
        {
            var policy = new ConfiguredRetryPolicyFactory().GetDefaultAzureServiceBusRetryPolicy();
            Assert.IsInstanceOfType(policy.ErrorDetectionStrategy, typeof(ServiceBusTransientErrorDetectionStrategy));
            Assert.IsInstanceOfType(policy.RetryStrategy, typeof(FixedInterval));
        }

        [TestMethod]
        public void GetDefaultStorageRetryPolicyReturnsConfigured()
        {
            var policy = new ConfiguredRetryPolicyFactory().GetDefaultAzureStorageRetryPolicy();
            Assert.IsInstanceOfType(policy.ErrorDetectionStrategy, typeof(StorageTransientErrorDetectionStrategy));
            Assert.IsInstanceOfType(policy.RetryStrategy, typeof(FixedInterval));
        }

        [TestMethod]
        public void GetDefaultSqlCommandRetryPolicyReturnsConfigured()
        {
            var policy = new ConfiguredRetryPolicyFactory().GetDefaultSqlCommandRetryPolicy();
            Assert.IsInstanceOfType(policy.ErrorDetectionStrategy, typeof(SqlAzureTransientErrorDetectionStrategy));
            Assert.IsInstanceOfType(policy.RetryStrategy, typeof(FixedInterval));
        }

        [TestMethod]
        public void GetDefaultSqlConnectionRetryPolicyReturnsConfigured()
        {
            var policy = new ConfiguredRetryPolicyFactory().GetDefaultSqlConnectionRetryPolicy();
            Assert.IsInstanceOfType(policy.ErrorDetectionStrategy, typeof(SqlAzureTransientErrorDetectionStrategy));
            Assert.IsInstanceOfType(policy.RetryStrategy, typeof(FixedInterval));
        }
    }
}
