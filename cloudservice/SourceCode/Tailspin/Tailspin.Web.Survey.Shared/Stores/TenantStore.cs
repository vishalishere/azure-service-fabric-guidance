namespace Tailspin.Web.Survey.Shared.Stores
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Tailspin.Web.Survey.Shared.Helpers;
    using Tailspin.Web.Survey.Shared.Models;
    using Tailspin.Web.Survey.Shared.Stores.AzureStorage;

    public class TenantStore : ITenantStore
    {
        private const string TenantAccountTag = "_accountinfo_";

        private readonly IAzureBlobContainer<Tenant> tenantBlobContainer;
        private readonly IAzureBlobContainer<byte[]> logosBlobContainer;

        public TenantStore(IAzureBlobContainer<Tenant> tenantBlobContainer, IAzureBlobContainer<byte[]> logosBlobContainer)
        {
            this.tenantBlobContainer = tenantBlobContainer;
            this.logosBlobContainer = logosBlobContainer;
            this.CacheEnabled = false;
        }

        public bool CacheEnabled { get; set; }

        public void Initialize()
        {
            this.logosBlobContainer.EnsureExist();

            this.tenantBlobContainer.EnsureExist();

            // This initialization method provisions two tenants for sample purposes. 
            // The following code will not be present on the real repository.
            if (this.GetTenant("adatum") == null)
            {
                this.SaveTenant(new Tenant
                                    {
                                        Name = "Adatum",
                                        HostGeoLocation = "Anywhere US",
                                        WelcomeText = "Adatum company has already been configured to authenticate using Adatum's issuer",
                                        SubscriptionKind = Models.SubscriptionKind.Premium,
                                        ModelExtensionAssembly = "Adatum.ModelExtensions",
                                        ModelExtensionNamespace = "Adatum.ModelExtensions",
                                        IssuerIdentifier = "http://adatum/trust",
                                        IssuerUrl = "https://localhost/Adatum.SimulatedIssuer.v2/",
                                        IssuerThumbPrint = "f260042d59e14817984c6183fbc6bfc71baf5462",
                                        ClaimType = "http://schemas.xmlsoap.org/claims/group",
                                        ClaimValue = "Marketing Managers",
                                        SqlAzureConnectionString =
                                            @"Server=tcp:YourDBServerName.database.windows.net,1433;Database=adatum-survey;User ID=YourUserID@YourDBServerName;Password=YourPassword;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;",
                                        DatabaseName = "adatum-survey.database.windows.net",
                                        DatabaseUserName = "adatumuser",
                                        DatabasePassword = "SecretPassword"
                                    });
            }

            if (this.GetTenant("fabrikam") == null)
            {
                this.SaveTenant(new Tenant
                                    {
                                        Name = "Fabrikam",
                                        HostGeoLocation = "Anywhere US",
                                        WelcomeText = "Fabrikam company has already been configured to authenticate using Fabrikam's issuer",
                                        SubscriptionKind = Models.SubscriptionKind.Premium,
                                        ModelExtensionAssembly = "Fabrikam.ModelExtensions",
                                        ModelExtensionNamespace = "Fabrikam.ModelExtensions",
                                        IssuerIdentifier = "http://fabrikam/trust",
                                        IssuerUrl = "https://localhost/Fabrikam.SimulatedIssuer.v2/",
                                        IssuerThumbPrint = "d2316a731b59683e744109278c80e2614503b17e",
                                        ClaimType = "http://schemas.xmlsoap.org/claims/group",
                                        ClaimValue = "Marketing Managers",
                                        SqlAzureConnectionString = string.Empty
                                    });
            }
        }

        public Tenant GetTenant(string tenant)
        {
            Func<Tenant> resolver = () => this.tenantBlobContainer.Get(tenant.ToLowerInvariant());
            return this.CacheEnabled ? TenantCacheHelper.GetFromCache(tenant, TenantAccountTag, resolver) : resolver();
        }

        public IEnumerable<string> GetTenantNames()
        {
            return this.tenantBlobContainer.GetBlobList().Select(b => b.Name);
        }

        public void SaveTenant(Tenant tenant)
        {
            this.tenantBlobContainer.Save(tenant.Name.ToLowerInvariant(), tenant);

            if (this.CacheEnabled)
            {
                TenantCacheHelper.AddToCache(tenant.Name, TenantAccountTag, tenant);
            }
        }

        public void UploadLogo(string tenant, byte[] logo)
        {
            this.logosBlobContainer.Save(tenant, logo);

            var tenantToUpdate = this.tenantBlobContainer.Get(tenant);
            tenantToUpdate.Logo = this.logosBlobContainer.GetUri(tenant).ToString();

            this.SaveTenant(tenantToUpdate);
        }
    }
}