namespace Tailspin.Web.Security
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Web.Mvc;
    using System.Web.Routing;
    using Microsoft.IdentityModel.Protocols.WSFederation;

    public abstract class AuthenticateAndAuthorizeRoleAttribute : FilterAttribute, IAuthorizationFilter
    {
        protected enum RequestAppendAttribute
        {
            RawUrl,
            ApplicationPath
        }

        public string Roles { get; set; }

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.Request.IsSecureConnection)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentUICulture, "https is required to browse the page: '{0}'.", filterContext.HttpContext.Request.Url.AbsoluteUri));
            }

            this.PreProcess(filterContext);

            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                this.AuthenticateUser(filterContext);
            }
            else
            {
                this.AuthorizeUser(filterContext);
            }

            this.PostProcess(filterContext);
        }

        protected static Uri GetReturnUrl(RequestContext context, RequestAppendAttribute appendAttribute, string endSubPath)
        {
            // In the Windows Azure environment, build a wreply parameter for  the SignIn request
            // that reflects the real address of the application.  
            var request = context.HttpContext.Request;
            var reqUrl = request.Url;
            var wreply = new StringBuilder();

            wreply.Append(reqUrl.Scheme);     // e.g. "http"
            wreply.Append("://");
            wreply.Append(request.Headers["Host"] ?? reqUrl.Authority);
            wreply.Append(RequestAppendAttribute.RawUrl.Equals(appendAttribute) ? request.RawUrl : request.ApplicationPath);

            if (!request.ApplicationPath.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                wreply.Append("/");
            }

            if (!string.IsNullOrWhiteSpace(endSubPath))
            {
                wreply.Append(endSubPath);
            }

            return new Uri(wreply.ToString());
        }

        protected virtual void PreProcess(AuthorizationContext context)
        {
        }

        protected virtual void PostProcess(AuthorizationContext context)
        {
        }

        protected virtual void AuthenticateUser(AuthorizationContext context)
        {
            // user is not authenticated and it's entering for the first time
            context.Result = new RedirectResult(
                this.BuildSignInMessage(
                    context,
                    new Uri(GetReturnUrl(context.RequestContext, RequestAppendAttribute.ApplicationPath, "FederationResult").ToString())).WriteQueryString());
        }

        protected virtual void AuthorizeUser(AuthorizationContext context)
        {
            var authorizedRoles = this.Roles.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            bool hasValidRole = false;
            foreach (var role in authorizedRoles)
            {
                if (context.HttpContext.User.IsInRole(role.Trim()))
                {
                    hasValidRole = true;
                    break;
                }
            }

            if (!hasValidRole)
            {
                context.Result = new HttpUnauthorizedResult();
                return;
            }
        }

        protected abstract WSFederationMessage BuildSignInMessage(AuthorizationContext context, Uri replyUrl);
    }
}