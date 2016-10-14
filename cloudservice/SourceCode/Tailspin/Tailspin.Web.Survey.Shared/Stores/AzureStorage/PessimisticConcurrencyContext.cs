namespace Tailspin.Web.Survey.Shared.Stores.AzureStorage
{
    using System;

    public class PessimisticConcurrencyContext : IConcurrencyControlContext
    {
        public PessimisticConcurrencyContext()
        {
            this.Duration = TimeSpan.FromSeconds(30);
        }

        public string LockId { get; internal set; }

        public string ObjectId { get; set; }

        public TimeSpan Duration { get; set; }
    }
}
