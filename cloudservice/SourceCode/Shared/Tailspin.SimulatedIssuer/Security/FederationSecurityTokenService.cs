namespace Tailspin.SimulatedIssuer.Security
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using System.Web.Configuration;
    using Microsoft.IdentityModel.Claims;
    using Microsoft.IdentityModel.Configuration;
    using Microsoft.IdentityModel.Protocols.WSIdentity;
    using Microsoft.IdentityModel.Protocols.WSTrust;
    using Microsoft.IdentityModel.SecurityTokenService;
    using Microsoft.Practices.Unity;
    using Samples.Web.ClaimsUtillities;
    using Tailspin.Web.Survey.Shared.Stores;

    public class FederationSecurityTokenService : SecurityTokenService
    {
        private readonly ITenantStore tenantStore;

        public FederationSecurityTokenService(SecurityTokenServiceConfiguration configuration)
            : base(configuration)
        {
            using (var container = new UnityContainer())
            {
                ContainerBootstraper.RegisterTypes(container);
                this.tenantStore = container.Resolve<ITenantStore>();
            }
        }

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

            scope.TokenEncryptionRequired = false;

            return scope;
        }

        protected override IClaimsIdentity GetOutputClaimsIdentity(IClaimsPrincipal principal, RequestSecurityToken request, Scope scope)
        {
            if (principal == null)
            {
                throw new InvalidRequestException("The caller's principal is null.");
            }

            var input = principal.Identity as ClaimsIdentity;

            var tenant = this.tenantStore.GetTenant(input.Claims.First().Issuer);
            if (tenant == null)
            {
                throw new InvalidOperationException("Issuer not trusted.");
            }

            var output = new ClaimsIdentity();

            CopyClaims(input, new[] { WSIdentityConstants.ClaimTypes.Name }, output);
            TransformClaims(input, tenant.ClaimType, tenant.ClaimValue, ClaimTypes.Role, Tailspin.Roles.SurveyAdministrator, output);
            output.Claims.Add(new Claim(Tailspin.ClaimTypes.Tenant, tenant.Name));

            return output;
        }

        private static void TransformClaims(IClaimsIdentity input, string inputClaimType, string inputClaimValue, string outputClaimType, string outputClaimValue, IClaimsIdentity output)
        {
            var inputClaims = input.Claims.Where(c => c.ClaimType == inputClaimType);

            if ((inputClaimValue == "*") && (outputClaimValue == "*"))
            {
                var claimsToAdd = inputClaims.Select(c => new Claim(outputClaimType, c.Value));
                output.Claims.AddRange(claimsToAdd);
            }
            else
            {
                if (inputClaims.Count(c => c.Value == inputClaimValue) > 0)
                {
                    output.Claims.Add(new Claim(outputClaimType, outputClaimValue));
                }
            }
        }

        private static void CopyClaims(IClaimsIdentity input, IEnumerable<string> claimTypes, IClaimsIdentity output)
        {
            output.Claims.CopyRange(input.Claims.Where(c => claimTypes.Contains(c.ClaimType)));
        }
    }
}