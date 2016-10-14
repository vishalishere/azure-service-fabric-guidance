namespace Tailspin.Web.Survey.Shared.Stores
{
    using System.Collections.Generic;
    using Tailspin.Web.Survey.Shared.Models;

    public interface ITenantStore
    {
        void Initialize();
        Tenant GetTenant(string tenant);
        IEnumerable<string> GetTenantNames();
        void SaveTenant(Tenant tenant);
        void UploadLogo(string tenant, byte[] logo);
    }
}