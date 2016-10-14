namespace Fabrikam.SimulatedIssuer.V2
{
    using System;
    using System.Globalization;
    using System.Web.UI;
    using Adatum.SimulatedIssuer.V2;
    using Microsoft.IdentityModel.Protocols.WSFederation;
    using Microsoft.IdentityModel.Web;

    public partial class _Default : Page
    {
        protected override void OnPreRender(EventArgs e)
        {
            string action = Request.QueryString[WSFederationConstants.Parameters.Action];

            try
            {
                if (action == WSFederationConstants.Actions.SignIn)
                {
                    // Process signin request.
                    string endpointAddress = "~/SimulatedWindowsAuthentication.aspx";
                    Response.Redirect(endpointAddress + "?" + Request.QueryString, false);
                }
                else if (action == WSFederationConstants.Actions.SignOut)
                {
                    // Process signout request.
                    SimulatedWindowsAuthenticationOperations.LogOutUser(this.Request, this.Response);
                    SignOutRequestMessage requestMessage = (SignOutRequestMessage)WSFederationMessage.CreateFromUri(Request.Url);
                    FederatedPassiveSecurityTokenServiceOperations.ProcessSignOutRequest(requestMessage, this.User, null, this.Response);
                    this.ActionExplanationLabel.Text = "Sign out from the issuer has been requested.";
                }
                else
                {
                    throw new InvalidOperationException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "The action '{0}' (Request.QueryString['{1}']) is unexpected. Expected actions are: '{2}' or '{3}'.",
                            string.IsNullOrEmpty(action) ? "<EMPTY>" : action,
                            WSFederationConstants.Parameters.Action,
                            WSFederationConstants.Actions.SignIn,
                            WSFederationConstants.Actions.SignOut));
                }
            }
            catch (Exception exception)
            {
                throw new Exception("An unexpected error occurred when processing the request. See inner exception for details.", exception);
            }
        }
    }
}