﻿namespace Tailspin.Web.Survey.Shared.Models
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web.Script.Serialization;
    using Tailspin.Web.Survey.Shared.Models.WordCloud;

    public class SurveyAnswersSummary
    {
        public SurveyAnswersSummary()
            : this(string.Empty, string.Empty)
        {
        }

        public SurveyAnswersSummary(string tenant, string slugName)
        {
            this.Tenant = tenant;
            this.SlugName = slugName;
            this.QuestionAnswersSummaries = new List<QuestionAnswersSummary>();
        }

        public string Tenant { get; set; }

        public string SlugName { get; set; }

        public int TotalAnswers { get; set; }

        public IList<QuestionAnswersSummary> QuestionAnswersSummaries { get; set; }

        public virtual void AddNewAnswer(SurveyAnswer surveyAnswer)
        {
            if (surveyAnswer == null)
            {
                return;
            }

            var newSurveyAnswersSummary = new SurveyAnswersSummary(surveyAnswer.Tenant, surveyAnswer.SlugName) { TotalAnswers = 1 };

            foreach (var questionAnswer in surveyAnswer.QuestionAnswers)
            {
                var questionAnswersSummary = new QuestionAnswersSummary
                {
                    QuestionType = questionAnswer.QuestionType,
                    QuestionText = questionAnswer.QuestionText,
                    PossibleAnswers = questionAnswer.PossibleAnswers
                };

                newSurveyAnswersSummary.QuestionAnswersSummaries.Add(questionAnswersSummary);

                switch (questionAnswer.QuestionType)
                {
                    case QuestionType.SimpleText:
                        var words = questionAnswer.Answer.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        var wordCounts = new Dictionary<string, int>();
                        var stemmedWords =
                            words.Where(w => !WordCloudFiltering.IsStopWord(w))
                                 .Select(w => Regex.Replace(w, @"[^a-z0-9]", string.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                                 .Where(w => !WordCloudFiltering.IsNumber(w));
                        stemmedWords.GroupBy(w => w, w => w).ToList()
                                    .ForEach(g => wordCounts.Add(g.Key, g.Count()));
                        questionAnswersSummary.AnswersSummary = new JavaScriptSerializer().Serialize(wordCounts);
                        break;

                    case QuestionType.MultipleChoice:
                        var choiceCounts = new Dictionary<string, int> { { questionAnswer.Answer, 1 } };
                        questionAnswersSummary.AnswersSummary = new JavaScriptSerializer().Serialize(choiceCounts);
                        break;

                    case QuestionType.FiveStars:
                        var answer = double.Parse(questionAnswer.Answer);
                        questionAnswersSummary.AnswersSummary = answer.ToString("F", CultureInfo.InvariantCulture);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            this.MergeWith(newSurveyAnswersSummary);
        }

        public virtual void MergeWith(SurveyAnswersSummary surveyAnswersSummary)
        {
            if (surveyAnswersSummary == null)
            {
                return;
            }

            if (string.Compare(this.Tenant, surveyAnswersSummary.Tenant) != 0)
            {
                throw new ArgumentException("The tenant of the surveyAnswersSummary does not match the tenant of this survey.");
            }

            if (string.Compare(this.SlugName, surveyAnswersSummary.SlugName) != 0)
            {
                throw new ArgumentException("The slug name of the surveyAnswersSummary does not match the slug name of this survey.");
            }

            if (this.TotalAnswers == 0)
            {
                this.QuestionAnswersSummaries = surveyAnswersSummary.QuestionAnswersSummaries;
                this.TotalAnswers = surveyAnswersSummary.TotalAnswers;
                return;
            }

            foreach (var newQuestionAnswersSummary in surveyAnswersSummary.QuestionAnswersSummaries)
            {
                QuestionAnswersSummary currentAnswer = newQuestionAnswersSummary;
                var existingQuestionAnswersSummary = this.QuestionAnswersSummaries.Single(s => s.QuestionText == currentAnswer.QuestionText);

                switch (existingQuestionAnswersSummary.QuestionType)
                {
                    case QuestionType.SimpleText:
                    case QuestionType.MultipleChoice:
                        var serializer = new JavaScriptSerializer();
                        var existingChoiceCounts = serializer.Deserialize<Dictionary<string, int>>(existingQuestionAnswersSummary.AnswersSummary);
                        var newChoiceCounts = serializer.Deserialize<Dictionary<string, int>>(newQuestionAnswersSummary.AnswersSummary);
                        var allChoices = existingChoiceCounts.Keys.Union(newChoiceCounts.Keys);
                        var mergedChoicesCounts = new Dictionary<string, int>(allChoices.Count());
                        foreach (var choice in allChoices)
                        {
                            int existingChoiceCount = 0;
                            if (existingChoiceCounts.ContainsKey(choice))
                            {
                                existingChoiceCount = existingChoiceCounts[choice];
                            }

                            int newChoiceCount = 0;
                            if (newChoiceCounts.ContainsKey(choice))
                            {
                                newChoiceCount = newChoiceCounts[choice];
                            }

                            mergedChoicesCounts.Add(choice, existingChoiceCount + newChoiceCount);
                        }

                        existingQuestionAnswersSummary.AnswersSummary = serializer.Serialize(mergedChoicesCounts);
                        break;

                    case QuestionType.FiveStars:
                        var newAnswersRating = double.Parse(newQuestionAnswersSummary.AnswersSummary, CultureInfo.InvariantCulture);
                        var existingAnswersRating = double.Parse(existingQuestionAnswersSummary.AnswersSummary, CultureInfo.InvariantCulture);
                        var updatedRatingAverage = ((newAnswersRating * surveyAnswersSummary.TotalAnswers) + (existingAnswersRating * this.TotalAnswers)) / (surveyAnswersSummary.TotalAnswers + this.TotalAnswers);
                        existingQuestionAnswersSummary.AnswersSummary = updatedRatingAverage.ToString("F", CultureInfo.InvariantCulture);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            this.TotalAnswers += surveyAnswersSummary.TotalAnswers;
        }
    }
}