namespace Tailspin.Web.Tests.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Tailspin.Web.Controllers;
    using Tailspin.Web.Models;
    using Tailspin.Web.Survey.Shared.Models;
    using Tailspin.Web.Survey.Shared.Stores;

    [TestClass]
    public class OnBoardingControllerFixture
    {
        [TestMethod]
        public void IndexReturnsTitleInTheModel()
        {
            var tenantStoreMock = new Mock<ITenantStore>();
            tenantStoreMock.Setup(t => t.GetTenantNames()).Returns(new List<string>() { "t1", "t2", "t3" });
            tenantStoreMock.Setup(t => t.GetTenant(It.IsAny<string>())).Returns<string>(name => new Tenant { Name = name });

            using (var controller = new OnBoardingController(tenantStoreMock.Object))
            {
                var result = controller.Index() as ViewResult;
                var model = result.ViewData.Model as TenantPageViewData<IEnumerable<Tenant>>;

                Assert.AreEqual("On boarding", model.Title);
                Assert.AreEqual(3, model.ContentModel.Count());
                Assert.IsTrue(model.ContentModel.Select(t => t.Name).Contains("t1"));
                Assert.IsTrue(model.ContentModel.Select(t => t.Name).Contains("t2"));
                Assert.IsTrue(model.ContentModel.Select(t => t.Name).Contains("t3"));
            }

            tenantStoreMock.Verify(t => t.GetTenantNames(), Times.Once());
            tenantStoreMock.Verify(t => t.GetTenant(It.IsAny<string>()), Times.Exactly(3));
        }
    }
}
