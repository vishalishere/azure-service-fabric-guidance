namespace Tailspin.Web.AcceptanceTests.Stores.AzureStorage
{
    using System;
    using System.Linq;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Tailspin.Web.Survey.Shared.Helpers;
    using Tailspin.Web.Survey.Shared.Stores.AzureStorage;

    [TestClass]
    public class AzureQueueFixture
    {
        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            var account = CloudConfiguration.GetStorageAccount("DataConnectionString");
            var newSurveyAnswerQueue = new AzureQueue<MessageForTests>(account);
            newSurveyAnswerQueue.EnsureExist();
        }

        [TestMethod]
        public void AddAndGetMessage()
        {
            var account = CloudConfiguration.GetStorageAccount("DataConnectionString");
            var queue = new AzureQueue<MessageForTests>(account);
            var message = new MessageForTests { Content = "content" };

            queue.Clear();
            queue.AddMessage(message);
            var actualMessage = queue.GetMessage();

            Assert.AreEqual(message.Content, actualMessage.Content);
        }

        [TestMethod]
        public void PurgeAndGetMessageReturnNull()
        {
            var account = CloudConfiguration.GetStorageAccount("DataConnectionString");
            var queue = new AzureQueue<MessageForTests>(account);
            var message = new MessageForTests { Content = "content" };

            queue.AddMessage(message);
            queue.Clear();
            var actualMessage = queue.GetMessage();

            Assert.IsNull(actualMessage);
        }

        [TestMethod]
        public void AddAndGetMessages()
        {
            var account = CloudConfiguration.GetStorageAccount("DataConnectionString");
            var newSurveyAnswerQueue = new AzureQueue<MessageForTests>(account);
            int maxMessagesToReturn = 2;

            newSurveyAnswerQueue.AddMessage(new MessageForTests());
            newSurveyAnswerQueue.AddMessage(new MessageForTests());
            var actualMessages = newSurveyAnswerQueue.GetMessages(maxMessagesToReturn);

            Assert.AreEqual(2, actualMessages.Count());
        }

        [TestMethod]
        public void AddAndGetAndDeleteMessage()
        {
            var account = CloudConfiguration.GetStorageAccount("DataConnectionString");
            var queue = new AzureQueue<MessageForTests>(account);
            var message = new MessageForTests { Content = "content" };

            queue.Clear();
            queue.AddMessage(message);
            var addedMessage = queue.GetMessage();
            queue.DeleteMessage(addedMessage);
            var actualMessage = queue.GetMessage();

            Assert.IsNull(actualMessage);
        }

        [TestMethod]
        public void AddAndDeleteMessage()
        {
            var account = CloudConfiguration.GetStorageAccount("DataConnectionString");
            var queue = new AzureQueue<MessageForTests>(account);
            var message = new MessageForTests { Content = "content" };

            queue.Clear();
            queue.AddMessage(message);
            var retrievedMessage = queue.GetMessage();

            Assert.AreEqual(message.Content, retrievedMessage.Content);

            queue.DeleteMessage(retrievedMessage);

            Assert.IsNull(queue.GetMessage());
        }

        [TestMethod]
        public void GetAndUpdateMessage()
        {
            var account = CloudConfiguration.GetStorageAccount("DataConnectionString");
            var queue = new AzureQueue<MessageForTests>(account, "messagefortests", TimeSpan.FromSeconds(1));
            var message = new MessageForTests { Content = "content" };

            queue.Clear();
            queue.AddMessage(message);
            var retrievedMessage = queue.GetMessage();

            Assert.AreEqual("content", retrievedMessage.Content);

            retrievedMessage.Content = "newContent";
            queue.UpdateMessage(retrievedMessage);

            // wait 1 second
            Thread.Sleep(1000);

            retrievedMessage = queue.GetMessage();
            Assert.AreEqual("newContent", retrievedMessage.Content);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void UpdateMessageThrowIfTryToUpdateOtherMessageType()
        {
            var account = CloudConfiguration.GetStorageAccount("DataConnectionString");
            var queue = new AzureQueue<MessageForTests>(account, "messagefortests", TimeSpan.FromSeconds(1));
            queue.UpdateMessage(new OtherMessage());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void UpdateMessageThrowIfMessageRefIsNull()
        {
            var account = CloudConfiguration.GetStorageAccount("DataConnectionString");
            var queue = new AzureQueue<MessageForTests>(account, "messagefortests", TimeSpan.FromSeconds(1));
            queue.UpdateMessage(new MessageForTests());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DeleteMessageThrowIfMessageRefIsNull()
        {
            var account = CloudConfiguration.GetStorageAccount("DataConnectionString");
            var queue = new AzureQueue<MessageForTests>(account);
            var message = new MessageForTests { Content = "content" };

            queue.DeleteMessage(message);
        }

        private class MessageForTests : AzureQueueMessage
        {
            public string Content { get; set; }
        }

        private class OtherMessage : AzureQueueMessage { }
    }
}
