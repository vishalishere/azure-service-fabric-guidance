namespace Tailspin.Web.AcceptanceTests.Stores.AzureStorage
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.StorageClient;
    using Moq;
    using Tailspin.Web.Survey.Shared.Stores.AzureStorage;

    [TestClass]
    public class AzureQueueMessageFixture
    {
        [TestMethod]
        public void SetAndGetMessageReference()
        {
            var cloudMessage = new CloudQueueMessage("dummy");
            var queueMessage = new TestAzureQueueMessage();
            queueMessage.SetMessageReference(cloudMessage);
            Assert.AreEqual(cloudMessage, queueMessage.GetMessageReference());
        }

        [TestMethod]
        public void SetAndGetUpdateableQueueReference()
        {
            var updateableQueue = new Mock<IUpdateableAzureQueue>();
            var queueMessage = new TestAzureQueueMessage();
            queueMessage.SetUpdateableQueueReference(updateableQueue.Object);
            Assert.AreEqual(updateableQueue.Object, queueMessage.GetUpdateableQueueReference());
        }

        [TestMethod]
        public void UpdateQueueMessageShouldCallIUpdateableAzureQueueUpdateMessage()
        {
            var updateableQueue = new Mock<IUpdateableAzureQueue>();
            var cloudMessage = new CloudQueueMessage("dummy");
            var queueMessage = new TestAzureQueueMessage();
            queueMessage.SetMessageReference(cloudMessage);
            queueMessage.SetUpdateableQueueReference(updateableQueue.Object);
            queueMessage.UpdateQueueMessage();

            updateableQueue.Verify(q => q.UpdateMessage(queueMessage), Times.Once());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UpdateQueueMessageShouldThrowIfIUpdateableAzureQueueIsNull()
        {
            var queueMessage = new TestAzureQueueMessage();
            queueMessage.UpdateQueueMessage();
        }

        private class TestAzureQueueMessage : AzureQueueMessage { }
    }
}
