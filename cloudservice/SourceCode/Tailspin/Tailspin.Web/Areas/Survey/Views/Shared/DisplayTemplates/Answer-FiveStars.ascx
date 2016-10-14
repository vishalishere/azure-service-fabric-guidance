﻿<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Tailspin.Web.Survey.Shared.Models.QuestionAnswer>" %>
<span class="questionText">
    <%:this.Model.QuestionText%>
</span>
<div class="answer">
    <span class="stars readonly">
        <% var rating = double.Parse(this.Model.Answer, System.Globalization.CultureInfo.InvariantCulture);  %>
        <label for="rating-1"><%:Html.RadioButton("rating-1", 1, rating > 0 && rating <= 1, new { disabled = "disabled" })%></label>
        <label for="rating-2"><%:Html.RadioButton("rating-2", 2, rating > 1 && rating <= 2, new { disabled = "disabled" })%></label>
        <label for="rating-3"><%:Html.RadioButton("rating-3", 3, rating > 2 && rating <= 3, new { disabled = "disabled" })%></label>
        <label for="rating-4"><%:Html.RadioButton("rating-4", 4, rating > 3 && rating <= 4, new { disabled = "disabled" })%></label>
        <label for="rating-5"><%:Html.RadioButton("rating-5", 5, rating > 4 && rating <= 5, new { disabled = "disabled" })%></label>
    </span>
</div>