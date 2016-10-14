namespace Tailspin.Web.Survey.Shared.Stores
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Client;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.WindowsAzure.StorageClient;
    using Tailspin.Web.Survey.Extensibility;
    using Tailspin.Web.Survey.Shared.DataExtensibility;
    using Tailspin.Web.Survey.Shared.Helpers;
    using Tailspin.Web.Survey.Shared.Models;
    using Tailspin.Web.Survey.Shared.Stores.AzureStorage;

    public class SurveyStore : ISurveyStore
    {
        private readonly IAzureTable<SurveyRow> surveyTable;
        private readonly IAzureTable<QuestionRow> questionTable;
        private readonly IUDFAzureTable surveyTableExtension;

        public SurveyStore(
            IAzureTable<SurveyRow> surveyTable, 
            IAzureTable<QuestionRow> questionTable,
            IUDFAzureTable surveyTableExtension)
        {
            this.surveyTable = surveyTable;
            this.questionTable = questionTable;
            this.surveyTableExtension = surveyTableExtension;
            this.CacheEnabled = false;
        }

        public bool CacheEnabled { get; set; }

        public void Initialize()
        {
            this.surveyTable.EnsureExist();
            this.questionTable.EnsureExist();
            this.surveyTableExtension.EnsureExist();
        }

        public string GetStorageKeyFor(Survey survey)
        {
            var slugName = string.IsNullOrEmpty(survey.SlugName) ? GenerateSlug(survey.Title, 100) : survey.SlugName;
            return string.Format(CultureInfo.InvariantCulture, "{0}_{1}", survey.Tenant, slugName);
        }

        public void SaveSurvey(Survey survey)
        {
            if (string.IsNullOrEmpty(survey.SlugName) && string.IsNullOrEmpty(survey.Title))
            {
                throw new ArgumentNullException("survey", "The survey for saving has to have the slug or the title.");
            }

            var slugName = string.IsNullOrEmpty(survey.SlugName) ? GenerateSlug(survey.Title, 100) : survey.SlugName;

            var surveyRow = new SurveyRow
                                {
                                    SlugName = slugName,
                                    Title = survey.Title,
                                    CreatedOn = DateTime.UtcNow,
                                    PartitionKey = survey.Tenant
                                };

            surveyRow.RowKey = string.Format(CultureInfo.InvariantCulture, "{0}_{1}", survey.Tenant, surveyRow.SlugName);

            var questionRows = new List<QuestionRow>(survey.Questions.Count);
            for (int i = 0; i < survey.Questions.Count; i++)
            {
                var question = survey.Questions[i];
                var questionRow = new QuestionRow
                                      {
                                          Text = question.Text,
                                          Type = Enum.GetName(typeof(QuestionType), question.Type),
                                          PossibleAnswers = question.PossibleAnswers
                                      };

                questionRow.PartitionKey = surveyRow.RowKey;
                questionRow.RowKey = string.Format(CultureInfo.InvariantCulture, "{0}_{1}", DateTime.UtcNow.GetFormatedTicks(), i.ToString("D3"));

                questionRows.Add(questionRow);
            }

            //// First add the questions
            this.questionTable.Add(questionRows);

            try
            {
                //// Then add the survey
                //// If this fails, the questions may end up orphan but data will be consistent for the user
                this.surveyTable.Add(surveyRow);

                if (this.CacheEnabled)
                {
                    TenantCacheHelper.AddToCache(survey.Tenant, slugName, survey);
                }
            }
            catch (DataServiceRequestException ex)
            {
                TraceHelper.TraceError(ex.TraceInformation());

                var dataServiceClientException = ex.InnerException as DataServiceClientException;
                if (dataServiceClientException != null)
                {
                    if (dataServiceClientException.StatusCode == 409)
                    {
                        this.questionTable.Delete(questionRows);
                        throw;
                    }
                }

                throw;
            }
        }

        public void DeleteSurveyByTenantAndSlugName(string tenant, string slugName)
        {
            var surveyRow = GetSurveyRowByTenantAndSlugName(this.surveyTable, tenant, slugName);

            if (surveyRow == null)
            {
                return;
            }

            this.surveyTable.Delete(surveyRow);

            var surveyQuestionRows = GetSurveyQuestionRowsByTenantAndSlugName(this.questionTable, tenant, slugName);
            this.questionTable.Delete(surveyQuestionRows);

            if (this.CacheEnabled)
            {
                TenantCacheHelper.RemoveFromCache(tenant, slugName);
            }
        }

        public Survey GetSurveyByTenantAndSlugName(string tenant, string slugName, bool getQuestions)
        {
            Func<Survey> resolver = () =>
            {
                var surveyRow = GetSurveyRowByTenantAndSlugName(this.surveyTable, tenant, slugName);

                if (surveyRow == null)
                {
                    return null;
                }

                var survey = new Survey(surveyRow.SlugName)
                {
                    Tenant = surveyRow.PartitionKey,
                    Title = surveyRow.Title,
                    CreatedOn = surveyRow.CreatedOn
                };

                if (getQuestions)
                {
                    var surveyQuestionRows = GetSurveyQuestionRowsByTenantAndSlugName(this.questionTable, tenant, slugName);
                    foreach (var questionRow in surveyQuestionRows)
                    {
                        survey.Questions.Add(
                            new Question
                            {
                                Text = questionRow.Text,
                                Type = (QuestionType)Enum.Parse(typeof(QuestionType), questionRow.Type),
                                PossibleAnswers = questionRow.PossibleAnswers
                            });
                    }
                }

                return survey;
            };

            return this.CacheEnabled ? TenantCacheHelper.GetFromCache(tenant, slugName, resolver) : resolver();
        }

        public IEnumerable<Survey> GetSurveysByTenant(string tenant)
        {
            var query = from s in this.surveyTable.Query
                        where s.PartitionKey == tenant
                        select s;

            return this.surveyTable.GetRetryPolicyFactoryInstance().GetDefaultAzureStorageRetryPolicy().ExecuteAction<IEnumerable<Survey>>(() =>
                query.ToList().Select(surveyRow => new Survey(surveyRow.SlugName)
                {
                    Tenant = surveyRow.PartitionKey,
                    Title = surveyRow.Title,
                    CreatedOn = surveyRow.CreatedOn
                }));
        }

        public IEnumerable<Survey> GetRecentSurveys()
        {
            var query = (from s in this.surveyTable.Query
                         select s).Take(10);

            return this.surveyTable.GetRetryPolicyFactoryInstance().GetDefaultAzureStorageRetryPolicy().ExecuteAction<IEnumerable<Survey>>(() =>
                query.ToList().Select(surveyRow => new Survey(surveyRow.SlugName)
                {
                    Tenant = surveyRow.PartitionKey,
                    Title = surveyRow.Title
                }));
        }

        public void SaveSurveyExtension(Survey survey, IModelExtension extension)
        {
            if (string.IsNullOrEmpty(survey.SlugName) && string.IsNullOrEmpty(survey.Title))
            {
                throw new ArgumentNullException("survey", "The survey for saving has to have the slug or the title.");
            }

            var slugName = string.IsNullOrEmpty(survey.SlugName) ? GenerateSlug(survey.Title, 100) : survey.SlugName;

            if (!(extension is TableServiceEntity))
            {
                throw new ArgumentException("The extension should inherit from TableServiceEntity", "extension");
            }

            var extensionEntity = extension as TableServiceEntity;
            extensionEntity.PartitionKey = survey.Tenant;
            extensionEntity.RowKey = string.Format(CultureInfo.InvariantCulture, "{0}_{1}", survey.Tenant, slugName);

            this.surveyTableExtension.Save(extensionEntity);
        }

        public IModelExtension GetSurveyExtensionByTenantAndSlugName(string tenant, string slugName, Type extensionType)
        {
            return this.surveyTableExtension
                .BuildQueryFor(extensionType)
                .FirstOrDefault(e => e.PartitionKey.Equals(tenant) && e.RowKey.Equals(slugName)) as IModelExtension;
        }

        public void DeleteSurveyExtensionByTenantAndSlugName(string tenant, string slugName)
        {
            var entity = new DummyExtension
            {
                PartitionKey = tenant,
                RowKey = slugName
            };

            this.surveyTableExtension.Delete(entity);
        }

        public IEnumerable<IModelExtension> GetSurveyExtensionsByTenant(string tenant, Type extensionType)
        {
            return this.surveyTableExtension
                .BuildQueryFor(extensionType)
                .Where(e => e.PartitionKey.Equals(tenant))
                .ToList()
                .Select(e => e as IModelExtension);
        }

        private static SurveyRow GetSurveyRowByTenantAndSlugName(IAzureTable<SurveyRow> surveyTable, string tenant, string slugName)
        {
            var rowKey = string.Format(CultureInfo.InvariantCulture, "{0}_{1}", tenant, slugName);

            var query = from s in surveyTable.Query
                        where s.RowKey == rowKey
                        select s;

            return surveyTable.GetRetryPolicyFactoryInstance().GetDefaultAzureStorageRetryPolicy().ExecuteAction<SurveyRow>(() => query.SingleOrDefault());
        }

        private static IEnumerable<QuestionRow> GetSurveyQuestionRowsByTenantAndSlugName(IAzureTable<QuestionRow> questionTable, string tenant, string slugName)
        {
            var paritionKey = string.Format(CultureInfo.InvariantCulture, "{0}_{1}", tenant, slugName);

            var query = from q in questionTable.Query
                        where q.PartitionKey == paritionKey
                        select q;

            return questionTable.GetRetryPolicyFactoryInstance().GetDefaultAzureStorageRetryPolicy().ExecuteAction<IEnumerable<QuestionRow>>(() => query.ToList());
        }

        private static string GenerateSlug(string txt, int maxLength)
        {
            string str = RemoveAccent(txt).ToLower();

            str = Regex.Replace(str, @"[^a-z0-9\s-]", string.Empty);
            str = Regex.Replace(str, @"\s+", " ").Trim();
            str = str.Substring(0, str.Length <= maxLength ? str.Length : maxLength).Trim();
            str = Regex.Replace(str, @"\s", "-");

            return str;
        }

        private static string RemoveAccent(string txt)
        {
            byte[] bytes = System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(txt);
            return System.Text.Encoding.ASCII.GetString(bytes);
        }

        private class DummyExtension : TableServiceEntity { }
    }
}