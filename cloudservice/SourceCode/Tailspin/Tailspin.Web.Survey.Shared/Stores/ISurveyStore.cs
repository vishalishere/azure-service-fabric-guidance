namespace Tailspin.Web.Survey.Shared.Stores
{
    using System;
    using System.Collections.Generic;
    using Tailspin.Web.Survey.Extensibility;
    using Tailspin.Web.Survey.Shared.Models;

    public interface ISurveyStore
    {
        void Initialize();

        string GetStorageKeyFor(Survey survey);

        void SaveSurvey(Survey survey);
        void DeleteSurveyByTenantAndSlugName(string tenant, string slugName);
        Survey GetSurveyByTenantAndSlugName(string tenant, string slugName, bool getQuestions);
        IEnumerable<Survey> GetSurveysByTenant(string tenant);
        IEnumerable<Survey> GetRecentSurveys();

        void SaveSurveyExtension(Survey survey, IModelExtension extension);
        void DeleteSurveyExtensionByTenantAndSlugName(string tenant, string slugName);
        IModelExtension GetSurveyExtensionByTenantAndSlugName(string tenant, string slugName, Type extensionType);
        IEnumerable<IModelExtension> GetSurveyExtensionsByTenant(string tenant, Type extensionType);
    }
}