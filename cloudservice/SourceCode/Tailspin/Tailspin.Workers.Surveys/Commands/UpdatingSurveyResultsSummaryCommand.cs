namespace Tailspin.Workers.Surveys.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Tailspin.Web.Survey.Shared.Helpers;
    using Tailspin.Web.Survey.Shared.Models;
    using Tailspin.Web.Survey.Shared.Stores;
    using Tailspin.Web.Survey.Shared.Stores.AzureStorage;
    using Web.Survey.Shared.QueueMessages;

    public class UpdatingSurveyResultsSummaryCommand : IBatchCommand<SurveyAnswerStoredMessage>
    {
        private readonly IDictionary<string, TenantSurveyProcessingInfo> tenantSurveyProcessingInfoCache;
        private readonly ISurveyAnswerStore surveyAnswerStore;
        private readonly ISurveyAnswersSummaryStore surveyAnswersSummaryStore;

        public UpdatingSurveyResultsSummaryCommand(IDictionary<string, TenantSurveyProcessingInfo> processingInfoCache, ISurveyAnswerStore surveyAnswerStore, ISurveyAnswersSummaryStore surveyAnswersSummaryStore)
        {
            this.tenantSurveyProcessingInfoCache = processingInfoCache;
            this.surveyAnswerStore = surveyAnswerStore;
            this.surveyAnswersSummaryStore = surveyAnswersSummaryStore;
        }

        public void PreRun()
        {
            this.tenantSurveyProcessingInfoCache.Clear();
        }

        public bool Run(SurveyAnswerStoredMessage message)
        {
            if (!message.AppendedToAnswers)
            {
                this.surveyAnswerStore.AppendSurveyAnswerIdToAnswersList(
                                        message.Tenant,
                                        message.SurveySlugName,
                                        message.SurveyAnswerBlobId);
                message.AppendedToAnswers = true;
                message.UpdateQueueMessage();
            }

            var surveyAnswer = this.surveyAnswerStore.GetSurveyAnswer(
                                    message.Tenant,
                                    message.SurveySlugName,
                                    message.SurveyAnswerBlobId);

            var keyInCache = string.Format(CultureInfo.InvariantCulture, "{0}-{1}", message.Tenant, message.SurveySlugName);
            TenantSurveyProcessingInfo surveyInfo;

            if (!this.tenantSurveyProcessingInfoCache.ContainsKey(keyInCache))
            {
                surveyInfo = new TenantSurveyProcessingInfo(message.Tenant, message.SurveySlugName);
                this.tenantSurveyProcessingInfoCache[keyInCache] = surveyInfo;
            }
            else
            {
                surveyInfo = this.tenantSurveyProcessingInfoCache[keyInCache];
            }

            surveyInfo.AnswersSummary.AddNewAnswer(surveyAnswer);
            surveyInfo.AnswersMessages.Add(message);

            return false;   // won't remove the message from the queue
        }

        public void PostRun()
        {
            foreach (var surveyInfo in this.tenantSurveyProcessingInfoCache.Values)
            {
                try
                {
                    this.surveyAnswersSummaryStore.MergeSurveyAnswersSummary(surveyInfo.AnswersSummary);

                    foreach (var message in surveyInfo.AnswersMessages)
                    {
                        try
                        {
                            message.DeleteQueueMessage();
                        }
                        catch (Exception e)
                        {
                            TraceHelper.TraceWarning("Error deleting message for '{0-1}': {2}", message.Tenant, message.SurveySlugName, e.Message);
                        }
                    }
                }
                catch (Exception e)
                {
                    // do nothing - will leave the messages in the queue for reprocessing
                    TraceHelper.TraceWarning(e.Message);
                }
            }
        }
    }
}
