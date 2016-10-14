namespace Tailspin.Web.Survey.Shared.Stores
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using AzureStorage;
    using QueueMessages;
    using Tailspin.Web.Survey.Shared.Helpers;
    using Tailspin.Web.Survey.Shared.Models;

    public class SurveyAnswerStore : ISurveyAnswerStore
    {
        private readonly ITenantStore tenantStore;
        private readonly ISurveyAnswerContainerFactory surveyAnswerContainerFactory;
        private readonly IAzureQueue<SurveyAnswerStoredMessage> standardSurveyAnswerStoredQueue;
        private readonly IAzureQueue<SurveyAnswerStoredMessage> premiumSurveyAnswerStoredQueue;
        private readonly IAzureBlobContainer<List<string>> surveyAnswerIdsListContainer;

        public SurveyAnswerStore(
            ITenantStore tenantStore, 
            ISurveyAnswerContainerFactory surveyAnswerContainerFactory,
            IAzureQueue<SurveyAnswerStoredMessage> standardSurveyAnswerStoredQueue, 
            IAzureQueue<SurveyAnswerStoredMessage> premiumSurveyAnswerStoredQueue, 
            IAzureBlobContainer<List<string>> surveyAnswerIdsListContainer)
        {
            this.tenantStore = tenantStore;
            this.surveyAnswerContainerFactory = surveyAnswerContainerFactory;
            this.standardSurveyAnswerStoredQueue = standardSurveyAnswerStoredQueue;
            this.premiumSurveyAnswerStoredQueue = premiumSurveyAnswerStoredQueue;
            this.surveyAnswerIdsListContainer = surveyAnswerIdsListContainer;
        }
        
        public void Initialize()
        {
            this.surveyAnswerIdsListContainer.EnsureExist();
            this.premiumSurveyAnswerStoredQueue.EnsureExist();
            this.standardSurveyAnswerStoredQueue.EnsureExist();
        }

        public void SaveSurveyAnswer(SurveyAnswer surveyAnswer)
        {
            var tenant = this.tenantStore.GetTenant(surveyAnswer.Tenant);
            if (tenant != null)
            {
                var surveyAnswerBlobContainer = this.surveyAnswerContainerFactory.Create(surveyAnswer.Tenant, surveyAnswer.SlugName);
                surveyAnswerBlobContainer.EnsureExist();

                surveyAnswer.CreatedOn = DateTime.UtcNow;
                var blobId = surveyAnswer.CreatedOn.GetFormatedTicks();
                surveyAnswerBlobContainer.Save(blobId, surveyAnswer);

                (SubscriptionKind.Premium.Equals(tenant.SubscriptionKind)
                    ? this.premiumSurveyAnswerStoredQueue
                    : this.standardSurveyAnswerStoredQueue)
                    .AddMessage(new SurveyAnswerStoredMessage
                    {
                        SurveyAnswerBlobId = blobId,
                        Tenant = surveyAnswer.Tenant,
                        SurveySlugName = surveyAnswer.SlugName
                    });
            }
        }

        public SurveyAnswer GetSurveyAnswer(string tenant, string slugName, string surveyAnswerId)
        {
            var surveyBlobContainer = this.surveyAnswerContainerFactory.Create(tenant, slugName);
            surveyBlobContainer.EnsureExist();
            return surveyBlobContainer.Get(surveyAnswerId);
        }

        public string GetFirstSurveyAnswerId(string tenant, string slugName)
        {
            string id = string.Format(CultureInfo.InvariantCulture, "{0}-{1}", tenant, slugName);
            var answerIdList = this.surveyAnswerIdsListContainer.Get(id);

            if (answerIdList != null)
            {
                return answerIdList[0];
            }

            return string.Empty;
        }

        public void AppendSurveyAnswerIdToAnswersList(string tenant, string slugName, string surveyAnswerId)
        {
            OptimisticConcurrencyContext context;
            string id = string.Format(CultureInfo.InvariantCulture, "{0}-{1}", tenant, slugName);
            var answerIdList = this.surveyAnswerIdsListContainer.Get(id, out context) ?? new List<string>(1);
            answerIdList.Add(surveyAnswerId);
            this.surveyAnswerIdsListContainer.Save(context, answerIdList);
        }

        public SurveyAnswerBrowsingContext GetSurveyAnswerBrowsingContext(string tenant, string slugName, string answerId)
        {
            string id = string.Format(CultureInfo.InvariantCulture, "{0}-{1}", tenant, slugName);
            var answerIdsList = this.surveyAnswerIdsListContainer.Get(id);

            string previousId = null;
            string nextId = null;
            if (answerIdsList != null)
            {
                var currentAnswerIndex = answerIdsList.FindIndex(i => i == answerId);

                if (currentAnswerIndex - 1 >= 0)
                {
                    previousId = answerIdsList[currentAnswerIndex - 1];
                }

                if (currentAnswerIndex + 1 <= answerIdsList.Count - 1)
                {
                    nextId = answerIdsList[currentAnswerIndex + 1];
                }
            }

            return new SurveyAnswerBrowsingContext
                       {
                           PreviousId = previousId,
                           NextId = nextId
                       };
        }

        public IEnumerable<string> GetSurveyAnswerIds(string tenant, string slugName)
        {
            string id = string.Format(CultureInfo.InvariantCulture, "{0}-{1}", tenant, slugName);
            return this.surveyAnswerIdsListContainer.Get(id);
        }

        public void DeleteSurveyAnswers(string tenant, string slugName)
        {
            var surveyBlobContainer = this.surveyAnswerContainerFactory.Create(tenant, slugName);
            surveyBlobContainer.DeleteContainer();

            string id = string.Format(CultureInfo.InvariantCulture, "{0}-{1}", tenant, slugName);
            this.surveyAnswerIdsListContainer.Delete(id);
        }
    }
}