﻿<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<Tailspin.Web.Models.TenantPageViewData<IEnumerable<Tailspin.Web.Areas.Survey.Models.SurveyModel>>>" %>

<%@ Import Namespace="Tailspin.Web.Utility" %>
<%@ Import Namespace="Tailspin.Web.Survey.Shared.Helpers" %>

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
    <div class="breadcrumbs">
        My Surveys
    </div>
    <% if (this.Model.ContentModel.Count() == 0) { %>
    <div class="noResults">
        <h3>There are no surveys.</h3>
    </div>
    <% } else { %>
    <%using (Html.BeginForm("Delete", "Surveys")) {%>
    <%:Html.AntiForgeryToken() %>
    <table border="0" cellspacing="0" cellpadding="0" class="tableGrid">
        <tr>
            <th>
                Title
            </th>
            <th>
                Created On
            </th>
            <%if (this.Model.ContentModel.First().Extensions != null)
              {
                  foreach (var extensionItem in this.Model.ContentModel.First().Extensions)
                  { %>
              <th>
              <%:extensionItem.PropertyName.SplitWords() %>
              </th>
            <%}
              }%>
            <th>
                Analyze
            </th>
            <th>
                Delete
            </th>
        </tr>
        <% foreach (var survey in this.Model.ContentModel) %>
        <% { %>
        <tr>
            <td>
                <%:Html.SurveyLink(survey.Title, this.ViewData["tenant"].ToString(), survey.SlugName)%>
            </td>
            <td>
                <%:survey.CreatedOn.ToLongDateString()%>
            </td>
            <%if (survey.Extensions != null)
              {
                  foreach (var extensionItem in survey.Extensions)
                  { %>
              <td>
              <%:extensionItem.PropertyValue%>
              </td>
            <%}
              }%>
            <td>
                <%:Html.ActionLink("Analyze", "Analyze", "Surveys", new { surveySlug = survey.SlugName }, new { })%>
            </td>
            <td>
                <a href="#" onclick="javascript: submitToDelete('<%:Url.Action("Delete", "Surveys", new { surveySlug = survey.SlugName })%>')">Delete</a>
            </td>
        </tr>
        <% } %>
    </table>
    <% } } %>

    <script type="text/javascript">
        function submitToDelete(url) {
            document.forms[0].action = url 
            document.forms[0].submit();
        }
    </script>
</asp:Content>
