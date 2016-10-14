namespace Tailspin.Web.Tests.Controllers
{
    using System.IO;
    using System.Web;
    using System.Web.Mvc;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Tailspin.Web.Controllers;
    using Tailspin.Web.Models;
    using Tailspin.Web.Survey.Shared.Models;
    using Tailspin.Web.Survey.Shared.Stores;

    [TestClass]
    public class AccountControllerFixture
    {
        [TestMethod]
        public void IndexReturnsEmptyViewName()
        {
            using (var controller = new AccountController(null))
            {
                var result = controller.Index() as ViewResult;

                Assert.AreEqual(string.Empty, result.ViewName);
            }
        }

        [TestMethod]
        public void IndexReturnsTitleInTheModel()
        {
            using (var controller = new AccountController(null))
            {
                var result = controller.Index() as ViewResult;

                var model = result.ViewData.Model as TenantMasterPageViewData;
                Assert.AreEqual("My Account", model.Title);
            }
        }

        [TestMethod]
        public void IndexReturnsTheTenantInTheModel()
        {
            using (var controller = new AccountController(null))
            {
                Tenant tenant = new Tenant();
                controller.Tenant = tenant;

                var result = controller.Index() as ViewResult;

                var model = result.ViewData.Model as TenantPageViewData<Tenant>;
                Assert.AreSame(tenant, model.ContentModel);
            }
        }

        [TestMethod]
        public void UploadLogoCallsTheStoreWithTheLogo()
        {
            var mockTenantStore = new Mock<ITenantStore>();
            var mockLogoFile = new Mock<HttpPostedFileBase>();
            
            var logoBytes = new byte[1];
            using (var stream = new MemoryStream(logoBytes))
            {
                mockLogoFile.Setup(f => f.ContentLength).Returns(1);
                mockLogoFile.Setup(f => f.InputStream).Returns(stream);

                using (var controller = new AccountController(mockTenantStore.Object))
                {
                    controller.UploadLogo("tenant", mockLogoFile.Object);
                }
            }

            mockTenantStore.Verify(r => r.UploadLogo("tenant", logoBytes), Times.Once());
        }

        [TestMethod]
        public void UploadLogoDoesNotCallTheStoreWhenContentLengthIs0()
        {
            var mockTenantStore = new Mock<ITenantStore>();
            var mockLogoFile = new Mock<HttpPostedFileBase>();
            mockLogoFile.Setup(f => f.ContentLength).Returns(0);

            using (var controller = new AccountController(mockTenantStore.Object))
            {
                controller.UploadLogo("tenant", mockLogoFile.Object);
            }

            mockTenantStore.Verify(r => r.UploadLogo(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Never());
        }

        [TestMethod]
        public void UploadLogoRedirectsToIndex()
        {
            var mockTenantStore = new Mock<ITenantStore>();

            using (var controller = new AccountController(mockTenantStore.Object))
            {
                var result = controller.UploadLogo("tenant", new Mock<HttpPostedFileBase>().Object) as RedirectToRouteResult;

                Assert.AreEqual("Index", result.RouteValues["action"]);
            }
        }
    }
}