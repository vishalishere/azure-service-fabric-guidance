﻿<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Tailspin.Web.Survey.Shared.Models.QuestionAnswer>" %>

<%:Html.RadioButtonFor(m => m.Answer, string.Empty, new { Checked = "checked", style="display: none;" } )%>
<% foreach (var possibleAnswer in this.Model.PossibleAnswers.Split('\n')) { %>
<div class="option">
    <%:Html.RadioButtonFor(m => m.Answer, possibleAnswer)%>
    <%:possibleAnswer %>
</div>
<% } %>