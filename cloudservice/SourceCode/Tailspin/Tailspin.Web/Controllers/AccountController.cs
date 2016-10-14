namespace Tailspin.Web.Controllers
{
    using System;
    using System.IO;
    using System.Web;
    using System.Web.Mvc;
    using Samples.Web.ClaimsUtillities;
    using Tailspin.Web.Security;
    using Tailspin.Web.Survey.Shared.Stores;

    [RequireHttps]
    [AuthenticateAndAuthorizeTenant(Roles = Tailspin.Roles.SurveyAdministrator)]
    public class AccountController : TenantController 
    {
        public AccountController(ITenantStore tenantStore) : base(tenantStore)
        {
        }

        public ActionResult Index()
        {
            var model = this.CreateTenantPageViewData(this.Tenant);
            model.Title = "My Account";
            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadLogo(string tenant, HttpPostedFileBase newLogo)
        {
            // TODO: Validate that the file received is an image
            if (newLogo != null && newLogo.ContentLength > 0)
            {
                this.TenantStore.UploadLogo(tenant, new BinaryReader(newLogo.InputStream).ReadBytes(Convert.ToInt32(newLogo.InputStream.Length)));
            }

            return this.RedirectToAction("Index");
        }
    }
}