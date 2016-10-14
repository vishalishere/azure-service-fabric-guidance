namespace Tailspin.Web.Survey.Shared.Helpers
{
    using System;
    using System.Linq;
    using Microsoft.ApplicationServer.Caching;

    internal static class TenantCacheHelper
    {
        internal static void AddToCache<T>(string tenant, string key, T @object) where T : class
        {
            using (var factory = new DataCacheFactory())
            {
                DataCache cache = factory.GetDefaultCache();
                if (!cache.GetSystemRegions().Contains(tenant.ToLowerInvariant()))
                {
                    cache.CreateRegion(tenant.ToLowerInvariant());
                }
                cache.Put(key.ToLowerInvariant(), @object, tenant.ToLowerInvariant());
            }
        }

        internal static T GetFromCache<T>(string tenant, string key, Func<T> @default) where T : class
        {
            var result = default(T);

            using (var factory = new DataCacheFactory())
            {
                var success = false;
                DataCache cache = factory.GetDefaultCache();
                result = cache.Get(key.ToLowerInvariant(), tenant.ToLowerInvariant()) as T;
                if (result != null)
                {
                    success = true;
                }
                else if (@default != null)
                {
                    result = @default();
                    if (result != null)
                    {
                        AddToCache(tenant.ToLowerInvariant(), key.ToLowerInvariant(), result);
                    }
                }
                TraceHelper.TraceInformation("cache {2} for {0} [{1}]", key, tenant, success ? "hit" : "miss");
            }

            return result;
        }

        internal static void RemoveFromCache(string tenant, string key)
        {
            using (var factory = new DataCacheFactory())
            {
                DataCache cache = factory.GetDefaultCache();
                cache.Remove(key.ToLowerInvariant(), tenant.ToLowerInvariant());
            }
        }
    }
}
