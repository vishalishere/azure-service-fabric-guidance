namespace Tailspin.Web.Controllers
{
    using System.Collections.Generic;
    using System.Web.Mvc;
    using Samples.Web.ClaimsUtillities;
    using Tailspin.Web.Models;
    using Tailspin.Web.Security;
    using Tailspin.Web.Survey.Shared.Models;
    using Tailspin.Web.Survey.Shared.Stores;
    
    [RequireHttps]
    [AuthenticateAndAuthorizeTailspinAttribute(Roles = Tailspin.Roles.TenantAdministrator)]
    public class ManagementController : Controller
    {
        private readonly ITenantStore tenantStore;

        public ManagementController(ITenantStore tenantStore)
        {
            this.tenantStore = tenantStore;
        }

        public ActionResult Index()
        {
            var model = new TenantPageViewData<IEnumerable<string>>(this.tenantStore.GetTenantNames())
            {
                Title = "Subscribers"
            };
            return this.View(model);
        }

        public ActionResult Detail(string tenant)
        {
            var contentModel = this.tenantStore.GetTenant(tenant);
            var model = new TenantPageViewData<Tenant>(contentModel)
            {
                Title = string.Format("{0} details", contentModel.Name)
            };
            return this.View(model);
        }

        public ActionResult New()
        {
            var model = new TenantPageViewData<Tenant>(new Tenant())
            {
                Title = "New Tenant"
            };
            return this.View(model);
        }

        [HttpPost]
        public ActionResult New(Tenant tenant)
        {
            if (string.IsNullOrWhiteSpace(tenant.Name))
            {
                var model = new TenantPageViewData<Tenant>(tenant)
                {
                    Title = "New Tenant : Error!"
                };
                this.ViewData["error"] = "Organization's name cannot be empty";
                return this.View(model);
            }
            else if (tenant.Name.Equals("new", System.StringComparison.InvariantCultureIgnoreCase))
            {
                var model = new TenantPageViewData<Tenant>(tenant)
                {
                    Title = "New Tenant : Error!"
                };
                this.ViewData["error"] = "Organization's name cannot be 'new'";
                return this.View(model);
            }

            // TODO: check if tenant already exist
            this.tenantStore.SaveTenant(tenant);

            return this.RedirectToAction("Index");
        }
    }
}
