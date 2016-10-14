<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<Tailspin.Web.Models.TenantPageViewData<IEnumerable<string>>>" %>

<%@ Import Namespace="Tailspin.Web.Utility" %>

<asp:Content ID="Content2" ContentPlaceHolderID="Head" runat="server">
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MenuContent" runat="server">
    <ul>
        <li class="current"><a>Subscriber's list</a></li>
        <li><%:Html.ActionLink("Add a new subscriber", "New", "Management")%></li>
    </ul>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <table border="0" cellspacing="0" cellpadding="0" class="tableGrid">
        <tr>
            <th>
                Tenant name
            </th>
            <th>
                Details
            </th>
            <th>
                Delete
            </th>
        </tr>
        <% foreach (var tenant in this.Model.ContentModel) %>
        <% { %>
        <tr>
            <td>
                <%=tenant.Capitalize()%>
            </td>
            <td>
                <%:Html.ActionLink("Details", "Detail", "Management", new { tenant = tenant }, new { })%>
            </td>
            <td>
                <a href="#" onclick="alert('This page is just a mockup, tenants cannot be deleted at Tailspin sample application.');">Delete</a>
            </td>
        </tr>
        <% } %>
    </table>
</asp:Content>
