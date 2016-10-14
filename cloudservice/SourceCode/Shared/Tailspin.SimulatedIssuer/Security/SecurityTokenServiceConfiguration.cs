namespace Tailspin.SimulatedIssuer.Security
{
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Configuration;
    using Microsoft.IdentityModel.Configuration;
    using Microsoft.IdentityModel.SecurityTokenService;
    using Samples.Web.ClaimsUtillities;

    public class SecurityTokenServiceConfiguration<T> : SecurityTokenServiceConfiguration where T : SecurityTokenService
    {
        private static readonly object SyncRoot = new object();

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public SecurityTokenServiceConfiguration()
            : base(
                WebConfigurationManager.AppSettings[ApplicationSettingsNames.IssuerName],
                new X509SigningCredentials(
                    CertificateUtilities.GetCertificate(
                    StoreName.My,
                    StoreLocation.LocalMachine,
                    WebConfigurationManager.AppSettings[ApplicationSettingsNames.SigningCertificateName])))
        {
            this.SecurityTokenService = typeof(T);
        }

        public static SecurityTokenServiceConfiguration<T> Current
        {
            get
            {
                var httpAppState = HttpContext.Current.Application;

                var keyName = typeof(T).Name + "_Configuration";

                var customConfiguration = httpAppState.Get(keyName) as SecurityTokenServiceConfiguration<T>;

                if (customConfiguration == null)
                {
                    lock (SyncRoot)
                    {
                        customConfiguration = httpAppState.Get(keyName) as SecurityTokenServiceConfiguration<T>;

                        if (customConfiguration == null)
                        {
                            customConfiguration = new SecurityTokenServiceConfiguration<T>();
                            httpAppState.Add(keyName, customConfiguration);
                        }
                    }
                }

                return customConfiguration;
            }
        }
    }
}