namespace Tailspin.SimulatedIssuer.Security
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using System.Web.Configuration;
    using Microsoft.IdentityModel.Claims;
    using Microsoft.IdentityModel.Configuration;
    using Microsoft.IdentityModel.Protocols.WSTrust;
    using Microsoft.IdentityModel.SecurityTokenService;
    using Samples.Web.ClaimsUtillities;

    public class IdentityProviderSecurityTokenService : SecurityTokenService
    {
        public IdentityProviderSecurityTokenService(SecurityTokenServiceConfiguration configuration)
            : base(configuration)
        {
        }

        public string CustomUserName { get; set; }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override Scope GetScope(IClaimsPrincipal principal, RequestSecurityToken request)
        {
            Scope scope = new Scope(request.AppliesTo.Uri.AbsoluteUri, SecurityTokenServiceConfiguration.SigningCredentials);

            string encryptingCertificateName = WebConfigurationManager.AppSettings[ApplicationSettingsNames.EncryptingCertificateName];
            if (!string.IsNullOrEmpty(encryptingCertificateName))
            {
                scope.EncryptingCredentials = new X509EncryptingCredentials(CertificateUtilities.GetCertificate(StoreName.My, StoreLocation.LocalMachine, encryptingCertificateName));
            }
            else
            {
                scope.TokenEncryptionRequired = false;
            }

            if (!string.IsNullOrEmpty(request.ReplyTo))
            {
                scope.ReplyToAddress = request.ReplyTo;
            }
            else
            {
                scope.ReplyToAddress = scope.AppliesToAddress;
            }
                        
            return scope;
        }

        protected override IClaimsIdentity GetOutputClaimsIdentity(IClaimsPrincipal principal, RequestSecurityToken request, Scope scope)
        {
            var outputIdentity = new ClaimsIdentity();

            if (null == principal)
            {
                throw new InvalidRequestException("The caller's principal is null.");
            }

            switch (principal.Identity.Name)
            {
                // In a production environment, all the information that will be added
                // as claims should be read from the authenticated Windows Principal.
                // The following lines are hardcoded because windows integrated 
                // authentication is disabled.
                case Tailspin.Users.FullName:
                    outputIdentity.Claims.AddRange(new List<Claim>
                       {
                           new Claim(ClaimTypes.Name, Tailspin.Users.FullName), 
                           new Claim(ClaimTypes.GivenName, "Robert"),
                           new Claim(ClaimTypes.Surname, "Roe"),
                           new Claim(ClaimTypes.Role, Tailspin.Roles.TenantAdministrator)
                       });
                    break;

                default:
                    if (!principal.Identity.Name.Equals(this.CustomUserName, System.StringComparison.InvariantCultureIgnoreCase))
                    {
                        throw new InvalidOperationException(string.Format("Cannot get claims for {0} - not authorized", principal.Identity.Name));
                    }

                    var tenantName = principal.Identity.Name.Split('\\')[0];

                    outputIdentity.Claims.AddRange(new List<Claim>
                       {
                           new Claim(ClaimTypes.Name, principal.Identity.Name), 
                           new Claim(ClaimTypes.GivenName, tenantName + " Jr."),
                           new Claim(ClaimTypes.Surname, "John"),
                           new Claim(ClaimTypes.Role, Tailspin.Roles.SurveyAdministrator),
                           new Claim(Tailspin.ClaimTypes.Tenant, tenantName)
                       });

                    break;
            }

            return outputIdentity;
        }
    }
}
