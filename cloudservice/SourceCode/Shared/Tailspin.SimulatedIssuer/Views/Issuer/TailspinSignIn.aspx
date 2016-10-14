<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<Tailspin.SimulatedIssuer.ViewModels.TailspinSignInViewModel>" %>

<%@ Import Namespace="System.Web.Mvc.Html"%>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceholder" runat="server">
<%using (Html.BeginForm()){ %>
    <div id="login">
        <p><%=this.Model.Domain%>'s users will be authenticated using Tailspin's registered members database.</p>
        <p>Please select a User to continue:</p>
        <div id="UserOptions">
            <input type="radio" checked="checked" /><%=this.Model.FullName%>
            <input type="hidden" name="Domain" value="<%=this.Model.Domain%>" />
            <input type="hidden" name="UserName" value="<%=this.Model.UserName%>" />
            <input type="hidden" name="SignInRequest" value="<%=this.Model.SignInRequest%>" />

        </div>
        <div style="text-align: right; margin-top: 10px;">
            <input type="submit" value="Continue with login..." />
        </div>
    </div>
    <%}%>
</asp:Content>
