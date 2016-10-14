﻿<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Tailspin.Web.Survey.Shared.Models.QuestionAnswersSummary>" %>
<span class="questionText">
    <%:this.Model.QuestionText%>
</span>
<div class="answer">
        <ul class="cloudList">
        <%  var summaryAll = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Dictionary<string, int>>(this.Model.AnswersSummary);
            var summaryKeysToDisplay = summaryAll.OrderByDescending(s => s.Value).Take(15).Select(s => s.Key);
            var summary = summaryAll.Where(s => summaryKeysToDisplay.Contains(s.Key)).ToDictionary(s => s.Key, s => s.Value);
            var wordCount = summary.Values.Sum();
            foreach (var key in summary.Keys) {
                double percent = (summary[key]  * 100) / wordCount;
                string wordClass;
                if      (percent < 10) wordClass = "cloudWord1";
                else if (percent < 20) wordClass = "cloudWord2";
                else if (percent < 30) wordClass = "cloudWord3";
                else if (percent < 40) wordClass = "cloudWord4";
                else if (percent < 50) wordClass = "cloudWord5";
                else if (percent < 60) wordClass = "cloudWord6";
                else if (percent < 70) wordClass = "cloudWord7";
                else if (percent < 80) wordClass = "cloudWord8";
                else if (percent < 90) wordClass = "cloudWord9";
                else                   wordClass = "cloudWord10";
        %>
            <li>
                <span class="<%:wordClass%>"><%: key %> (<%: summary[key] %>)</span>
            </li>
        <% } %>
        </ul>
</div>
