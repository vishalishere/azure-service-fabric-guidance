namespace Tailspin.SimulatedIssuer
{
    using System.Diagnostics.CodeAnalysis;
    using System.Web.Mvc;
    using System.Web.Routing;
    using Microsoft.Practices.Unity;
    using Tailspin.Web.Controllers;

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("*.svc");
            routes.IgnoreRoute("favicon.ico");
            routes.IgnoreRoute("*.htm");

            routes.MapRoute(
                "TailspinSignIn",
                "TailspinSignIn",
                new { controller = "Issuer", action = "TailspinSignIn" });

            routes.MapRoute(
                "SignInResponse",
                "SignInResponse",
                new { controller = "Issuer", action = "SignInResponse" });
            
            routes.MapRoute(
                "Default",
                string.Empty,
                new { controller = "Issuer", action = "Index" });
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Microsoft.DisposeObjectsBeforeLosingScope", Justification = "This container is used in the controller factory and cannot be disposed.")]
        protected void Application_Start()
        {
            var container = new UnityContainer();
            ContainerBootstraper.RegisterTypes(container);
            ControllerBuilder.Current.SetControllerFactory(new UnityControllerFactory(container));

            RegisterRoutes(RouteTable.Routes);
        }
    }
}