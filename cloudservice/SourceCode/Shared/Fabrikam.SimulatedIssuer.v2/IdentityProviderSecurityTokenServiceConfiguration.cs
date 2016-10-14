﻿namespace Adatum.SimulatedIssuer.V2
{
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Configuration;
    using Fabrikam.SimulatedIssuer.V2;
    using Microsoft.IdentityModel.Configuration;
    using Microsoft.IdentityModel.SecurityTokenService;
    using Samples.Web.ClaimsUtillities;

    public class IdentityProviderSecurityTokenServiceConfiguration : SecurityTokenServiceConfiguration
    {
        private const string CustomSecurityTokenServiceConfigurationKey = "IdentityProviderSecurityTokenServiceConfigurationKey";
        private static readonly object SyncRoot = new object();

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public IdentityProviderSecurityTokenServiceConfiguration()
            : base(
                WebConfigurationManager.AppSettings[ApplicationSettingsNames.IssuerName],
                new X509SigningCredentials(
                    CertificateUtilities.GetCertificate(
                    StoreName.My,
                    StoreLocation.LocalMachine,
                    WebConfigurationManager.AppSettings[ApplicationSettingsNames.SigningCertificateName])))
        {
            this.SecurityTokenService = typeof(IdentityProviderSecurityTokenService);
        }

        public static IdentityProviderSecurityTokenServiceConfiguration Current
        {
            get
            {
                var httpAppState = HttpContext.Current.Application;

                var customConfiguration = httpAppState.Get(CustomSecurityTokenServiceConfigurationKey) as IdentityProviderSecurityTokenServiceConfiguration;

                if (customConfiguration == null)
                {
                    lock (SyncRoot)
                    {
                        customConfiguration = httpAppState.Get(CustomSecurityTokenServiceConfigurationKey) as IdentityProviderSecurityTokenServiceConfiguration;

                        if (customConfiguration == null)
                        {
                            customConfiguration = new IdentityProviderSecurityTokenServiceConfiguration();
                            httpAppState.Add(CustomSecurityTokenServiceConfigurationKey, customConfiguration);
                        }
                    }
                }

                return customConfiguration;
            }
        }
    }
}