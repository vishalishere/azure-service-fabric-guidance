﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<Tailspin.Web.Models.TenantPageViewData<Tailspin.Web.Survey.Shared.Models.Tenant>>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="Head" runat="server">
    <script src="<%:Url.Content("~/Scripts/jquery-1.4.1.min.js")%>" language="javascript" type="text/javascript"></script>
    <script src="<%:Url.Content("~/Scripts/management.js")%>" language="javascript" type="text/javascript"></script>
    <%if (this.ViewData["error"] != null)
      {%>
    <script type="text/javascript">
        alert("<%=this.ViewData["error"].ToString()%>");
    </script>
    <%}%>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MenuContent" runat="server">
    <ul>
        <li><%:Html.ActionLink("Subscriber's list", "Index", "Management")%></li>
        <li class="current"><a>Add a new subscriber</a></li>
    </ul>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div id="issuerOptionTabs">
        <div class="sectionexplanationcontainer">
            <span class="titlesection">Tenant configuration</span> <span class="explanationsection">
                <div id="yourIssuerTab" class="issuerOptionTab">
                    <%using (Html.BeginForm())
                      {%>
                    <div class="sampleform">
                        <table class="configTable">
                            <tbody>
                                <tr>
                                    <td>
                                        Organization name:
                                    </td>
                                    <td>
                                        <%= Html.TextBox("Name", this.Model.ContentModel.Name, new { size = "55" })%>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        Geolocation of your Windows Azure host:
                                    </td>
                                    <td>
                                        <%= Html.DropDownList("HostGeoLocation", new List<SelectListItem>
                                                            {
                                                                new SelectListItem {Text = "Anywhere Asia", Value = "Anywhere Asia"},
                                                                new SelectListItem {Text = "Anywhere Europe", Value = "Anywhere Europe"},
                                                                new SelectListItem {Text = "Anywhere US", Value = "Anywhere US"},
                                                                new SelectListItem {Text = "East Asia", Value = "East Asia"},
                                                                new SelectListItem {Text = "North Central US", Value = "North Central US"},
                                                                new SelectListItem {Text = "North Europe", Value = "North Europe"},
                                                                new SelectListItem {Text = "South Central US", Value = "South Central US"},
                                                                new SelectListItem {Text = "Southeast Asia", Value = "Southeast Asia"},
                                                                new SelectListItem {Text = "West Europe", Value = "West Europe"},
                                                            })%>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        Subscription Type:
                                    </td>
                                    <td>
                                        <%= Html.DropDownList("SubscriptionKind", new List<SelectListItem>
                                                            {
                                                                new SelectListItem {Text = "Standard", Value = Tailspin.Web.Survey.Shared.Models.SubscriptionKind.Standard.ToString()},
                                                                new SelectListItem {Text = "Premium", Value = Tailspin.Web.Survey.Shared.Models.SubscriptionKind.Premium.ToString()}
                                                            }, new { @class = "subscriptionSelector" })%>
                                    </td>
                                </tr>
                                <tr class="standardConfigSelected" style="display: none;">
                                    <td></td>
                                    <td>
                                        <b>Note:</b> In a real application, you would have a registered members database
                                        for tenants who use Tailspin's authentication instead of their own and each tenant
                                        will be allowed to add/remove its authorized members from his account.
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        Welcome Text:
                                    </td>
                                    <td>
                                        <%: Html.TextBox("WelcomeText", this.Model.ContentModel.WelcomeText, new { size = "80" })%>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                        <div class="premiumConfig">
                            <u>The following data is optional if subscriber's subscription is <b>Premium</b> and want to use a custom STS (otherwise will use Tailspin's)</u>
                            <table class="configTable">
                                <tbody>
                                    <tr>
                                        <td>
                                            Identifier:
                                        </td>
                                        <td>
                                            <%: Html.TextBox("IssuerIdentifier", this.Model.ContentModel.IssuerIdentifier, new { size = "55" })%>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            Sign-in URL:
                                        </td>
                                        <td>
                                            <%: Html.TextBox("IssuerUrl", this.Model.ContentModel.IssuerUrl, new { size = "55" })%>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            Thumbprint:
                                        </td>
                                        <td>
                                            <%: Html.TextBox("IssuerThumbPrint", this.Model.ContentModel.IssuerThumbPrint, new { size = "55" })%>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            Admin Claim Type:
                                        </td>
                                        <td>
                                            <%: Html.TextBox("ClaimType", this.Model.ContentModel.ClaimType, new { size = "55" })%>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            Admin Claim Value:
                                        </td>
                                        <td>
                                            <%: Html.TextBox("ClaimValue", this.Model.ContentModel.ClaimValue, new { size = "55" })%>
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                            <u>The following data is optional if subscriber's subscription is <b>Premium</b> and want to extend the survey's model with a custom one</u>
                            <table class="configTable">
                                <tbody>
                                    <tr>
                                        <td>
                                            Assembly Name:
                                        </td>
                                        <td>
                                            <%: Html.TextBox("ModelExtensionAssembly", this.Model.ContentModel.ModelExtensionAssembly, new { size = "55" })%>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            Types Namespace:
                                        </td>
                                        <td>
                                            <%: Html.TextBox("ModelExtensionNamespace", this.Model.ContentModel.ModelExtensionNamespace, new { size = "55" })%>
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                    </div>
                    <div style="text-align: right; margin-top: 10px;">
                        <input type="submit" value="Add Tenant" />
                    </div>
                    <% } %>
                </div>
            </span>
        </div>
    </div>
</asp:Content>
