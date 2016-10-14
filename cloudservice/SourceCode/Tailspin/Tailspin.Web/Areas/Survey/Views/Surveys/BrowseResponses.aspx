﻿<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<Tailspin.Web.Models.TenantPageViewData<Tailspin.Web.Areas.Survey.Models.BrowseResponseModel>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MenuContent" runat="server">
    <ul>
        <li><%:Html.ActionLink("New Survey", "New", "Surveys")%></li>
        <li class="current"><a>My Surveys</a></li>
        <li><%:Html.ActionLink("My Account", "Index", "Account", new {area = string.Empty}, null)%></li>
    </ul>
    <div class="clear">
    </div>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div id="pageTitle">
        <h2>
            <%:Html.ActionLink("Summary", "Analyze", "Surveys")%>
            <span>Browse Responses</span>
            <%:Html.ActionLink("Export Responses", "ExportResponses", "Surveys")%>
        </h2>
    </div>
    <div class="breadcrumbs">
        <%:Html.ActionLink("My Surveys", "Index", "Surveys")%> &gt;
        <%: this.Model.Title %> &gt;
        <%:Html.ActionLink("Analyze", "Analyze", "Surveys")%> &gt;
        Browse Reponses
    </div>

    <h2>
        Analyze:
        <%:this.Model.Title%>
    </h2>
    <div id="surveyForm">
        <div style="padding-top:20px;padding-bottom:20px;">
            <span style="padding-right:20em">
                <% if (Model.ContentModel.CanMoveBackward){ %>
                <%:Html.ActionLink("« Previous", "BrowseResponses", "Surveys", new { answerId = Model.ContentModel.PreviousAnswerId }, null)%>
                <% } else { %>
                Previous
                <% } %>
            </span>
            <span>
                <% if (Model.ContentModel.CanMoveForward){ %>
                <%:Html.ActionLink("Next »", "BrowseResponses", "Surveys", new { answerId = Model.ContentModel.NextAnswerId }, null)%>
                <% } else { %>
                Next
                <% } %>
            </span>
        </div>
        <% if (this.Model.ContentModel.SurveyAnswer != null) { %>
        <ol>
            <% for (int i = 0; i < this.Model.ContentModel.SurveyAnswer.QuestionAnswers.Count; i++ ) { %>
            <li>
                <%: Html.DisplayFor(m => m.ContentModel.SurveyAnswer.QuestionAnswers[i], "Answer-" + Tailspin.Web.Survey.Public.Utility.QuestionTemplateFactory.Create(Model.ContentModel.SurveyAnswer.QuestionAnswers[i].QuestionType))%>
            </li>
            <% } %>
        </ol>
        <% } else { %>
        No responses to this survey yet.
        <% } %>
    </div>
    
    <script src="<%:Url.Content("~/Scripts/jquery-1.4.1.min.js")%>" language="javascript" type="text/javascript"></script>
    <script src="<%:Url.Content("~/Scripts/jquery.rating.js")%>" language="javascript" type="text/javascript"></script>
</asp:Content>
