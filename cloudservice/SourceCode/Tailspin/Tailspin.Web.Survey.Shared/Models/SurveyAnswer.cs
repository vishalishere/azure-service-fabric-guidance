﻿namespace Tailspin.Web.Survey.Shared.Models
{
    using System;
    using System.Collections.Generic;

    public class SurveyAnswer
    {
        public SurveyAnswer()
        {
            this.QuestionAnswers = new List<QuestionAnswer>();
        }

        public string SlugName { get; set; }

        public string Tenant { get; set; }

        public string Title { get; set; }

        public DateTime CreatedOn { get; set; }

        public IList<QuestionAnswer> QuestionAnswers { get; set; }
    }
}