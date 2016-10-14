﻿<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Tailspin.Web.Survey.Shared.Models.QuestionAnswer>" %>
<%:Html.RadioButtonFor(m => m.Answer, string.Empty, new { Checked = "checked", style="display: none;" } )%>
<span class="stars">
    <label for="rating-1"><%:Html.RadioButtonFor(m => m.Answer, 1)%></label>
    <label for="rating-2"><%:Html.RadioButtonFor(m => m.Answer, 2)%></label>
    <label for="rating-3"><%:Html.RadioButtonFor(m => m.Answer, 3)%></label>
    <label for="rating-4"><%:Html.RadioButtonFor(m => m.Answer, 4)%></label>
    <label for="rating-5"><%:Html.RadioButtonFor(m => m.Answer, 5)%></label>
</span>
