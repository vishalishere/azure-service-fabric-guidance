namespace Samples.Web.ClaimsUtillities
{
    public static class Tailspin
    {
        public static string TenantName
        {
            get
            {
                return "Tailspin";
            }
        }

        public static class ClaimTypes
        {
            public const string Tenant = "http://schemas.tailspin.com/claims/2010/06/tenant";
        }

        public static class Federation
        {
            public const string HomeRealm = "http://tailspin/trust";
        }

        public static class Roles
        {
            public const string SurveyAdministrator = "Survey Administrator";
            public const string TenantAdministrator = "Tenant Administrator";
        }

        public static class Users
        {
            public const string Domain = "TAILSPIN";
            public const string Administrator = "Administrator";
            public const string FullName = "TAILSPIN\\Administrator";
        }
    }
}