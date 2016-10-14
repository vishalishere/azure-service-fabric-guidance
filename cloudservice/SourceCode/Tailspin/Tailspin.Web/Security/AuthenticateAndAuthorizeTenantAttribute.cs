namespace Tailspin.Web.Security
{
    using System;
    using System.Globalization;
    using System.Web.Mvc;
    using Microsoft.IdentityModel.Protocols.WSFederation;
    using Microsoft.IdentityModel.Web;
    using Samples.Web.ClaimsUtillities;
    using Tailspin.Web.Controllers;
    using Tailspin.Web.Survey.Shared.Models;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class AuthenticateAndAuthorizeTenantAttribute : AuthenticateAndAuthorizeRoleAttribute
    {
        protected override void PreProcess(AuthorizationContext context)
        {
            if ((context.Controller as TenantController) == null)
            {
                throw new NotSupportedException("The AuthenticateAndAuthorize attribute can only be used in controllers that inherit from TenantController.");
            }
            var tenantController = context.Controller as TenantController;

            var tenantName = (string)context.RouteData.Values["tenant"];
            var tenant = tenantController.TenantStore.GetTenant(tenantName);
            if (tenant == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, "'{0}' is not a valid tenant.", tenantName));
            }

            tenantController.Tenant = tenant;
        }

        protected override WSFederationMessage BuildSignInMessage(AuthorizationContext context, Uri replyUrl)
        {
            var tenant = (context.Controller as TenantController).Tenant;

            var fam = FederatedAuthentication.WSFederationAuthenticationModule;
            var signIn = new SignInRequestMessage(new Uri(fam.Issuer), fam.Realm)
            {
                Context = AuthenticateAndAuthorizeRoleAttribute.GetReturnUrl(context.RequestContext, RequestAppendAttribute.RawUrl, null).ToString(),
                HomeRealm = SubscriptionKind.Premium.Equals(tenant.SubscriptionKind)
                    ? tenant.IssuerIdentifier ?? Tailspin.Federation.HomeRealm + "/" + (context.Controller as TenantController).Tenant.Name
                    : Tailspin.Federation.HomeRealm + "/" + (context.Controller as TenantController).Tenant.Name,
                Reply = replyUrl.ToString()
            };

            return signIn;
        }

        protected override void AuthorizeUser(AuthorizationContext context)
        {
            var tenantRequested = (string)context.RouteData.Values["tenant"];
            var userTenant = ClaimHelper.GetCurrentUserClaim(Tailspin.ClaimTypes.Tenant).Value;
            if (!tenantRequested.Equals(userTenant, StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new HttpUnauthorizedResult();
                return;
            }

            base.AuthorizeUser(context);
        }
    }
}
