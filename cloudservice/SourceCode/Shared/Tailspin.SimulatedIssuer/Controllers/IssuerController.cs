namespace Tailspin.SimulatedIssuer.Controllers
{
    using System;
    using System.Globalization;
    using System.Web;
    using System.Web.Mvc;
    using Microsoft.IdentityModel.Protocols.WSFederation;
    using Microsoft.IdentityModel.SecurityTokenService;
    using Microsoft.IdentityModel.Web;
    using Microsoft.Security.Application;
    using Samples.Web.ClaimsUtillities;
    using Tailspin.SimulatedIssuer.Security;
    using Tailspin.SimulatedIssuer.ViewModels;
    using Tailspin.Web.Survey.Shared.Models;
    using Tailspin.Web.Survey.Shared.Stores;

    public class IssuerController : Controller
    {
        private readonly ITenantStore tenantStore;

        public IssuerController(ITenantStore tenantStore)
        {
            this.tenantStore = tenantStore;
        }

        public ActionResult Index()
        {
            string action = this.Request.QueryString[WSFederationConstants.Parameters.Action] ?? this.Request.Form[WSFederationConstants.Parameters.Action];

            try
            {
                if (action == WSFederationConstants.Actions.SignIn)
                {
                    return this.HandleSignInRequest();
                }
                else if (action == WSFederationConstants.Actions.SignOut)
                {
                    return this.HandleSignOutRequest();
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

        public ActionResult TailspinSignIn(string signInRequest)
        {
            // This simulates user authentication using Tailspin's registered members database
            var homeRealm = HttpUtility.ParseQueryString(signInRequest)["whr"];
            var user = Tailspin.Users.Administrator;
            var domain = Tailspin.Users.Domain;
            if (!homeRealm.Equals(Tailspin.Federation.HomeRealm))
            {
                domain = homeRealm.Substring(Tailspin.Federation.HomeRealm.Length + 1).ToUpperInvariant();
                user = AllOrganizations.Users.Administrator;
            }

            var model = new TailspinSignInViewModel()
            {
                Domain = domain,
                UserName = user,
                SignInRequest = signInRequest
            };

            return this.View(model);
        }
        
        [HttpPost]
        public ActionResult TailspinSignIn(TailspinSignInViewModel signInInfo)
        {
            var ctx = System.Web.HttpContext.Current;

            SimulatedWindowsAuthenticationOperations.LogOnUser(signInInfo.FullName, ctx, ctx.Request, ctx.Response);

            return this.HandleTailspinSignInResponse(signInInfo.FullName, new Uri(signInInfo.SignInRequest));
        }

        [ValidateInput(false)]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult SignInResponse()
        {
            if (this.User != null && this.User.Identity != null && this.User.Identity.IsAuthenticated)
            {
                var responseMessage =
                    WSFederationMessage.CreateFromFormPost(this.HttpContext.ApplicationInstance.Request);
                return this.HandleSignInResponse(responseMessage.Context);
            }

            throw new UnauthorizedAccessException();
        }

        // This method decides which role this issuer will be acting as when a sign-in request is received
        private ActionResult HandleSignInRequest()
        {
            var homeRealm = this.Request["whr"];

            // The issuer will act as an Identity Provider when the home realm is empty
            if (string.IsNullOrEmpty(homeRealm))
            {
                throw new ArgumentException("This issuer only acts as a Federation Provider. The whr parameter should be set to the identifier of the issuer you want to use.");
            }

            if (homeRealm.Equals(Tailspin.Federation.HomeRealm))
            {
                // The issuer will act as a STS for tailspin home realm users
                return this.HandleTailspinSignInRequest();
            }
            else if (homeRealm.StartsWith(Tailspin.Federation.HomeRealm))
            {
                // The issuer will act as a STS for tailspin home realm registered tenant's users
                return this.HandleTailspinSignInRequest();
            }
            else
            {
                // The issuer will act as a Federation Provider when the home realm has a value
                return this.HandleFederatedSignInRequest();
            }
        }

        private ActionResult HandleSignOutRequest()
        {
            SignOutRequestMessage requestMessage = (SignOutRequestMessage)WSFederationMessage.CreateFromUri(this.Request.Url);
            FederatedPassiveSecurityTokenServiceOperations.ProcessSignOutRequest(requestMessage, this.User, null, this.HttpContext.ApplicationInstance.Response);
            this.ViewData["ActionExplanation"] = "Sign out from the issuer has been requested.";
            this.ViewData["ReturnUrl"] = Encoder.HtmlAttributeEncode(this.Request.QueryString["wreply"]);
            return this.View();
        }

        private ActionResult HandleFederatedSignInRequest()
        {
            var tenant = this.GetTenantByIssuerIdentifier(this.Request["whr"]);

            if (tenant == null)
            {
                throw new InvalidOperationException("The home realm is not trusted for federation.");
            }
            
            var contextId = Guid.NewGuid().ToString();
            this.CreateContextCookie(contextId, this.Request.Url.ToString());

            var message = new SignInRequestMessage(new Uri(tenant.IssuerUrl), FederatedAuthentication.WSFederationAuthenticationModule.Realm)
            {
                CurrentTime = DateTime.UtcNow.ToString("s", CultureInfo.InvariantCulture) + "Z",
                Reply = FederatedAuthentication.WSFederationAuthenticationModule.Realm,
                Context = contextId
            };

            return this.Redirect(message.RequestUrl);
        }

        private ActionResult HandleTailspinSignInRequest()
        {
            var ctx = System.Web.HttpContext.Current;

            string authenticatedUser;
            if (!SimulatedWindowsAuthenticationOperations.TryToAuthenticateUser(ctx, ctx.Request, ctx.Response, out authenticatedUser))
            {
                return this.RedirectToAction("TailspinSignIn", new { SignInRequest = ctx.Request.Url.ToString() });
            }

            return this.HandleTailspinSignInResponse(authenticatedUser, ctx.Request.Url);
        }

        private ActionResult HandleTailspinSignInResponse(string userNameToValidate, Uri originalRequestUrl)
        {
            var ctx = System.Web.HttpContext.Current;

            SignInRequestMessage requestMessage = (SignInRequestMessage)WSFederationMessage.CreateFromUri(originalRequestUrl);
            SecurityTokenService sts = new IdentityProviderSecurityTokenService(SecurityTokenServiceConfiguration<IdentityProviderSecurityTokenService>.Current)
            {
                CustomUserName = userNameToValidate
            };
            SignInResponseMessage responseMessage = FederatedPassiveSecurityTokenServiceOperations.ProcessSignInRequest(requestMessage, this.User, sts);
            FederatedPassiveSecurityTokenServiceOperations.ProcessSignInResponse(responseMessage, ctx.Response);

            return this.Content(responseMessage.WriteFormPost());
        }

        private ActionResult HandleSignInResponse(string contextId)
        {
            var ctxCookie = this.Request.Cookies[contextId];
            if (ctxCookie == null)
            {
                throw new InvalidOperationException("Context cookie not found");
            }

            var originalRequestUri = new Uri(ctxCookie.Value);
            this.DeleteContextCookie(contextId);

            SignInRequestMessage requestMessage = (SignInRequestMessage)WSFederationMessage.CreateFromUri(originalRequestUri);

            SecurityTokenService sts = new FederationSecurityTokenService(SecurityTokenServiceConfiguration<FederationSecurityTokenService>.Current);
            SignInResponseMessage responseMessage = FederatedPassiveSecurityTokenServiceOperations.ProcessSignInRequest(requestMessage, User, sts);
            FederatedPassiveSecurityTokenServiceOperations.ProcessSignInResponse(responseMessage, this.HttpContext.ApplicationInstance.Response);

            return this.Content(responseMessage.WriteFormPost());
        }

        private void CreateContextCookie(string contextId, string context)
        {
            var contextCookie = new HttpCookie(contextId, context)
            {
                Secure = FederatedAuthentication.SessionAuthenticationModule.CookieHandler.RequireSsl,
                Path = FederatedAuthentication.SessionAuthenticationModule.CookieHandler.Path,
                Domain = FederatedAuthentication.SessionAuthenticationModule.CookieHandler.Domain,
                HttpOnly = FederatedAuthentication.SessionAuthenticationModule.CookieHandler.HideFromClientScript
            };

            TimeSpan? lifetime = FederatedAuthentication.SessionAuthenticationModule.CookieHandler.PersistentSessionLifetime;
            if (lifetime.HasValue)
            {
                contextCookie.Expires = DateTime.UtcNow.Add(lifetime.Value);
            }

            Response.Cookies.Add(contextCookie);
        }

        private void DeleteContextCookie(string contextId)
        {
            var contextCookie = new HttpCookie(contextId)
            {
                Secure = FederatedAuthentication.SessionAuthenticationModule.CookieHandler.RequireSsl,
                Path = FederatedAuthentication.SessionAuthenticationModule.CookieHandler.Path,
                Domain = FederatedAuthentication.SessionAuthenticationModule.CookieHandler.Domain,
                HttpOnly = FederatedAuthentication.SessionAuthenticationModule.CookieHandler.HideFromClientScript
            };

            contextCookie.Expires = DateTime.UtcNow.AddDays(-1);

            Response.Cookies.Add(contextCookie);
        }

        private Tenant GetTenantByIssuerIdentifier(string issuerIdentifier)
        {
            foreach (var tenantName in this.tenantStore.GetTenantNames())
            {
                var tenant = this.tenantStore.GetTenant(tenantName);
                if (tenant.IssuerIdentifier.Equals(issuerIdentifier, StringComparison.InvariantCultureIgnoreCase))
                {
                    return tenant;
                }
            }

            return null;
        }
    }
}
