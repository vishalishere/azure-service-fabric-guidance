namespace Tailspin.Web
{
    using System.Web.Mvc;
    using System.Web.Routing;

    public static class AppRoutes
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute(
                "OnBoarding",
                string.Empty,
                new { controller = "OnBoarding", action = "Index" });

            routes.MapRoute(
                "Management",
                "Management",
                new { controller = "Management", action = "Index" });

            routes.MapRoute(
                "Management-New",
                "Management/new",
                new { controller = "Management", action = "New" });

            routes.MapRoute(                
                "Management-Detail",        
                "Management/{tenant}",
                new { controller = "Management", action = "Detail" });

            routes.MapRoute(
               "JoinTenant",
               "Join",
               new { controller = "OnBoarding", action = "Join" });

            routes.MapRoute(
                "FederationResultProcessing",
                "FederationResult",
                new { controller = "ClaimsAuthentication", action = "FederationResult" });

            routes.MapRoute(
                "FederatedSignout",
                "Signout",
                new { controller = "ClaimsAuthentication", action = "Signout" });

            routes.MapRoute(
                "MyAccount",
                "{tenant}/MyAccount",
                new { controller = "Account", action = "Index" });

            routes.MapRoute(
                "UploadLogo",
                "{tenant}/MyAccount/UploadLogo",
                new { controller = "Account", action = "UploadLogo" });
        }
    }
}
