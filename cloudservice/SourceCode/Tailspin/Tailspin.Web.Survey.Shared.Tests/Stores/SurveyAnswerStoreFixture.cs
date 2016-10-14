namespace Tailspin.Web.Survey.Shared.Tests.Stores
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using QueueMessages;
    using Shared.Stores;
    using Shared.Stores.AzureStorage;
    using Tailspin.Web.Survey.Shared.Models;

    [TestClass]
    public class SurveyAnswerStoreFixture
    {
        [TestMethod]
        public void SaveSurveyAnswerCreatesBlobContainerForGivenTenantAndSurvey()
        {
            var mockTenantStore = new Mock<ITenantStore>();
            var mockSurveyContainerFactory = new Mock<ISurveyAnswerContainerFactory>();
            var store = new SurveyAnswerStore(
                mockTenantStore.Object,
                mockSurveyContainerFactory.Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object,
                null,
                new Mock<IAzureBlobContainer<List<string>>>().Object);

            mockTenantStore.Setup(t => t.GetTenant("tenant")).Returns(new Tenant());
            mockSurveyContainerFactory.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<string>()))
                                      .Returns(new Mock<IAzureBlobContainer<SurveyAnswer>>().Object);

            var surveyAnswer = new SurveyAnswer { Tenant = "tenant", SlugName = "slug-name" };
            store.SaveSurveyAnswer(surveyAnswer);

            mockTenantStore.Verify(t => t.GetTenant("tenant"));
            mockSurveyContainerFactory.Verify(f => f.Create("tenant", "slug-name"));
        }

        [TestMethod]
        public void SaveSurveyAnswerEnsuresBlobContainerExists()
        {
            var mockSurveyAnswerBlobContainer = new Mock<IAzureBlobContainer<SurveyAnswer>>();
            var mockTenantStore = new Mock<ITenantStore>();
            var mockSurveyContainerFactory = new Mock<ISurveyAnswerContainerFactory>();
            var store = new SurveyAnswerStore(
                mockTenantStore.Object,
                mockSurveyContainerFactory.Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object,
                null,
                new Mock<IAzureBlobContainer<List<string>>>().Object);

            mockTenantStore.Setup(t => t.GetTenant(It.IsAny<string>())).Returns(new Tenant());
            mockSurveyContainerFactory.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<string>()))
                                      .Returns(mockSurveyAnswerBlobContainer.Object);

            store.SaveSurveyAnswer(new SurveyAnswer());

            mockTenantStore.Verify(t => t.GetTenant(It.IsAny<string>()));
            mockSurveyAnswerBlobContainer.Verify(c => c.EnsureExist());
        }

        [TestMethod]
        public void SaveSurveyAnswerSavesAnswerInBlobContainer()
        {
            var mockSurveyAnswerBlobContainer = new Mock<IAzureBlobContainer<SurveyAnswer>>();
            var mockTenantStore = new Mock<ITenantStore>();
            var mockSurveyContainerFactory = new Mock<ISurveyAnswerContainerFactory>();
            var store = new SurveyAnswerStore(
                mockTenantStore.Object,
                mockSurveyContainerFactory.Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object,
                null,
                new Mock<IAzureBlobContainer<List<string>>>().Object);

            mockTenantStore.Setup(t => t.GetTenant(It.IsAny<string>())).Returns(new Tenant());
            mockSurveyContainerFactory.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<string>()))
                                      .Returns(mockSurveyAnswerBlobContainer.Object);

            var surveyAnswer = new SurveyAnswer();
            store.SaveSurveyAnswer(surveyAnswer);

            mockTenantStore.Verify(t => t.GetTenant(It.IsAny<string>()));
            mockSurveyAnswerBlobContainer.Verify(c => c.Save(It.IsAny<string>(), It.Is<SurveyAnswer>(a => a == surveyAnswer)));
        }

        [TestMethod]
        public void SaveSurveyAnswerSavesAnswerInBlobContainerWithId()
        {
            var mockSurveyAnswerBlobContainer = new Mock<IAzureBlobContainer<SurveyAnswer>>();
            var mockTenantStore = new Mock<ITenantStore>();
            var mockSurveyContainerFactory = new Mock<ISurveyAnswerContainerFactory>();
            var store = new SurveyAnswerStore(
                mockTenantStore.Object,
                mockSurveyContainerFactory.Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object,
                null,
                new Mock<IAzureBlobContainer<List<string>>>().Object);

            mockTenantStore.Setup(t => t.GetTenant(It.IsAny<string>())).Returns(new Tenant());
            mockSurveyContainerFactory.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<string>()))
                                      .Returns(mockSurveyAnswerBlobContainer.Object);

            store.SaveSurveyAnswer(new SurveyAnswer());

            mockTenantStore.Verify(t => t.GetTenant(It.IsAny<string>()));
            mockSurveyAnswerBlobContainer.Verify(c => c.Save(It.Is<string>(s => s.Length == 19), It.IsAny<SurveyAnswer>()));
        }

        [TestMethod]
        public void SaveSurveyAnswerAddMessageToQueueWithSavedBlobId()
        {
            var mockTenantStore = new Mock<ITenantStore>();
            var mockSurveyAnswerBlobContainer = new Mock<IAzureBlobContainer<SurveyAnswer>>();
            var mockSurveyContainerFactory = new Mock<ISurveyAnswerContainerFactory>();
            var mockSurveyAnswerStoredQueue = new Mock<IAzureQueue<SurveyAnswerStoredMessage>>();
            var store = new SurveyAnswerStore(
                mockTenantStore.Object,
                mockSurveyContainerFactory.Object, 
                mockSurveyAnswerStoredQueue.Object,
                null,
                new Mock<IAzureBlobContainer<List<string>>>().Object);

            mockTenantStore.Setup(t => t.GetTenant(It.IsAny<string>())).Returns(new Tenant());
            mockSurveyContainerFactory.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(mockSurveyAnswerBlobContainer.Object);            
            string blobId = string.Empty;
            mockSurveyAnswerBlobContainer.Setup(c => c.Save(It.IsAny<string>(), It.IsAny<SurveyAnswer>()))
                .Callback((string id, SurveyAnswer sa) => blobId = id);

            store.SaveSurveyAnswer(new SurveyAnswer());

            mockTenantStore.Verify(t => t.GetTenant(It.IsAny<string>()));
            mockSurveyAnswerStoredQueue.Verify(
                q => q.AddMessage(
                    It.Is<SurveyAnswerStoredMessage>(m => m.SurveyAnswerBlobId == blobId)));
        }

        [TestMethod]
        public void SaveSurveyAnswerAddMessageToQueueWithTenant()
        {
            var mockTenantStore = new Mock<ITenantStore>();
            var mockSurveyContainerFactory = new Mock<ISurveyAnswerContainerFactory>();
            var mockSurveyAnswerStoredQueue = new Mock<IAzureQueue<SurveyAnswerStoredMessage>>();
            var store = new SurveyAnswerStore(
                mockTenantStore.Object,
                mockSurveyContainerFactory.Object, 
                mockSurveyAnswerStoredQueue.Object, 
                null,
                new Mock<IAzureBlobContainer<List<string>>>().Object);

            mockTenantStore.Setup(t => t.GetTenant("tenant")).Returns(new Tenant());
            mockSurveyContainerFactory.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<string>()))
                                      .Returns(new Mock<IAzureBlobContainer<SurveyAnswer>>().Object);

            store.SaveSurveyAnswer(new SurveyAnswer { Tenant = "tenant" });

            mockTenantStore.Verify(t => t.GetTenant("tenant"));
            mockSurveyAnswerStoredQueue.Verify(
                q => q.AddMessage(
                    It.Is<SurveyAnswerStoredMessage>(m => m.Tenant == "tenant")));
        }

        [TestMethod]
        public void SaveSurveyAnswerAddMessageToQueueWithSurveySlugName()
        {
            var mockTenantStore = new Mock<ITenantStore>();
            var mockSurveyContainerFactory = new Mock<ISurveyAnswerContainerFactory>();
            var mockSurveyAnswerStoredQueue = new Mock<IAzureQueue<SurveyAnswerStoredMessage>>();
            var store = new SurveyAnswerStore(
                mockTenantStore.Object,
                mockSurveyContainerFactory.Object, 
                mockSurveyAnswerStoredQueue.Object, 
                null,
                new Mock<IAzureBlobContainer<List<string>>>().Object);

            mockTenantStore.Setup(t => t.GetTenant(It.IsAny<string>())).Returns(new Tenant());
            mockSurveyContainerFactory.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<string>()))
                                      .Returns(new Mock<IAzureBlobContainer<SurveyAnswer>>().Object);

            store.SaveSurveyAnswer(new SurveyAnswer { SlugName = "slug-name" });

            mockTenantStore.Verify(t => t.GetTenant(It.IsAny<string>()));
            mockSurveyAnswerStoredQueue.Verify(
                q => q.AddMessage(
                    It.Is<SurveyAnswerStoredMessage>(m => m.SurveySlugName == "slug-name")));
        }

        [TestMethod]
        public void AppendSurveyAnswerIdToAnswersListGetTheAnswersListBlob()
        {
            var mockSurveyAnswerIdsListContainer = new Mock<IAzureBlobContainer<List<string>>>();
            var store = new SurveyAnswerStore(
                new Mock<ITenantStore>().Object,
                new Mock<ISurveyAnswerContainerFactory>().Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object,
                mockSurveyAnswerIdsListContainer.Object);

            store.AppendSurveyAnswerIdToAnswersList("tenant", "slug-name", string.Empty);

            var optContext = It.IsAny<OptimisticConcurrencyContext>();
            mockSurveyAnswerIdsListContainer.Verify(c => c.Get("tenant-slug-name", out optContext), Times.Once());
        }

        [TestMethod]
        public void AppendSurveyAnswerIdToAnswersListSavesModifiedListToTheAnswersListBlob()
        {
            var mockSurveyAnswerIdsListContainer = new Mock<IAzureBlobContainer<List<string>>>();
            var store = new SurveyAnswerStore(
                new Mock<ITenantStore>().Object,
                new Mock<ISurveyAnswerContainerFactory>().Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object, 
                mockSurveyAnswerIdsListContainer.Object);
           
            var answerIdsList = new List<string> { "id 1", "id 2" };
            var optContext = It.IsAny<OptimisticConcurrencyContext>();
            mockSurveyAnswerIdsListContainer.Setup(c => c.Get("tenant-slug-name", out optContext)).Returns(answerIdsList);
            List<string> savedList = null;
            mockSurveyAnswerIdsListContainer.Setup(c => c.Save(optContext, It.IsAny<List<string>>()))
                                            .Callback<IConcurrencyControlContext, List<string>>((id, l) => savedList = l);

            store.AppendSurveyAnswerIdToAnswersList("tenant", "slug-name", "new id");

            Assert.AreEqual(3, savedList.Count);
            Assert.AreEqual("new id", savedList.Last());
        }

        [TestMethod]
        public void AppendSurveyAnswerIdToAnswersListCreatesListWhenItDoesNotExistAndSavesIt()
        {
            var mockSurveyAnswerIdsListContainer = new Mock<IAzureBlobContainer<List<string>>>();
            var store = new SurveyAnswerStore(
                new Mock<ITenantStore>().Object,
                new Mock<ISurveyAnswerContainerFactory>().Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object,
                mockSurveyAnswerIdsListContainer.Object);
            
            List<string> answerIdsList = null;
            var optContext = It.IsAny<OptimisticConcurrencyContext>();
            mockSurveyAnswerIdsListContainer.Setup(c => c.Get("tenant-slug-name", out optContext)).Returns(answerIdsList);
            List<string> savedList = null;
            mockSurveyAnswerIdsListContainer.Setup(c => c.Save(optContext, It.IsAny<List<string>>()))
                                            .Callback<IConcurrencyControlContext, List<string>>((id, l) => savedList = l);

            store.AppendSurveyAnswerIdToAnswersList("tenant", "slug-name", "new id");

            Assert.AreEqual(1, savedList.Count);
            Assert.AreEqual("new id", savedList.Last());
        }

        [TestMethod]
        public void GetSurveyAnswerBrowsingContextGetTheAnswersListBlob()
        {
            var mockSurveyAnswerIdsListContainer = new Mock<IAzureBlobContainer<List<string>>>();
            var store = new SurveyAnswerStore(
                new Mock<ITenantStore>().Object,
                new Mock<ISurveyAnswerContainerFactory>().Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object,
                mockSurveyAnswerIdsListContainer.Object);

            store.GetSurveyAnswerBrowsingContext("tenant", "slug-name", string.Empty);

            mockSurveyAnswerIdsListContainer.Verify(c => c.Get("tenant-slug-name"), Times.Once());
        }

        [TestMethod]
        public void GetSurveyAnswerBrowsingContextReturnsPreviousId()
        {
            var mockSurveyAnswerIdsListContainer = new Mock<IAzureBlobContainer<List<string>>>();
            var store = new SurveyAnswerStore(
                new Mock<ITenantStore>().Object,
                new Mock<ISurveyAnswerContainerFactory>().Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object,
                mockSurveyAnswerIdsListContainer.Object);

            mockSurveyAnswerIdsListContainer.Setup(c => c.Get("tenant-slug-name"))
                                            .Returns(new List<string> { "id 1", "id 2", "id 3" });

            var surveyAnswerBrowsingContext = store.GetSurveyAnswerBrowsingContext("tenant", "slug-name", "id 2");

            Assert.AreEqual("id 1", surveyAnswerBrowsingContext.PreviousId);
        }

        [TestMethod]
        public void GetSurveyAnswerBrowsingContextReturnsNextId()
        {
            var mockSurveyAnswerIdsListContainer = new Mock<IAzureBlobContainer<List<string>>>();
            var store = new SurveyAnswerStore(
                new Mock<ITenantStore>().Object,
                new Mock<ISurveyAnswerContainerFactory>().Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object,
                mockSurveyAnswerIdsListContainer.Object);

            mockSurveyAnswerIdsListContainer.Setup(c => c.Get("tenant-slug-name"))
                                            .Returns(new List<string> { "id 1", "id 2", "id 3" });

            var surveyAnswerBrowsingContext = store.GetSurveyAnswerBrowsingContext("tenant", "slug-name", "id 2");

            Assert.AreEqual("id 3", surveyAnswerBrowsingContext.NextId);
        }

        [TestMethod]
        public void GetSurveyAnswerBrowsingContextReturnsNullNextIdAndPreviousIdWhenListDoesNotExist()
        {
            var mockSurveyAnswerIdsListContainer = new Mock<IAzureBlobContainer<List<string>>>();
            var store = new SurveyAnswerStore(
                new Mock<ITenantStore>().Object,
                new Mock<ISurveyAnswerContainerFactory>().Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object,
                mockSurveyAnswerIdsListContainer.Object);

            mockSurveyAnswerIdsListContainer.Setup(c => c.Get("tenant-slug-name"))
                                            .Returns(default(List<string>));

            var surveyAnswerBrowsingContext = store.GetSurveyAnswerBrowsingContext("tenant", "slug-name", "id");

            Assert.IsNull(surveyAnswerBrowsingContext.PreviousId);
            Assert.IsNull(surveyAnswerBrowsingContext.NextId);
        }

        [TestMethod]
        public void GetSurveyAnswerBrowsingContextReturnsNullNextIdWhenEndOfList()
        {
            var mockSurveyAnswerIdsListContainer = new Mock<IAzureBlobContainer<List<string>>>();
            var store = new SurveyAnswerStore(
                new Mock<ITenantStore>().Object,
                new Mock<ISurveyAnswerContainerFactory>().Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object,
                mockSurveyAnswerIdsListContainer.Object);

            mockSurveyAnswerIdsListContainer.Setup(c => c.Get("tenant-slug-name"))
                                            .Returns(new List<string> { "id 1" });

            var surveyAnswerBrowsingContext = store.GetSurveyAnswerBrowsingContext("tenant", "slug-name", "id 1");

            Assert.IsNull(surveyAnswerBrowsingContext.NextId);
        }

        [TestMethod]
        public void GetSurveyAnswerBrowsingContextReturnsNullPreviousIdWhenInBeginingOfList()
        {
            var mockSurveyAnswerIdsListContainer = new Mock<IAzureBlobContainer<List<string>>>();
            var store = new SurveyAnswerStore(
                new Mock<ITenantStore>().Object,
                new Mock<ISurveyAnswerContainerFactory>().Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object,
                mockSurveyAnswerIdsListContainer.Object);

            mockSurveyAnswerIdsListContainer.Setup(c => c.Get("tenant-slug-name"))
                                            .Returns(new List<string> { "id 1" });

            var surveyAnswerBrowsingContext = store.GetSurveyAnswerBrowsingContext("tenant", "slug-name", "id 1");

            Assert.IsNull(surveyAnswerBrowsingContext.PreviousId);
        }

        [TestMethod]
        public void GetFirstSurveyAnswerIdGetTheAnswersListBlob()
        {
            var mockSurveyAnswerIdsListContainer = new Mock<IAzureBlobContainer<List<string>>>();
            var store = new SurveyAnswerStore(
                new Mock<ITenantStore>().Object,
                new Mock<ISurveyAnswerContainerFactory>().Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object,
                mockSurveyAnswerIdsListContainer.Object);

            store.GetFirstSurveyAnswerId("tenant", "slug-name");

            mockSurveyAnswerIdsListContainer.Verify(c => c.Get("tenant-slug-name"), Times.Once());
        }

        [TestMethod]
        public void GetFirstSurveyAnswerIdReturnsTheAnswerWhichAppearsFirstOnTheListWhenListIsNotEmpty()
        {
            var mockSurveyAnswerIdsListContainer = new Mock<IAzureBlobContainer<List<string>>>();
            var store = new SurveyAnswerStore(
                new Mock<ITenantStore>().Object,
                new Mock<ISurveyAnswerContainerFactory>().Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object,
                mockSurveyAnswerIdsListContainer.Object);

            mockSurveyAnswerIdsListContainer.Setup(c => c.Get("tenant-slug-name"))
                                            .Returns(new List<string> { "id" });

            var id = store.GetFirstSurveyAnswerId("tenant", "slug-name");

            Assert.AreEqual("id", id);
        }

        [TestMethod]
        public void GetFirstSurveyAnswerIdReturnsEmprtyStringWhenListIsEmpty()
        {
            var mockSurveyAnswerIdsListContainer = new Mock<IAzureBlobContainer<List<string>>>();
            var store = new SurveyAnswerStore(
                new Mock<ITenantStore>().Object,
                new Mock<ISurveyAnswerContainerFactory>().Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object,
                mockSurveyAnswerIdsListContainer.Object);

            mockSurveyAnswerIdsListContainer.Setup(c => c.Get("tenant-slug-name"))
                                            .Returns(default(List<string>));

            var id = store.GetFirstSurveyAnswerId("tenant", "slug-name");

            Assert.AreEqual(string.Empty, id);
        }

        [TestMethod]
        public void GetSurveyAnswerCreatesBlobContainerForGivenTenantAndSurvey()
        {
            var mockSurveyContainerFactory = new Mock<ISurveyAnswerContainerFactory>();
            var store = new SurveyAnswerStore(
                new Mock<ITenantStore>().Object,
                mockSurveyContainerFactory.Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object, 
                new Mock<IAzureBlobContainer<List<string>>>().Object);

            mockSurveyContainerFactory.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<string>()))
                                      .Returns(new Mock<IAzureBlobContainer<SurveyAnswer>>().Object);

            store.GetSurveyAnswer("tenant", "slug-name", string.Empty);

            mockSurveyContainerFactory.Verify(f => f.Create("tenant", "slug-name"));
        }

        [TestMethod]
        public void GetSurveyAnswerEnsuresBlobContainerExists()
        {
            var mockSurveyAnswerBlobContainer = new Mock<IAzureBlobContainer<SurveyAnswer>>();
            var mockSurveyContainerFactory = new Mock<ISurveyAnswerContainerFactory>();
            var store = new SurveyAnswerStore(
                new Mock<ITenantStore>().Object,
                mockSurveyContainerFactory.Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object, 
                new Mock<IAzureBlobContainer<List<string>>>().Object);

            mockSurveyContainerFactory.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<string>()))
                                      .Returns(mockSurveyAnswerBlobContainer.Object);

            store.GetSurveyAnswer("tenant", "slug-name", string.Empty);

            mockSurveyAnswerBlobContainer.Verify(c => c.EnsureExist());
        }

        [TestMethod]
        public void GetSurveyAnswerGetsAnswerFromBlobContainer()
        {
            var mockSurveyAnswerBlobContainer = new Mock<IAzureBlobContainer<SurveyAnswer>>();
            var mockSurveyContainerFactory = new Mock<ISurveyAnswerContainerFactory>();
            var store = new SurveyAnswerStore(
                new Mock<ITenantStore>().Object,
                mockSurveyContainerFactory.Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object,
                new Mock<IAzureBlobContainer<List<string>>>().Object);

            mockSurveyContainerFactory.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<string>()))
                                      .Returns(mockSurveyAnswerBlobContainer.Object);

            store.GetSurveyAnswer("tenant", "slug-name", "id");

            mockSurveyAnswerBlobContainer.Verify(c => c.Get("id"));
        }

        [TestMethod]
        public void GetSurveyAnswerReturnsAnswerObtainedFromBlobContainer()
        {
            var mockSurveyAnswerBlobContainer = new Mock<IAzureBlobContainer<SurveyAnswer>>();
            var mockSurveyContainerFactory = new Mock<ISurveyAnswerContainerFactory>();
            var store = new SurveyAnswerStore(
                new Mock<ITenantStore>().Object,
                mockSurveyContainerFactory.Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object,
                new Mock<IAzureBlobContainer<List<string>>>().Object);

            mockSurveyContainerFactory.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<string>()))
                                      .Returns(mockSurveyAnswerBlobContainer.Object);
            var surveyAnswer = new SurveyAnswer();
            mockSurveyAnswerBlobContainer.Setup(c => c.Get(It.IsAny<string>()))
                                         .Returns(surveyAnswer);

            var actualSurveyAnswer = store.GetSurveyAnswer("tenant", "slug-name", string.Empty);

            Assert.AreSame(surveyAnswer, actualSurveyAnswer);
        }

        [TestMethod]
        public void GetSurveyAnswerIdsReturnsList()
        {
            var azureBlobContainerMock = new Mock<IAzureBlobContainer<List<string>>>();
            azureBlobContainerMock.Setup(b => b.Get("tenant-slug")).Returns(new List<string>() { "1", "2", "3" });
            
            var surveyStore = new SurveyAnswerStore(null, null, null, null, azureBlobContainerMock.Object);
            var answers = surveyStore.GetSurveyAnswerIds("tenant", "slug");

            Assert.AreEqual(3, answers.Count());

            azureBlobContainerMock.Verify(b => b.Get("tenant-slug"), Times.Once());
        }

        [TestMethod]
        public void DeleteSurveyAnswersCreatesBlobContainer()
        {
            var mockSurveyContainerFactory = new Mock<ISurveyAnswerContainerFactory>();
            var store = new SurveyAnswerStore(
                new Mock<ITenantStore>().Object,
                mockSurveyContainerFactory.Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object, 
                new Mock<IAzureBlobContainer<List<string>>>().Object);

            mockSurveyContainerFactory.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<string>()))
                                      .Returns(new Mock<IAzureBlobContainer<SurveyAnswer>>().Object);

            store.DeleteSurveyAnswers("tenant", "slug-name");

            mockSurveyContainerFactory.Verify(f => f.Create("tenant", "slug-name"), Times.Once());
        }

        [TestMethod]
        public void DeleteSurveyAnswersCallsDeleteFromBlobContainer()
        {
            var mockSurveyAnswerBlobContainer = new Mock<IAzureBlobContainer<SurveyAnswer>>();
            var mockSurveyContainerFactory = new Mock<ISurveyAnswerContainerFactory>();
            var store = new SurveyAnswerStore(
                new Mock<ITenantStore>().Object,
                mockSurveyContainerFactory.Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object, 
                new Mock<IAzureBlobContainer<List<string>>>().Object);

            mockSurveyContainerFactory.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<string>()))
                                      .Returns(mockSurveyAnswerBlobContainer.Object);

            store.DeleteSurveyAnswers("tenant", "slug-name");

            mockSurveyAnswerBlobContainer.Verify(c => c.DeleteContainer(), Times.Once());
        }

        [TestMethod]
        public void DeleteSurveyAnswersDeletesAnswersList()
        {
            var mockSurveyContainerFactory = new Mock<ISurveyAnswerContainerFactory>();
            var mockSurveyAnswerIdsListContainer = new Mock<IAzureBlobContainer<List<string>>>();
            var store = new SurveyAnswerStore(
                new Mock<ITenantStore>().Object,
                mockSurveyContainerFactory.Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object,
                new Mock<IAzureQueue<SurveyAnswerStoredMessage>>().Object,
                mockSurveyAnswerIdsListContainer.Object);

            mockSurveyContainerFactory.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<string>()))
                                      .Returns(new Mock<IAzureBlobContainer<SurveyAnswer>>().Object);

            store.DeleteSurveyAnswers("tenant", "slug-name");

            mockSurveyAnswerIdsListContainer.Verify(c => c.Delete("tenant-slug-name"), Times.Once());
        }
    }
}