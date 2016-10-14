﻿namespace Tailspin.Web.Survey.Shared.Stores
{
    using System.Globalization;
    using Models;
    using Tailspin.Web.Survey.Shared.Stores.AzureStorage;

    public class SurveyAnswersSummaryStore : ISurveyAnswersSummaryStore
    {
        private readonly IAzureBlobContainer<SurveyAnswersSummary> surveyAnswersSummaryBlobContainer;

        public SurveyAnswersSummaryStore(IAzureBlobContainer<SurveyAnswersSummary> surveyAnswersSummaryBlobContainer)
        {
            this.surveyAnswersSummaryBlobContainer = surveyAnswersSummaryBlobContainer;
        }

        public void Initialize()
        {
            this.surveyAnswersSummaryBlobContainer.EnsureExist();
        }

        public SurveyAnswersSummary GetSurveyAnswersSummary(string tenant, string slugName)
        {
            var id = string.Format(CultureInfo.InvariantCulture, "{0}-{1}", tenant, slugName);
            return this.surveyAnswersSummaryBlobContainer.Get(id);
        }

        public void SaveSurveyAnswersSummary(SurveyAnswersSummary surveyAnswersSummary)
        {
            var id = string.Format(CultureInfo.InvariantCulture, "{0}-{1}", surveyAnswersSummary.Tenant, surveyAnswersSummary.SlugName);
            this.surveyAnswersSummaryBlobContainer.Save(id, surveyAnswersSummary);
        }

        public void MergeSurveyAnswersSummary(SurveyAnswersSummary partialSurveyAnswersSummary)
        {
            OptimisticConcurrencyContext context;
            var id = string.Format(CultureInfo.InvariantCulture, "{0}-{1}", partialSurveyAnswersSummary.Tenant, partialSurveyAnswersSummary.SlugName);
            var surveyAnswersSummaryInStore = this.surveyAnswersSummaryBlobContainer.Get(id, out context);
            partialSurveyAnswersSummary.MergeWith(surveyAnswersSummaryInStore);
            this.surveyAnswersSummaryBlobContainer.Save(context, partialSurveyAnswersSummary);
        }

        public void DeleteSurveyAnswersSummary(string tenant, string slugName)
        {
            var id = string.Format(CultureInfo.InvariantCulture, "{0}-{1}", tenant, slugName);
            this.surveyAnswersSummaryBlobContainer.Delete(id);
        }
    }
}